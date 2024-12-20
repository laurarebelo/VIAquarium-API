﻿public class DeadFish
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
    public int DaysLived { get; set; }
    public int RespectCount { get; set; } = 0;
    public string CauseOfDeath { get; set; }
    public string Template { get; set; }
    public byte[] Sprite { get; set; }
    
    public DeadFish(){}
    
    public void Respect(int amount)
    {
        RespectCount += Math.Abs(amount);
    }
}