using System.Security.Cryptography;
using Server.Services.Strategies;

namespace Server.Services;

public class VotingCenterService
{
    private RSA rsa;

    private DataProviderService _dataProvider;

    public RSAParameters PublicKey => rsa.ExportParameters(false);

    public VotingCenterService(DataProviderService data)
    {
        _dataProvider = data;
        rsa = RSA.Create();
    }

    public void Vote(byte[] msg, byte[] sign, byte[] signedData)
    {
        if (msg.Length == 0)
        {
            throw new Exception("data is empty");
        }
        
        // lab 1
        // var strategy = new SimpleVotingStrategy(_dataProvider, this);
        // strategy.Vote(new Bulletin { Message = msg, Sign = sign});
        
        // lab 2
        var strategy = new BlindSignVotingStrategy(_dataProvider, this);
        strategy.Vote(msg, sign, signedData);
    }

    public object Sign(object msgs)
    {
        // lab 2
        var strategy = new BlindSignVotingStrategy(_dataProvider, this);
        return strategy.Sign(msgs);
    }
    
    internal byte[] SignWithPrivateKey(byte[] msg)
    {
        return rsa.SignData(msg, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
    }
    
    public bool CheckIfSigned(byte[] msg, byte[] data)
    {
        return rsa.VerifyData(data, msg, HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1);
    }

    public byte[] ApplyPrivateKey(byte[] msg)
    {
        return rsa.Decrypt(msg, RSAEncryptionPadding.Pkcs1);
    }
    
    public bool CheckIfSigned(byte[] msg)
    {
        return true;
    }
}