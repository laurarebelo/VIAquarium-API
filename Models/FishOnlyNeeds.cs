namespace VIAquarium_API.Models;

public class FishOnlyNeeds
{
    public int Id { get; set; }
    public int HungerLevel { get; set; }
    public int SocialLevel { get; set; }
    public FishOnlyNeeds(){}
    public FishOnlyNeeds(int id, int hungerLevel, int socialLevel)
    {
        Id = id;
        HungerLevel = hungerLevel;
        SocialLevel = socialLevel;
    }

}