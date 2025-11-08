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

    public RegistrationService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<(bool Success, RegistrationResponse? Response, IEnumerable<string> Errors)> RegisterOrganizationAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.CompanyName,
                Industry = request.Industry,
                Country = request.Country,
                Timezone = request.Timezone,
                Currency = "USD",
                Phone = request.CompanyPhone,
                Email = request.BusinessEmail,
                PlanType = "free",
                SubscriptionStatus = "active"
            };

            await _dbContext.Organizations.AddAsync(organization, cancellationToken);

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Name = string.IsNullOrWhiteSpace(request.BranchName) ? "Main location" : request.BranchName!,
                Address = request.BranchAddress,
                City = request.BranchCity,
                Country = request.Country,
                Timezone = request.Timezone,
                Phone = request.CompanyPhone,
                IsDefault = true
            };

            await _dbContext.Branches.AddAsync(branch, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var user = new ApplicationUser
            {
                UserName = request.OwnerEmail,
                Email = request.OwnerEmail
            };

            var identityResult = await _userManager.CreateAsync(user, request.OwnerPassword);
            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return (false, null, identityResult.Errors.Select(e => e.Description));
            }

            var (firstName, lastName) = SplitName(request.OwnerFullName);

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                BranchId = branch.Id,
                UserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                RoleTitle = "Owner",
                IsActive = true
            };

            await _dbContext.StaffMembers.AddAsync(staff, cancellationToken);
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

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ("", "");
        }

        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return ("", "");
        }

        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        var firstName = parts[0];
        var lastName = string.Join(" ", parts.Skip(1));
        return (firstName, lastName);
    }
}
