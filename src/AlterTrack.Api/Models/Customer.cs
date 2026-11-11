namespace AlterTrack.Api.Models;

public class Customer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string EmployeeNumber {get; set;}

    public required string Phone { get; set; }

    public string? Email { get; set; }

    public string? Notes { get; set; }

    /// <summary>Stored in UTC; convert to local time only for display.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
