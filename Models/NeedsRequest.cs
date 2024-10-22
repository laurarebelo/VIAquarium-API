using System.ComponentModel.DataAnnotations;

namespace VIAquarium_API.Models;

[Serializable]
public class NeedsRequest
{
    [Required]
    [Range(1,100)]
    public int hungerPoints { get; set; }

    [Required]
    [Range(1,100)]
    public int pettingPoints { get; set; }
}