using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Server.Auth;
using NovaCRM.Server.Contracts;
using NovaCRM.Server.Domain;

namespace NovaCRM.Server.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegistrationService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
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
                Country = country,
                Timezone = timezone,
                Currency = "USD",
                Phone = companyPhone,
                Email = ownerEmail,
                BusinessId = businessId,
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

            var user = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail
            };

            var identityResult = await _userManager.CreateAsync(user, request.OwnerPassword);
            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return (false, null, identityResult.Errors.Select(e => e.Description));
            }

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                BranchId = branch.Id,
                UserId = user.Id,
                FirstName = "Owner",
                LastName = companyName,
                RoleTitle = "Owner",
                IsActive = true
            };

            await _dbContext.StaffMembers.AddAsync(staff, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var ownerRoles = new[] { "Owner", "Admin" };
            foreach (var role in ownerRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, role);
                if (!addToRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return (false, null, addToRoleResult.Errors.Select(e => e.Description));
                }
            }

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
