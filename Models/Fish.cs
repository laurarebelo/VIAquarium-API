public class Fish
{
    public int Id { get; set; } // Primary key
    public string Name { get; set; } // Fish name

    public Fish(string name) {
        Id = 0;
        Name = name;
    }

    public Fish(int id, string name)
    {
        Id = id;
        Name = name;
    }
}