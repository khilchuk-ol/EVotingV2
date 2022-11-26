namespace Server.Models;

public class BlindPackageModel
{
    public long R { get; set; }
    
    public List<byte[]>? Messages { get; set; }
    
    public List<byte[]>? SignedBulletins { get; set; }
    
    public List<byte[]>? Bulletins { get; set; }
}