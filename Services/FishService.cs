using Microsoft.EntityFrameworkCore;

namespace VIAquarium_API.Services;

public class FishService : IFishService
{
    private readonly AquariumContext _context;

    public FishService(AquariumContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Fish>> GetAllFish()
    {
        return await UpdateAllFishHunger();
    }

    public async Task<Fish> GetFishById(int fishId)
    {
        Fish? fish = await _context.Fish.FindAsync(fishId);
        if (fish != null)
        {
            return fish;
        }

        throw new Exception($"Fish {fishId} not found in database");
    }

    public async Task<Fish> AddFish(string fishName)
    {
        Fish fish = new Fish(fishName);
        _context.Fish.Add(fish);
        await _context.SaveChangesAsync();
        return fish;
    }

    public async Task<bool> RemoveFish(int fishId)
    {
        var fish = await _context.Fish.FindAsync(fishId);
        if (fish == null)
        {
            return false;
        }

        _context.Fish.Remove(fish);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task UpdateFish(Fish fish)
    {
        _context.Fish.Update(fish);
        await _context.SaveChangesAsync();
    }

    public async Task<Fish> UpdateFishHunger(int fishId)
    {
        Fish fish = await GetFishById(fishId);
        int hungerPointsLost = CalculateHungerLossSinceUpdate(fish.LastUpdatedHunger);
        fish.GetHungry(hungerPointsLost);
        await UpdateFish(fish);
        return fish;
    }
    
    private async Task<List<Fish>> UpdateAllFishHunger()
    {
        List<Fish> allFish = await _context.Fish.ToListAsync();
        List<Fish> allFishUpdated = new List<Fish>();
        foreach (var fish in allFish)
        {
            Fish newFish = await UpdateFishHunger(fish.Id);
            allFishUpdated.Add(newFish);
        }
        return allFishUpdated;
    }

    public async Task<Fish> FeedFish(int fishId, int howMuch)
    {
        await UpdateFishHunger(fishId);
        Fish fish = await GetFishById(fishId);
        fish.Feed(howMuch);
        await UpdateFish(fish);
        return fish;
    }

    private int CalculateHungerLossSinceUpdate(DateTime lastUpdate)
    {
        TimeSpan howLongSinceLastUpdate = DateTime.Today - lastUpdate;
        double minutesSinceLastUpdate = howLongSinceLastUpdate.Minutes;
        // For a fish to die in 7 days, it takes 100 minutes to lose 1 hunger point...
        int hungerPointsLost = (int)(minutesSinceLastUpdate / 100);
        return hungerPointsLost;
    }
}