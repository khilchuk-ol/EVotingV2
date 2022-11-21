using Client.Services.Strategies;
using Server.Entity;
using Server.Services;

namespace Client.Services;

public class ClientVotingService
{
    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenterService;

    public ClientVotingService(DataProviderService dataProviderService, VotingCenterService votingCenterService)
    {
        _dataProvider = dataProviderService;
        _votingCenterService = votingCenterService;
    }
    
    public void Vote(int userId, int candidateId)
    {
        var user = _dataProvider.GetUserById(userId);
        if (user == null)
        {
            throw new Exception("User with such personal id does not exist");
        }
        
        ValidateData(user, candidateId);
        
        // lab 1
        var strategy = new SimpleVotingStrategy(_dataProvider, _votingCenterService);
        strategy.Vote(user, candidateId);
    }

    private void ValidateData(User user, int candidateId)
    {
        if (!user.CanVote)
        {
            throw new Exception("User with such personal id can not vote");
        }

        if (_dataProvider.GetCandidateById(candidateId) == null)
        {
            throw new Exception("Candidate with such id does not exist");
        }
    }
}