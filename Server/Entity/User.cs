using System.Security.Cryptography;

namespace Server.Entity;

public class User
{
    public int Id { get; set; }
    
    public bool CanVote { get; set; }
    
    private RSA rsa { get; set; }
    
    private RSA secondRsa { get; set; }
    
    public RSAParameters PublicKey => rsa.ExportParameters(false);

    public User(int keysize = 1024, int secondKeySize = 1024)
    {
        rsa = RSA.Create(keysize);
        secondRsa = RSA.Create(secondKeySize);
    }

    public byte[] ApplyPrivateKey(byte[] msg)
    {
        return rsa.Decrypt(msg, RSAEncryptionPadding.Pkcs1);
    }
    
    public byte[] ApplyPublicKey(byte[] msg)
    {
        return rsa.Encrypt(msg, RSAEncryptionPadding.Pkcs1);
    }
    
    public byte[] ApplySecondPrivateKey(byte[] msg)
    {
        return secondRsa.Decrypt(msg, RSAEncryptionPadding.Pkcs1);
    }
    
    public byte[] ApplySecondPublicKey(byte[] msg)
    {
        return secondRsa.Encrypt(msg, RSAEncryptionPadding.Pkcs1);
    }
    
    public byte[] SignWithPrivateKey(byte[] msg)
    {
        return rsa.SignData(msg, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
    }
    
    public bool CheckIfSigned(byte[] signature, byte[] data)
    {
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1);
    }
}