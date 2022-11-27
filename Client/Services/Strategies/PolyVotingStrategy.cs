using System.Security.Cryptography;
using Server.Encoding;
using Server.Entity;
using Server.Models;
using Server.Services;
using Server.Services.Registration;

namespace Client.Services.Strategies;

public class PolyVotingStrategy
{
    private GammaEncoder _gammaEncoder = new(7003189);

    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenter;

    private Dictionary<int, int> _usersRandomId = new();

    private RegistrationCenterService _registrationCenter;

    private Random _random = new();

    public PolyVotingStrategy(
        DataProviderService dataProviderService, 
        VotingCenterService votingCenterService,
        RegistrationCenterService registrationCenter)
    {
        _dataProvider = dataProviderService;
        _votingCenter = votingCenterService;
        _registrationCenter = registrationCenter;
    }

    public void Vote(User user, int candidateId)
    {
        var regNumber = _registrationCenter.GenerateRegNumber(user.Id);

        if (!_usersRandomId.ContainsKey(user.Id))
        {
            var randomId = _random.Next(1, Int32.MaxValue);
            while (_usersRandomId.ContainsValue(randomId))
            {
                randomId = _random.Next(1, Int32.MaxValue);
            }
            
            _usersRandomId.Add(user.Id, randomId);
        }

        var id = _usersRandomId[user.Id];
        
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(_votingCenter.PublicKey);

        var bulletinId = _dataProvider.GenerateBulletinId(id, candidateId);
        var hashed = rsa.Encrypt(BitConverter.GetBytes(bulletinId), RSAEncryptionPadding.Pkcs1);

        var signed = _votingCenter.SignWithPrivateKey(hashed);
        
        _votingCenter.Vote(new PolyBulletinMessage
        {
            Id = id,
            RegistrationNumber = regNumber,
            data = hashed,
            signedData = signed
        });
    }
}