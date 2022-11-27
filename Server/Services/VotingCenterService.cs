using System.Security.Cryptography;
using Server.Entity;
using Server.Models;
using Server.Services.Registration;
using Server.Services.Strategies;

namespace Server.Services;

public class VotingCenterService
{
    private RSA rsa;

    private DataProviderService _dataProvider;

    public RSAParameters PublicKey => rsa.ExportParameters(false);
    
    private DSA dsa;

    private RegistrationCenterService _registrationCenterService;

    public VotingCenterService(DataProviderService data, RegistrationCenterService registrationCenterService)
    {
        _registrationCenterService = registrationCenterService;
        _dataProvider = data;
        
        rsa = RSA.Create();
        
        dsa = DSA.Create();
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

    public void Vote(PolyBulletinMessage msg)
    {
        // lab 3
        
        var strategy = new PolyVotingStrategy(_dataProvider, this, _registrationCenterService);
        strategy.Vote(new Bulletin()
        {
            Id = msg.Id,
            Message = msg.data,
            Sign = msg.signedData
        }, msg.RegistrationNumber);
    }

    public object Sign(object msgs)
    {
        // lab 2
        var strategy = new BlindSignVotingStrategy(_dataProvider, this);
        return strategy.Sign(msgs);
    }
    
    public byte[] SignWithPrivateKey(byte[] msg)
    {
        return rsa.SignData(msg, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
    }
    
    public bool CheckIfSigned(byte[] msg, byte[] data)
    {
        return rsa.VerifyData(msg, data, HashAlgorithmName.SHA512,
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

    public byte[] DSASignHash(byte[] msg)
    {
        var HashAlg = "SHA1";
        
        byte[] sig = null;

        try
        {
            var svc = new DSACryptoServiceProvider();
            svc.ImportParameters(dsa.ExportParameters(true));
            
            DSASignatureFormatter DSAFormatter = new DSASignatureFormatter(svc);

            DSAFormatter.SetHashAlgorithm(HashAlg);
            sig = DSAFormatter.CreateSignature(msg);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e.Message);
        }

        return sig;
    }

    internal bool DSAVerifyHash(byte[] msg, byte[] signedMsg)
    {
        var HashAlg = "SHA1";
        
        bool verified = false;

        try
        {
            var svc = new DSACryptoServiceProvider();
            svc.ImportParameters(dsa.ExportParameters(false));
            
            DSASignatureDeformatter DSADeformatter = new DSASignatureDeformatter(svc);

            DSADeformatter.SetHashAlgorithm(HashAlg);
            verified = DSADeformatter.VerifySignature(msg, signedMsg);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine(e.Message);
        }

        return verified;
    }
}