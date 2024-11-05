public interface IFishService
{
    Task<IEnumerable<Fish>> GetAllFish();
    Task<Fish> GetFishById(int fishId);
    Task<Fish> AddFish(string fishName);
    Task<bool> RemoveFish(int fishId);
    Task<Fish> DecayFishHunger(int fishId);
    Task<Fish> DecayFishSocial(int fishId);
    Task<Fish> FeedFish(int fishId, int howMuch);
    Task<Fish> PetFish(int fishId, int howMuch); 
    Task HandleFishDeaths(); // New method
}