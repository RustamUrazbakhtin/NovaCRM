namespace NovaCRM.Data.Model;

public partial class ClientTagLink
{
    public Guid ClientId { get; set; }
    public Guid TagId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public ClientTag Tag { get; set; } = null!;
}
