namespace MineOS.Application.Options;

public sealed class CurseForgeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.curseforge.com";
    public int GameId { get; set; } = 432; // Minecraft
}
