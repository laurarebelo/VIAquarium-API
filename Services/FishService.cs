using System.Diagnostics;
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
        return await DecayAllFishStates();
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

    public async Task<Fish> DecayFishHunger(int fishId)
    {
        return await DecayFishState(fishId, fish => 
        {
            int hungerPointsLost = CalculateLossSinceUpdate(fish.LastUpdatedHunger, 100);
            if (hungerPointsLost > 0)
            {
                fish.GetHungry(hungerPointsLost);
            }
        });
    }

    public async Task<Fish> DecayFishSocial(int fishId)
    {
        return await DecayFishState(fishId, fish => 
        {
            int socialPointsLost = CalculateLossSinceUpdate(fish.LastUpdatedSocial, 144);
            if (socialPointsLost > 0)
            {
                fish.GetLonely(socialPointsLost);
            }
        });
    }

    private async Task<Fish> DecayFishState(int fishId, Action<Fish> decayAction)
    {
        Fish fish = await GetFishById(fishId);
        decayAction(fish);
        await UpdateFish(fish);
        return fish;
    }

    public async Task<Fish> FeedFish(int fishId, int howMuch)
    {
        await DecayFishHunger(fishId);
        Fish fish = await GetFishById(fishId);
        fish.Feed(howMuch);
        await UpdateFish(fish);
        return fish;
    }

    public async Task<Fish> PetFish(int fishId, int howMuch)
    {
        await DecayFishSocial(fishId);
        Fish fish = await GetFishById(fishId);
        fish.Pet(howMuch);
        await UpdateFish(fish);
        return fish;
    }

    private async Task<List<Fish>> DecayAllFishStates()
    {
        List<Fish> allFish = await _context.Fish.ToListAsync();
        List<Fish> allFishUpdated = new List<Fish>();
        foreach (var fish in allFish)
        {
            Fish updatedFish = await DecayFishHunger(fish.Id); 
            updatedFish = await DecayFishSocial(updatedFish.Id);
            allFishUpdated.Add(updatedFish);
        }
        return allFishUpdated;
    }

    private int CalculateLossSinceUpdate(DateTime lastUpdate, int lossInterval)
    {
        TimeSpan howLongSinceLastUpdate = DateTime.Now - lastUpdate;
        double minutesSinceLastUpdate = howLongSinceLastUpdate.TotalMinutes;
        int pointsLost = (int)(minutesSinceLastUpdate / lossInterval);
        return pointsLost;
    }
}
