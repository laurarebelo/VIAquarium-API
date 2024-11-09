using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VIAquarium_API.Models;

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
    public async Task<Fish> AddFish(FishCreation fishCreationObj)
    {
        Fish fish = new Fish(fishCreationObj);
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

            foreach (var fish in allFish)
            {
                var updatedFish = await DecayFishHunger(fish.Id);
                await DecayFishSocial(updatedFish.Id);
            } 
            await HandleFishDeaths();
            List<Fish> fishListNew = await _context.Fish.ToListAsync();
            
            return fishListNew;
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
    
            var fishToDie = await _context.Fish
                .Where(f => 
                    (f.HungerLevel == 0) ||
                    (f.SocialLevel == 0 )
                )
                .ToListAsync();
    
            foreach (var fish in fishToDie)
            {
                var causeOfDeath = (fish.HungerLevel == 0)
                    ? "Hunger"
                    : "Loneliness";

                var deadFish = new DeadFish
                {
                    Name = fish.Name,
                    DateOfDeath = DateTime.UtcNow,
                    DateOfBirth = fish.DateOfBirth,
                    DaysLived = (int)(DateTime.UtcNow - fish.DateOfBirth).TotalDays,
                    RespectCount = 0,
                    CauseOfDeath = causeOfDeath
                };

                await _context.DeadFish.AddAsync(deadFish);
                _context.Fish.Remove(fish);
                await _context.SaveChangesAsync();
                
                //if both hunger and loneliness are 0 it will die of hunger, do we care?
                
            }
        }

    }
}