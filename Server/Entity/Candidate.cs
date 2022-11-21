namespace Server.Entity;

public class Candidate
{
    public int Id { get; set; }

    public string Name { get; set; } = "anonymous";
    
    public override string ToString()
    {
        return $"{Id}: {Name}";
    }
}