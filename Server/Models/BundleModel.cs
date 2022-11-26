namespace Server.Models;

public class BundleModel
{
    public List<BlindPackageModel> Packages { get; set; }
    
    public long Id { get; set; }
}