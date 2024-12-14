using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    if (hungerPointsLost >= fish.HungerLevel)
                    {
                        hungerPointsLost = fish.HungerLevel;
                        var timeOfDeath = fish.LastUpdatedHunger + TimeSpan.FromMinutes(hungerPointsLost * 100);
                        return (timeOfDeath, "Hunger");
                    }
                    else
                    {
                        fish.GetHungry(hungerPointsLost);
                    }
                }
                return (null, null);
            });
        }

        public async Task<Fish> DecayFishSocial(int fishId)
        {
            return await DecayFishState(fishId, fish =>
            {
                int socialPointsLost = CalculateLossSinceUpdate(fish.LastUpdatedSocial, 144);
                if (socialPointsLost > 0)
                {
                    if (socialPointsLost >= fish.SocialLevel)
                    {
                        socialPointsLost = fish.SocialLevel;
                        var timeOfDeath = fish.LastUpdatedSocial + TimeSpan.FromMinutes(socialPointsLost * 144);
                        return (timeOfDeath, "Loneliness");
                    }
                    else
                    {
                        fish.GetLonely(socialPointsLost);
                    }
                }

                return (null, null);
            });
        }

        private async Task<Fish> DecayFishState(int fishId,
            Func<Fish, (DateTime? timeOfDeath, string? cause)> decayAction)
        {
            var fish = await GetFishById(fishId);
            var (timeOfDeath, cause) = decayAction(fish);

            if (timeOfDeath.HasValue && cause != null)
            {
                await KillFish(fish.Id, timeOfDeath.Value, cause);
            }
            else
            {
                await UpdateFish(fish);
            }

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
                if (_context.Entry(updatedFish).State == EntityState.Detached)
                {
                    // If the fish died from hunger, no need to check social because
                    // it would cause an error since it's in the DeadFish table already
                    continue;
                }

                await DecayFishSocial(updatedFish.Id);
            }

            return await _context.Fish.ToListAsync();
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

        public async Task<DeadFish> KillFish(int fishId, DateTime dateOfDeath, string causeOfDeath = "Hunger")
        {
            var fish = await GetFishById(fishId);
            var deadFish = new DeadFish
            {
                Name = fish.Name,
                DateOfDeath = dateOfDeath,
                DateOfBirth = fish.DateOfBirth,
                DaysLived = (int)(dateOfDeath - fish.DateOfBirth).TotalDays,
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
            FishCreation revivedFishCreation =
                new FishCreation(deadFish.Name, deadFish.Template, Convert.ToBase64String(deadFish.Sprite));
            var revivedFish = new Fish(revivedFishCreation);
            await _context.Fish.AddAsync(revivedFish);
            _context.DeadFish.Remove(deadFish);
            await _context.SaveChangesAsync();
            return revivedFish;
        }

        public async Task<List<FishOnlyNeeds>> GetAliveFishNeeds()
        {
            await DecayAllFishStates();
            var allFish = await _context.Fish.ToListAsync();
            var fishNeedsList = allFish.Select(fish => new FishOnlyNeeds
            {
                Id = fish.Id,
                HungerLevel = fish.HungerLevel,
                SocialLevel = fish.SocialLevel
            }).ToList();

            return fishNeedsList;
        }
    }
}