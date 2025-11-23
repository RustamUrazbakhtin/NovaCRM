using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Server.Contracts;
using NovaCRM.Data;
using NovaCRM.Data.Model;

namespace NovaCRM.Server.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<AspNetUser> _passwordHasher;

    public RegistrationService(
        ApplicationDbContext dbContext,
        IPasswordHasher<AspNetUser> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, RegistrationResponse? Response, IEnumerable<string> Errors)> RegisterOrganizationAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Any())
        {
            return (false, null, validationErrors);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var companyName = request.CompanyName.Trim();
            var country = request.Country.Trim();
            var timezone = request.Timezone.Trim();
            var companyPhone = request.CompanyPhone.Trim();
            var ownerEmail = request.OwnerEmail.Trim();
            var businessId = string.IsNullOrWhiteSpace(request.BusinessId)
                ? null
                : request.BusinessId.Trim();

            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                Timezone = timezone,
                Currency = "USD",
                Phone = companyPhone,
                Email = ownerEmail,
                PlanType = "free",
                SubscriptionStatus = "active"
            };

            await _dbContext.Organizations.AddAsync(organization, cancellationToken);

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Name = "Main location",
                Address = null,
                City = null,
                Country = country,
                Timezone = timezone,
                Phone = companyPhone,
                IsDefault = true
            };

            await _dbContext.Branches.AddAsync(branch, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var normalizedEmail = ownerEmail.ToUpperInvariant();

            var existingUser = await _dbContext.AspNetUsers
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail || u.NormalizedUserName == normalizedEmail, cancellationToken);
            if (existingUser)
            {
                await transaction.RollbackAsync(cancellationToken);
                return (false, null, new[] { "A user with this email already exists." });
            }

            var user = new AspNetUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = ownerEmail,
                NormalizedUserName = normalizedEmail,
                Email = ownerEmail,
                NormalizedEmail = normalizedEmail,
                EmailConfirmed = false,
                PhoneNumber = companyPhone,
                PhoneNumberConfirmed = false,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                SecurityStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = true,
                AccessFailedCount = 0
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.OwnerPassword);

            await _dbContext.AspNetUsers.AddAsync(user, cancellationToken);

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                BranchId = branch.Id,
                UserId = user.Id,
                FirstName = ownerEmail,
                LastName = string.Empty,
                RoleTitle = "Owner",
                IsActive = true,
                Phone = companyPhone,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Staff.AddAsync(staff, cancellationToken);

            var ownerRoles = new[] { "Owner", "Admin" };
            var availableRoles = await _dbContext.AspNetRoles
                .Where(role => ownerRoles.Contains(role.Name!))
                .ToListAsync(cancellationToken);

            foreach (var role in availableRoles)
            {
                user.Roles.Add(role);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = new RegistrationResponse(organization.Id, branch.Id, user.Id);
            return (true, response, Array.Empty<string>());
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static IEnumerable<string> Validate(RegistrationRequest request)
    {
        var errors = new List<string>();

        void Require(string? value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(message);
            }
        }

        Require(request.CompanyName, "Company name is required.");
        Require(request.Country, "Country is required.");
        Require(request.Timezone, "Timezone is required.");
        Require(request.CompanyPhone, "Company phone is required.");
        Require(request.OwnerEmail, "Owner email is required.");
        Require(request.OwnerPassword, "Owner password is required.");
        Require(request.OwnerPasswordRepeat, "Please confirm the owner password.");

        if (!string.Equals(request.OwnerPassword, request.OwnerPasswordRepeat, StringComparison.Ordinal))
        {
            errors.Add("Passwords do not match.");
        }

        return errors;
    }
}
