using System.Security.Cryptography;
using Server.Encoding;
using Server.Entity;
using Server.Services.Registration;

namespace Server.Services.Strategies;

public class PolyVotingStrategy
{
    private GammaEncoder _gammaEncoder = new(7003189);

    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenter;
    
    private List<long> _regNumbers;

    private List<int> _votedUsers = new();
    
    public PolyVotingStrategy(
        DataProviderService dataProviderService, 
        VotingCenterService votingCenterService, 
        RegistrationCenterService registrationCenter)
    {
        _dataProvider = dataProviderService;
        _votingCenter = votingCenterService;

        _regNumbers = new List<long>(registrationCenter.RegistrationNumbers.ToArray());
    }

    public void Vote(Bulletin b, long regNumber)
    {
        if (_votedUsers.Contains(b.Id))
        {
            throw new Exception("user already voted");
        }
        
        if (!_regNumbers.Contains(regNumber))
        {
            throw new Exception("registration number does not exist");
        }

        if (!_votingCenter.CheckIfSigned(b.Message, b.Sign))
        {
            throw new Exception("message is not signed");
        }

        var bulletinId = _votingCenter.ApplyPrivateKey(b.Message);
        var bulletin = _dataProvider.UnwrapBulletinId(BitConverter.ToInt32(bulletinId));

        if (bulletin.UserId != b.Id)
        {
            throw new Exception("user ids mismatch");
        }
        
        _votedUsers.Add(b.Id);
        _regNumbers.Remove(regNumber);
        
        ValidateUserAlreadyVoted(b.Id);

        _dataProvider.SaveVoteResult(new VoteResult(b.Id, bulletin.CandidateId));
    }
    
    public void ValidateUserAlreadyVoted(int userId)
    {
        if (_dataProvider.GetAllVoteResults().Exists(vr => vr.UserId == userId))
        {
            throw new Exception("user already voted");
        }
    }
}