public class Fish
{
    public int Id { get; set; } // Primary key
    public string Name { get; set; } // Fish name
    public int HungerLevel { get; set; }
    public DateTime LastUpdatedHunger { get; set; }
    public int SocialLevel { get; set; }
    public DateTime LastUpdatedSocial { get; set; }
    public DateTime DateOfBirth { get; set; }

    public Fish(string name)
    {
        Name = name;
        DateOfBirth = DateTime.UtcNow;
        ResetNeeds();
    }

    public Fish(int id, string name)
    {
        Id = id;
        Name = name;
        DateOfBirth = DateTime.UtcNow;
        ResetNeeds();
    }

    private void ResetNeeds()
    {
        HungerLevel = 100;
        LastUpdatedHunger = DateTime.UtcNow;
        SocialLevel = 100;
        LastUpdatedSocial = DateTime.UtcNow;
    }

    public void Feed(int amount)
    {
        HungerLevel += amount;
        if (HungerLevel > 100) HungerLevel = 100;
        LastUpdatedHunger = DateTime.UtcNow;
    }

    public void Pet(int amount)
    {
        SocialLevel += amount;
        if (SocialLevel > 100) SocialLevel = 100;
        LastUpdatedSocial = DateTime.UtcNow;
    }

    public void GetLonely(int amount)
    {
        SocialLevel -= amount;
        if (SocialLevel < 0) SocialLevel = 0;
        LastUpdatedSocial = DateTime.UtcNow;
        
    }

    public void GetHungry(int amount)
    {
        HungerLevel -= amount;
        if (HungerLevel < 0) HungerLevel = 0;
        LastUpdatedHunger = DateTime.UtcNow;

    }
}
