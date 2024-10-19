namespace VIAquarium_API.Services;

public interface IFishService
{
    Task<IEnumerable<Fish>> GetAllFish();
    Task<Fish> GetFishById(int fishId);
    Task<Fish> AddFish(string fishName);
    Task<bool> RemoveFish(int fishId);
    Task<Fish> UpdateFishHunger(int fishId);
    Task<Fish> FeedFish(int fishId, int howMuch);

}