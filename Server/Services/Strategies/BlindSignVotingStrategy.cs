using Server.Encoding;
using Server.Entity;
using Server.Models;

namespace Server.Services.Strategies;

public class BlindSignVotingStrategy
{
    private List<long> _checkedUsers = new();
    
    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenter;

    public BlindSignVotingStrategy(DataProviderService dataProviderService, VotingCenterService votingCenterService)
    {
        _dataProvider = dataProviderService;
        _votingCenter = votingCenterService;
    }

    public BlindPackageModel Sign(object msgs)
    {
        var bundle = (BundleModel)msgs;
        
        if (_checkedUsers.Contains(bundle.Id))
        {
            throw new Exception("user has already sent bulletins");
        }
        
        var random = new Random();
        var uncheckedIdx = random.Next(0, bundle.Packages.Count);

        for (int i = 0; i < bundle.Packages.Count; i++)
        {
            if (i == uncheckedIdx) continue;

            var pkg = bundle.Packages[i];

            for (int j = 0; j < pkg.Messages!.Count; j++)
            {
                var gamma = new GammaEncoder((int)pkg.R);
                
                if (gamma.DecodeBinary(pkg.Messages[j]) != pkg.Bulletins![j])
                {
                    throw new Exception("packages are not valid");
                }
            }
        }
        
        _checkedUsers.Add(bundle.Id);

        var signedBulletins = new List<byte[]>();
        foreach (var m in bundle.Packages[uncheckedIdx].Bulletins!)
        {
            signedBulletins.Add(_votingCenter.SignWithPrivateKey(m));
        }

        return new BlindPackageModel
        {
            R = bundle.Packages[uncheckedIdx].R,
            Messages = bundle.Packages[uncheckedIdx].Messages,
            Bulletins = bundle.Packages[uncheckedIdx].Bulletins,
            SignedBulletins = signedBulletins
        };
    }

    public void Vote(byte[] msg, byte[] signed, byte[] _)
    {
        var bulletinId = _votingCenter.ApplyPrivateKey(msg);
        var bulletin = _dataProvider.UnwrapBulletinId(BitConverter.ToInt32(bulletinId));

        // check by other means
        if (!_votingCenter.CheckIfSigned(signed))
        {
            throw new Exception("sign data and message data mismatched; signed incorrectly");
        }

        ValidateUserAlreadyVoted(bulletin.UserId);
        
        _dataProvider.SaveVoteResult(new VoteResult(bulletin.UserId, bulletin.CandidateId));
    }
    
    public void ValidateUserAlreadyVoted(int userId)
    {
        if (_dataProvider.GetAllVoteResults().Exists(vr => vr.UserId == userId))
        {
            throw new Exception("user already voted");
        }
    }
}