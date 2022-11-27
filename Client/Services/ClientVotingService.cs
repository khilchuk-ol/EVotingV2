using Client.Services.Strategies;
using Server.Entity;
using Server.Services;
using Server.Services.Registration;

namespace Client.Services;

public class ClientVotingService
{
    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenterService;

    private RegistrationCenterService _registrationCenter;

    public ClientVotingService(
        DataProviderService dataProviderService, 
        VotingCenterService votingCenterService, 
        RegistrationCenterService registrationCenter)
    {
        _dataProvider = dataProviderService;
        _votingCenterService = votingCenterService;
        _registrationCenter = registrationCenter;
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
        //var strategy = new SimpleVotingStrategy(_dataProvider, _votingCenterService);
        //strategy.Vote(user, candidateId);
        
        // lab 2
        //var strategy = new BlindSignVotingStrategy(_dataProvider, _votingCenterService);
        //strategy.Vote(user, candidateId);
        
        // lab 3
        var strategy = new PolyVotingStrategy(_dataProvider, _votingCenterService, _registrationCenter);
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