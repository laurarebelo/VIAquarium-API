using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VIAquarium_API.Services
{
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
            var fish = await _context.Fish.FindAsync(fishId);
            return fish ?? throw new Exception($"Fish {fishId} not found in database");
        }

        public async Task<Fish> AddFish(string fishName)
        {
            var fish = new Fish(fishName);
            _context.Fish.Add(fish);
            await _context.SaveChangesAsync();
            return fish;
        }

        public async Task<bool> RemoveFish(int fishId)
        {
            var fish = await _context.Fish.FindAsync(fishId);
            if (fish == null) return false;

            _context.Fish.Remove(fish);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Fish> DecayFishHunger(int fishId)
        {
            return await DecayFishState(fishId, fish =>
            {
                int hungerPointsLost = CalculateLossSinceUpdate(fish.LastUpdatedHunger, 100);
                if (hungerPointsLost > 0)
                {
                    fish.GetHungry(hungerPointsLost);
                    if (fish.HungerLevel == 0 && fish.DeathStartTime == null)
                    {
                        fish.DeathStartTime = DateTime.UtcNow;
                    }
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
            var fish = await GetFishById(fishId);
            decayAction(fish);
            await UpdateFish(fish);
            return fish;
        }

        public async Task<Fish> FeedFish(int fishId, int howMuch)
        {
            await DecayFishHunger(fishId);
            var fish = await GetFishById(fishId);
            fish.Feed(howMuch);
            fish.DeathStartTime = null; // Reset DeathStartTime if fed
            await UpdateFish(fish);
            return fish;
        }

        public async Task<Fish> PetFish(int fishId, int howMuch)
        {
            await DecayFishSocial(fishId);
            var fish = await GetFishById(fishId);
            fish.Pet(howMuch);
            await UpdateFish(fish);
            return fish;
        }

        private async Task<List<Fish>> DecayAllFishStates()
        {
            var allFish = await _context.Fish.ToListAsync();
            var allFishUpdated = new List<Fish>();

            foreach (var fish in allFish)
            {
                var updatedFish = await DecayFishHunger(fish.Id);
                updatedFish = await DecayFishSocial(updatedFish.Id);
                allFishUpdated.Add(updatedFish);
            }

            return allFishUpdated;
        }

        private int CalculateLossSinceUpdate(DateTime lastUpdate, int lossInterval)
        {
            var howLongSinceLastUpdate = DateTime.UtcNow - lastUpdate;
            double minutesSinceLastUpdate = howLongSinceLastUpdate.TotalMinutes;
            int pointsLost = (int)(minutesSinceLastUpdate / lossInterval);
            return pointsLost;
        }

        private async Task UpdateFish(Fish fish)
        {
            _context.Fish.Update(fish);
            await _context.SaveChangesAsync();
        }

        public async Task HandleFishDeaths()
        {
            var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            // Find fish that meet the death conditions
            var fishToDie = await _context.Fish
                .Where(f => 
                    (f.HungerLevel == 0 && f.DeathStartTime.HasValue && f.DeathStartTime <= threeDaysAgo) ||
                    (f.SocialLevel == 0 && f.SocialDeathStartTime.HasValue && f.SocialDeathStartTime <= sevenDaysAgo)
                )
                .ToListAsync();

            // Log the count of fish to be moved to DeadFish table
            Console.WriteLine($"{fishToDie.Count} fish found for removal based on hunger or social level.");

            foreach (var fish in fishToDie)
            {
                var causeOfDeath = fish.HungerLevel == 0 && fish.DeathStartTime <= threeDaysAgo 
                    ? "Starvation"
                    : "Loneliness";

                var deadFish = new DeadFish
                {
                    Name = fish.Name,
                    DateOfDeath = DateTime.UtcNow,
                    DateOfBirth = fish.DateOfBirth,
                    DaysLived = (int)(DateTime.UtcNow - fish.DateOfBirth).TotalDays,
                    RespectCount = 0, // Placeholder or calculated value
                    CauseOfDeath = causeOfDeath
                };

                // Log fish details for verification
                Console.WriteLine($"Moving fish {fish.Name} to DeadFish table. Cause of death: {causeOfDeath}");

                _context.DeadFish.Add(deadFish);
                _context.Fish.Remove(fish); // Remove from AliveFish
            }

            await _context.SaveChangesAsync();
        }

    }
}
