using System.Security.Cryptography;
using Server.Encoding;
using Server.Entity;

namespace Server.Services.Strategies;

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
    
    public void Vote(Bulletin b)
    {
        var decrypted = _votingCenter.ApplyPrivateKey(b.Message);
        var bulletinId = BitConverter.ToInt32(decrypted);

        var bulletin = _dataProvider.UnwrapBulletinId(bulletinId);

        var user = _dataProvider.GetUserById(bulletin.UserId);
        if (user == null)
        {
            throw new Exception("user not found");
        }

        if (b.Id != 0 && b.Id != bulletinId)
        {
            throw new Exception("bulletin id data missmatch");
        }
        
        ValidateUserAlreadyVoted(bulletin.UserId);

        if (!user.CheckIfSigned(b.Sign, _gammaEncoder.EncodeBinary(decrypted)))
        {
            throw new Exception("sign data and message data mismatched; signed incorrectly");
        }

        b.Id = bulletinId;
        
        _dataProvider.SaveBulletin(b);
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