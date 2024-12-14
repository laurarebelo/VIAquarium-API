using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VIAquarium_API.Models;
using VIAquarium_API.Services;

[Route("api/[controller]")]
[ApiController]
public class FishController : ControllerBase
{
    private readonly IFishService fishService;

    public FishController(IFishService service)
    {
        fishService = service;
    }

    // ======= BASIC ACTIONS ========= //

    // GET: api/fish/alive

    [HttpGet("alive")]
    public async Task<ActionResult<IEnumerable<Fish>>> GetFish()
    {
        var fishList = await fishService.GetAllFish();
        return Ok(fishList);
    }

    // GET: api/fish/dead
    [HttpGet("dead")]
    public async Task<ActionResult<IEnumerable<DeadFish>>> GetAllDeadFish(
        string? sortBy = null, 
        string searchName = null, 
        int? startIndex = null, 
        int? endIndex = null)
    {
        var deadFishList = await fishService.GetAllDeadFish(sortBy, searchName, startIndex, endIndex);
        return Ok(deadFishList);
    }


    // POST: api/fish
    [HttpPost]
    public async Task<ActionResult<Fish>> PostFish(FishCreation fishCreation)
    {
        try
        {
            Fish fish = await fishService.AddFish(fishCreation);
            return CreatedAtAction(nameof(GetFish), new { id = fish.Id }, fish);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
        }
    }
    
    // POST: api/fish/{id}/kill
    [HttpPost("{id}/kill")]
    public async Task<DeadFish> KillFish(int id, [FromQuery] string causeOfDeath = "Hunger")
    {
        return await fishService.KillFish(id, causeOfDeath);
    }

    // DELETE: api/fish/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFish(int id)
    {
        bool fishRemoved = await fishService.RemoveFish(id);
        return fishRemoved ? Ok(fishRemoved) : NotFound();
    }


    // =========== FISH NEEDS ============ //

    // POST: api/fish/{fishId}/{needType}}
    [HttpPatch("{fishId}/{needType}")]
    public async Task<IActionResult> UpdateFishNeeds(int fishId, string needType, [FromBody] NeedsRequest needsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Fish fish;
        switch (needType.ToLower())
        {
            case "hunger":
                fish = await fishService.FeedFish(fishId, needsRequest.points);
                return Ok(new { fish.Id, fish.HungerLevel });

            case "social":
                fish = await fishService.PetFish(fishId, needsRequest.points);
                return Ok(new { fish.Id, fish.SocialLevel });

            default:
                return BadRequest("Invalid need type. Use 'hunger' or 'social'.");
        }
    }
    
    // POST: api/fish/{id}/respect
    [HttpPost("dead/{id}/respect")]
    public async Task<IActionResult> RespectDeadFish(int id, int howMuch)
    {
        try
        {
            var respectedFish = await fishService.RespectDeadFish(id, howMuch);
            return Ok(new 
            { 
                message = "The dead fish has been respected.", 
                fishId = respectedFish.Id, 
                name = respectedFish.Name, 
                respectedCount = respectedFish.RespectCount 
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Dead fish not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
        }
    }
    
    // POST: api/fish/dead/{id}/revive
    [HttpPost("dead/{id}/revive")]
    public async Task<Fish> ReviveFish(int id)
    {
        return await fishService.ReviveFish(id);
    }

}