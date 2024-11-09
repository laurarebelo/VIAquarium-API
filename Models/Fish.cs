using VIAquarium_API.Models;

public class Fish
{
    public int Id { get; set; } // Primary key
    public string Name { get; set; } // Fish name
    public int HungerLevel { get; set; }
    public DateTime LastUpdatedHunger { get; set; }
    public int SocialLevel { get; set; }
    public DateTime LastUpdatedSocial { get; set; }
    public string Template { get; set; }
    public byte[] Sprite { get; set; }

    public Fish(FishCreation fishCreationObj)
    {
        Id = 0;
        Name = fishCreationObj.name;
        // validate template type
        if (!FishTemplate.IsValid(fishCreationObj.template))
        {
            throw new ArgumentException($"Invalid template: {fishCreationObj.template}");
        }
        Template = fishCreationObj.template;
        Sprite = Convert.FromBase64String(fishCreationObj.sprite);
        ResetNeeds();
    }

    public Fish() {}
    
    public Fish(string name)
    {
        Id = 0;
        Name = name;
        Template = FishTemplate.Default;
        ResetNeeds();
    }

    public Fish(int id, string name)
    {
        Id = id;
        Name = name;
        Template = FishTemplate.Default;
        ResetNeeds();
    }

    private void ResetNeeds()
    {
        HungerLevel = 100;
        LastUpdatedHunger = DateTime.Now;
        SocialLevel = 100;
        LastUpdatedSocial = DateTime.Now;
    }

    public void Feed(int amount)
    {
        HungerLevel += amount;
        if (HungerLevel > 100) HungerLevel = 100;
        LastUpdatedHunger = DateTime.Now;
    }

    public void GetHungry(int amount)
    {
        HungerLevel -= amount;
        if (HungerLevel < 0) HungerLevel = 0;
        LastUpdatedHunger = DateTime.Now;
    }
}