using System.ComponentModel.DataAnnotations;

namespace VIAquarium_API.Models;

[Serializable]
public class NeedsRequest
{
    [Required]
    [Range(1,100)]
    public int points { get; set; }
}