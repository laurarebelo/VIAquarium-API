using System.ComponentModel.DataAnnotations;

namespace VIAquarium_API.Models;

[Serializable]
public class FeedRequest
{
    [Required]
    [Range(1,100)]
    public int hungerPoints { get; set; }
}