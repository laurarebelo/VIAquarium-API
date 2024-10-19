using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class FishController : ControllerBase
{
    private readonly AquariumContext _context;

    public FishController(AquariumContext context)
    {
        _context = context;
    }

    // GET: api/fish
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fish>>> GetFish()
    {
        return await _context.Fish.ToListAsync();
    }

    // POST: api/fish
    [HttpPost]
    public async Task<ActionResult<Fish>> PostFish(string fishName)
    {
        Fish fish = new Fish(fishName);
        _context.Fish.Add(fish);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFish), new { id = fish.Id }, fish);
    }

    // DELETE: api/fish/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFish(int id)
    {
        var fish = await _context.Fish.FindAsync(id);
        if (fish == null)
        {
            return NotFound();
        }

        _context.Fish.Remove(fish);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
