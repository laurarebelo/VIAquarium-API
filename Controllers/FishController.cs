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

    // GET: api/fish
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fish>>> GetFish()
    {
        var fishList = await fishService.GetAllFish();
        return Ok(fishList);
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
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message }); }
    }

    // DELETE: api/fish/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFish(int id)
    {
        bool fishRemoved = await fishService.RemoveFish(id);
        return fishRemoved ? Ok(fishRemoved) : NotFound();
    }


    // =========== FISH NEEDS ============ //

    // POST: api/fish/{fishId}/hunger
    [HttpPost("{fishId}/hunger")]
    public async Task<IActionResult> AddHungerPoints(int fishId, [FromBody] FeedRequest feedRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Fish fish = await fishService.FeedFish(fishId, feedRequest.hungerPoints);
        return Ok(new
        {
            fish.Id,
            fish.HungerLevel
        });
    }
}