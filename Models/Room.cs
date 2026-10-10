using System.ComponentModel.DataAnnotations;

namespace TestApplication.Models;

public class Room
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int RoomNumber { get; set; }

    [Required, StringLength(100)]
    public string RoomType { get; set; } = string.Empty;
}
