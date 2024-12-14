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

        private async Task UpdateDeadFish(DeadFish deadFish)
        {
            _context.DeadFish.Update(deadFish);
            await _context.SaveChangesAsync();
        }

        public async Task<DeadFish> KillFish(int fishId, string causeOfDeath = "Hunger")
        {
            var fish = await GetFishById(fishId);
            var deadFish = new DeadFish
            {
                Name = fish.Name,
                DateOfDeath = DateTime.UtcNow,
                DateOfBirth = fish.DateOfBirth,
                DaysLived = (int)(DateTime.UtcNow - fish.DateOfBirth).TotalDays,
                RespectCount = 0,
                CauseOfDeath = causeOfDeath,
                Template = fish.Template,
                Sprite = fish.Sprite,
            };
            await _context.DeadFish.AddAsync(deadFish);
            _context.Fish.Remove(fish);
            await _context.SaveChangesAsync();
            return deadFish;
        }

        public async Task HandleFishDeaths()
        {
            var fishToDie = await _context.Fish
                .Where(f =>
                    (f.HungerLevel == 0) ||
                    (f.SocialLevel == 0)
                )
                .ToListAsync();

            foreach (var fish in fishToDie)
            {
                var causeOfDeath = (fish.HungerLevel == 0)
                    ? "Hunger"
                    : "Loneliness";

                await KillFish(fish.Id, causeOfDeath);
            }
        }


        public async Task<IEnumerable<DeadFish>> GetAllDeadFish(
            string? sortBy = null,
            string? searchName = null,
            int? startIndex = null,
            int? endIndex = null)
        {
            var query = _context.DeadFish.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(fish => EF.Functions.Like(fish.Name, $"%{searchName}%"));
            }

            if (sortBy != null)
            {
                switch (sortBy.ToLower())
                {
                    case "lastdied":
                        query = query.OrderByDescending(fish => fish.DateOfDeath);
                        break;
                    case "mostrespect":
                        query = query.OrderByDescending(fish => fish.RespectCount);
                        break;
                    case "mostdayslived":
                        query = query.OrderByDescending(fish => fish.DaysLived);
                        break;
                    default:
                        break;
                }
            }

            if (startIndex.HasValue && endIndex.HasValue)
            {
                query = query.Skip(startIndex.Value).Take(endIndex.Value - startIndex.Value);
            }
            else if (startIndex.HasValue)
            {
                query = query.Skip(startIndex.Value);
            }
            else if (endIndex.HasValue)
            {
                query = query.Take(endIndex.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<DeadFish> GetDeadFishById(int fishId)
        {
            var deadFish = await _context.DeadFish.FindAsync(fishId);
            return deadFish ?? throw new Exception($"Fish {fishId} not found in database");
        }

        public async Task<DeadFish> RespectDeadFish(int fishId, int howMuch)
        {
            var fish = await GetDeadFishById(fishId);
            fish.Respect(howMuch);
            await UpdateDeadFish(fish);
            return fish;
        }

        public async Task<Fish> ReviveFish(int deadFishId)
        {
            var deadFish = await GetDeadFishById(deadFishId);
            FishCreation revivedFishCreation = new FishCreation(deadFish.Name, deadFish.Template, Convert.ToBase64String(deadFish.Sprite));
            var revivedFish = new Fish(revivedFishCreation);
            await _context.Fish.AddAsync(revivedFish);
            _context.DeadFish.Remove(deadFish);
            await _context.SaveChangesAsync();
            return revivedFish;
        }
    }
}