using System.ComponentModel.DataAnnotations;

namespace SupportWebApp.Models;

public class SupportMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Navn skal udfyldes")]
    [StringLength(100, ErrorMessage = "Maks. 100 tegn")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email skal udfyldes")]
    [EmailAddress(ErrorMessage = "Ugyldig emailadresse")]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "Ugyldigt telefonnummer")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Vælg en kategori")]
    public string Category { get; set; } = "";   // partition key

    [Required(ErrorMessage = "Beskrivelse skal udfyldes")]
    [MinLength(10, ErrorMessage = "Skriv mindst 10 tegn")]
    [StringLength(2000, ErrorMessage = "Maks. 2000 tegn")]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}