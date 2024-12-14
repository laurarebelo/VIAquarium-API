using VIAquarium_API.Models;

namespace VIAquarium_API.Services;

public interface IFishService
{
    Task<IEnumerable<Fish>> GetAllFish();
    Task<Fish> GetFishById(int fishId);
    Task<Fish> AddFish(FishCreation fishCreationObj);
    Task<bool> RemoveFish(int fishId);
    Task<Fish> DecayFishHunger(int fishId);
    Task<Fish> DecayFishSocial(int fishId);
    Task<Fish> FeedFish(int fishId, int howMuch);
    Task<Fish> PetFish(int fishId, int howMuch); 
    Task<DeadFish> KillFish(int fishId, DateTime dateOfDeath, string causeOfDeath);
    Task<IEnumerable<DeadFish>> GetAllDeadFish(string? sortBy, string? searchName, int? startIndex, int? endIndex);
    Task<DeadFish> GetDeadFishById(int fishId);
    Task<DeadFish> RespectDeadFish(int fishId, int howMuch);
    Task<Fish> ReviveFish(int deadFishId);
    Task<List<FishOnlyNeeds>> GetAliveFishNeeds();
}