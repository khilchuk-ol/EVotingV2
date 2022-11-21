namespace Server.Entity;

public class Bulletin
{
    public int Id { get; set; }
    
    public byte[] Sign { get; set; }
    
    public byte[] Message { get; set; }
}