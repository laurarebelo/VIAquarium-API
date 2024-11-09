namespace VIAquarium_API.Models;

public static class FishTemplate
{
    public const string Angelfish = "Angelfish";
    public const string AnglerFish = "AnglerFish";
    public const string BlueTang = "BlueTang";
    public const string Clownfish = "Clownfish";
    public const string Default = "Default";
    public const string Jellyfish = "Jellyfish";
    public const string Sardine = "Sardine";
    public const string Starfish = "Starfish";
    public const string PufferFish = "PufferFish";
    
    // using lowercase so that the validation is type insensitive
    private static readonly HashSet<string> ValidTemplates = new()
    {
        Angelfish.ToLower(), AnglerFish.ToLower(), BlueTang.ToLower(), Clownfish.ToLower(),
        Default.ToLower(), Jellyfish.ToLower(), Sardine.ToLower(),
        Starfish.ToLower(), PufferFish.ToLower()
    };

    public static bool IsValid(string template)
    {
        return ValidTemplates.Contains(template.ToLower());
    }

}
