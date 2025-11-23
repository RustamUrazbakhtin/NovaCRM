using System.ComponentModel.DataAnnotations;

namespace NovaCRM.Server.Contracts;

public class RegistrationRequest
{
    [Required]
    public string CompanyName { get; set; } = null!;

    [Required]
    public string Country { get; set; } = null!;

    [Required]
    public string Timezone { get; set; } = null!;

    [Required]
    public string CompanyPhone { get; set; } = null!;

    [Required]
    public string OwnerFirstName { get; set; } = null!;

    [Required]
    public string OwnerLastName { get; set; } = null!;

    public DateTime? OwnerBirthday { get; set; }

    [Required]
    [EmailAddress]
    public string OwnerEmail { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string OwnerPassword { get; set; } = null!;

    [Required]
    [Compare(nameof(OwnerPassword), ErrorMessage = "Passwords do not match.")]
    public string OwnerPasswordRepeat { get; set; } = null!;

    public string? BusinessId { get; set; }
}
