namespace Server.Models;

public class PolyBulletinMessage
{
    public int Id { get; set; }
    
    public long RegistrationNumber { get; set; }
    
    public byte[] data { get; set; }
    
    public byte[] signedData { get; set; }
}