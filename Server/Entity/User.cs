using System.Security.Cryptography;

namespace Server.Entity;

public class User
{
    public int Id { get; set; }
    
    public bool CanVote { get; set; }
    
    private RSA rsa { get; set; }
    
    public RSAParameters PublicKey => rsa.ExportParameters(false);

    public User()
    {
        rsa = RSA.Create();
    }

    public byte[] ApplyPrivateKey(byte[] msg)
    {
        var rsaService = new RSACryptoServiceProvider();
        rsaService.ImportParameters(rsa.ExportParameters(true));

        return rsaService.Encrypt(msg, false);
    }
    
    public byte[] SignWithPrivateKey(byte[] msg)
    {
        return rsa.SignData(msg, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
    }
    
    public bool CheckIfSigned(byte[] msg, byte[] data)
    {
        return rsa.VerifyData(data, msg, HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1);
    }
}