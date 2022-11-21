using System.Security.Cryptography;
using Server.Encoding;
using Server.Entity;
using Server.Services;

namespace Client.Services.Strategies;

public class SimpleVotingStrategy
{
    private GammaEncoder _gammaEncoder = new();

    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenter;

    public SimpleVotingStrategy(DataProviderService dataProviderService, VotingCenterService votingCenterService)
    {
        _dataProvider = dataProviderService;
        _votingCenter = votingCenterService;
    }
    
    public void Vote(User user, int candidateId)
    {
        var bulletinId = _dataProvider.GenerateBulletinId(user.Id, candidateId);

        var hashed = _gammaEncoder.EncodeBinary(BitConverter.GetBytes(bulletinId));
        var signedMsg = user.SignWithPrivateKey(hashed);
        
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(_votingCenter.PublicKey);

        var encrypted = rsa.Encrypt(BitConverter.GetBytes(bulletinId), RSAEncryptionPadding.Pkcs1);
        
        _votingCenter.Vote(encrypted, signedMsg);
    }
}