using System.Numerics;
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

    public SeparatedVotingStrategy[] SeparatedVotingStrategies { get; }
    
    public VotingCenterService(DataProviderService data, RegistrationCenterService registrationCenterService)
    {
        _registrationCenterService = registrationCenterService;
        _dataProvider = data;
        
        rsa = RSA.Create();
        
        dsa = DSA.Create();

        SeparatedVotingStrategies = new[]
        {
            new SeparatedVotingStrategy(),
            new SeparatedVotingStrategy()
        };
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
        // var strategy = new BlindSignVotingStrategy(_dataProvider, this);
        // strategy.Vote(msg, sign, signedData);
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
    
    public bool CheckIfSigned(byte[] msg, byte[] signature)
    {
        return rsa.VerifyData(msg, signature, HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1);
    }
    
    public byte[] ApplyPublicKey(byte[] msg)
    {
        return rsa.Encrypt(msg, RSAEncryptionPadding.Pkcs1);
    }

    public byte[] ApplyPrivateKey(byte[] msg)
    {
        return rsa.Decrypt(msg, RSAEncryptionPadding.Pkcs1);
    }

    public BigInteger Encrypt(BigInteger x)
    {
        return BigInteger.Pow(x, 7) % 33;
    }
    
    private BigInteger Decrypt(BigInteger x)
    {
        return BigInteger.Pow(x, 3) % 33;
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

    public Dictionary<int, int> GetSeparatedResults()
    {
        var a = SeparatedVotingStrategies[0];
        var b = SeparatedVotingStrategies[1];
        
        var res = new Dictionary<int, int>();

        foreach (var key in a.Bulletins.Keys)
        {
            if (!b.Bulletins.ContainsKey(key))
            {
                throw new Exception("missing bulletin part");
            }
            
            var x = new BigInteger(a.Bulletins[key]);
            var y = new BigInteger(b.Bulletins[key]);

            var multiplied = x * y;
            
            var decrypted = Decrypt(multiplied);
            var candidateId = (int)decrypted;
            
            if (res.ContainsKey(candidateId))
            {
                res[candidateId] += 1;
            }
            else
            {
                res.Add(candidateId, 1);
            }
        }

        return res;
    }
}