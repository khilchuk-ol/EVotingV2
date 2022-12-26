using System.Security.Cryptography;

namespace Server.Services.Strategies;

public class SeparatedVotingStrategy
{
    
    private RSA rsa;

    public Dictionary<int, byte[]> Bulletins { get; } = new();

    public SeparatedVotingStrategy()
    {
        rsa = RSA.Create();
    }
    
    public byte[] Sign(byte[] msg)
    {
        return rsa.SignData(msg, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
    }
    
    private bool VerifySign(byte[] msg, byte[] signature)
    {
        return rsa.VerifyData(msg, signature, HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1);
    }

    public void Accept(int id, byte[] msg, byte[] signature)
    {
        if (!VerifySign(msg, signature))
        {
            throw new Exception("sign is incorrect");
        }

        if (Bulletins.ContainsKey(id))
        {
            throw new Exception("user already sent bulletin");
        }
        
        Bulletins.Add(id, msg);
    }
}