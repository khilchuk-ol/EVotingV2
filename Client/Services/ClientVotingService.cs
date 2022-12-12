using Client.Logic;
using Client.Services.Strategies;
using Server.Entity;
using Server.Models;
using Server.Services;
using Server.Services.Registration;

namespace Client.Services;

public class ClientVotingService
{
    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenterService;

    private RegistrationCenterService _registrationCenter;

    private UsersVotingStrategy _strategy;

    private List<User> _voters;

    public ClientVotingService(
        DataProviderService dataProviderService, 
        VotingCenterService votingCenterService, 
        RegistrationCenterService registrationCenter)
    {
        _dataProvider = dataProviderService;
        _votingCenterService = votingCenterService;
        _registrationCenter = registrationCenter;
        
        // lab4
        _voters = _dataProvider.GetVoters(4);
        if (_voters.Count != 4)
        {
            throw new Exception("not enough voters");
        }
        
        var a = new VoterChain(_voters[0]);
        var b = new VoterChain(_voters[1]);
        var c = new VoterChain(_voters[2]);
        var d = new VoterChain(_voters[3]);

        // next - encryption order
        // prev - decryption order
        
        d.Next = c;
        c.Next = b;
        b.Next = a;

        a.Prev = b;
        b.Prev = c;
        c.Prev = d;

        var strategy = new UsersVotingStrategy(_dataProvider, d);
        _strategy = strategy;
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
        //var strategy = new PolyVotingStrategy(_dataProvider, _votingCenterService, _registrationCenter);
        //strategy.Vote(user, candidateId);
        
        // lab 4
        if (!_voters.Contains(user))
        {
            throw new Exception("users is not chosen for registration or cannot vote");
        }

        _strategy.Vote(user, candidateId);
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

    public IEnumerable<VotingResultsModel> GetVotingResults()
    {
        //return _dataProvider.GetVotingResults();
        
        // lab 4
        return _strategy.GetResults().Select(r => new VotingResultsModel
        {
            Score = r.Value,
            CandidateName = _dataProvider.GetAllCandidates().FirstOrDefault(c => c.Id == r.Key)?.Name
        });
    }
}