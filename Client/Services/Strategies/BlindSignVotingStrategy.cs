using System.Security.Cryptography;
using Server.Encoding;
using Server.Entity;
using Server.Models;
using Server.Services;

namespace Client.Services.Strategies;

public class BlindSignVotingStrategy
{
    private readonly int[] primesOrdered = 
    {
        5,
        7,
        11,
        13,
        17,
        19,
        23,
        31,
        37,
        41,
        43,
        47,
        53,
        57,
        59,
        61,
        67,
        71
    };

    private const int GAMMA = 73;

    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenter;

    private GammaEncoder _gamma = new(GAMMA);

    public BlindSignVotingStrategy(DataProviderService dataProviderService, VotingCenterService votingCenterService)
    {
        _dataProvider = dataProviderService;
        _votingCenter = votingCenterService;
    }
    
    public void Vote(User user, int candidateId)
    {
        var packages = CreatePackages(user);
        
        var id = BitConverter.ToInt32(_gamma.EncodeBinary(BitConverter.GetBytes(user.Id)));

        var bundle = new BundleModel
        {
            Packages = packages,
            Id = id
        };

        var signed = (BlindPackageModel)_votingCenter.Sign(bundle);

        if (signed.Messages!.Count != signed.SignedBulletins!.Count)
        {
            throw new Exception("center voting system hasn't signed messages");
        }
        
        for (int i = 0; i < signed.Messages.Count; i++)
        {
            var msg = signed.Messages[i];
            var signedBulletin = signed.SignedBulletins[i];

            var gamma = new GammaEncoder((int)signed.R);
            var bulletinId = user.ApplyPrivateKey(gamma.DecodeBinary(msg));

            var bulletin = _dataProvider.UnwrapBulletinId(BitConverter.ToInt32(bulletinId));

            if (bulletin.CandidateId != candidateId) continue;

            if (bulletin.UserId != id) continue;

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(_votingCenter.PublicKey);

            var preparedBulletin = rsa.Encrypt(bulletinId, RSAEncryptionPadding.Pkcs1);
            
            _votingCenter.Vote(preparedBulletin, signedBulletin, signed.Bulletins![i]);
        }
    }

    private List<BlindPackageModel> CreatePackages(User u)
    {
        var candidates = _dataProvider.GetAllCandidates();
        var id = BitConverter.ToInt32(_gamma.EncodeBinary(BitConverter.GetBytes(u.Id)));

        var msgs = new List<BlindPackageModel>();

        for (int i = 0; i < 10; i++)
        {
            var random = new Random();
            var idx = random.Next(0, primesOrdered.Length - 1);

            var generatedPrimes = GeneratePrimesNaive(random.Next(50, 150), primesOrdered[idx], primesOrdered[idx + 1]);
            var indxR = random.Next(0, generatedPrimes.Count);
            var r = generatedPrimes[indxR];

            while (BitConverter.ToInt64(u.PublicKey.Modulus) % r == 0)
            {
                r = generatedPrimes[++indxR % generatedPrimes.Count];
            }
            
            var maskedBulletins = new List<byte[]>();
            var bulletins = new List<byte[]>();
            
            foreach (var c in candidates)
            {
                var bulletinId = u.ApplyPublicKey(BitConverter.GetBytes(_dataProvider.GenerateBulletinId(id, c.Id)));

                var gamma = new GammaEncoder(r);
                var maskedBulletin = gamma.EncodeBinary(bulletinId);

                maskedBulletins.Add(maskedBulletin);
                bulletins.Add(bulletinId);
            }
            
            msgs.Add(new BlindPackageModel
            {
                R = r,
                Messages = maskedBulletins,
                Bulletins = bulletins
            });
        }

        return msgs;
    }

    private List<int> GeneratePrimesNaive(int n, int firstPrime, int secondPrime)
    {
        var primes = new List<int>();
        primes.Add(firstPrime);
        var nextPrime = secondPrime;
        while (primes.Count < n)
        {
            var sqrt = (int)Math.Sqrt(nextPrime);
            var isPrime = true;
            for (var i = 0; primes[i] <= sqrt; i++)
            {
                if (nextPrime % primes[i] == 0)
                {
                    isPrime = false;
                    break;
                }
            }
            if (isPrime)
            {
                primes.Add(nextPrime);
            }
            nextPrime += 2;
        }
        return primes;
    }
}