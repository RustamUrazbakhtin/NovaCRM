using System.ComponentModel.DataAnnotations;

namespace NovaCRM.Server.Contracts;

public class RegistrationRequest
{
    [Required]
    public string CompanyName { get; set; } = null!;

    public string? Industry { get; set; }

    [Required]
    public string Country { get; set; } = null!;

    [Required]
    public string Timezone { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string BusinessEmail { get; set; } = null!;

    [Phone]
    public string? CompanyPhone { get; set; }

    public string? BranchName { get; set; }

    public string? BranchAddress { get; set; }

    public string? BranchCity { get; set; }

    [Required]
    public string OwnerFullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string OwnerEmail { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string OwnerPassword { get; set; } = null!;
}
