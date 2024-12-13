namespace VIAquarium_API.Models;

[Serializable]
public class FishCreation(string name, string template, string sprite)
{
    public string name { get; set; } = name;
    public string template { get; set; } = template;
    public string sprite { get; set; } = sprite;
}