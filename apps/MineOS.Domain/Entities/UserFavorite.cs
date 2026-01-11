namespace MineOS.Domain.Entities;

public class UserFavorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string ServerName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
