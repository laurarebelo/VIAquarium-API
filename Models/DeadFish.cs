public class DeadFish
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
    public int DaysLived { get; set; }
    public int RespectCount { get; set; } = 0;
    public string CauseOfDeath { get; set; }
}