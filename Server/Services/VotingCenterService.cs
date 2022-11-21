using System.Security.Cryptography;
using Server.DataSource;
using Server.Entity;
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

    public void Vote(byte[] msg, byte[] sign)
    {
        if (msg.Length == 0 || sign.Length == 0)
        {
            throw new Exception("data is empty");
        }
        
        // lab 1
        var strategy = new SimpleVotingStrategy(_dataProvider, this);
        strategy.Vote(new Bulletin { Message = msg, Sign = sign});
    }

    public byte[] ApplyPrivateKey(byte[] msg)
    {
        return rsa.Decrypt(msg, RSAEncryptionPadding.Pkcs1);
    }
}