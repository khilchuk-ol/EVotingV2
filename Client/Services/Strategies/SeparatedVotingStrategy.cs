using System.Security.Cryptography;
using Server.Entity;
using Server.Services;

namespace Client.Services.Strategies;

public class SeparatedVotingStrategy
{
    private DataProviderService _dataProvider;

    private VotingCenterService _votingCenter;

    public SeparatedVotingStrategy(DataProviderService dataProviderService, VotingCenterService votingCenterService)
    {
        _dataProvider = dataProviderService;
        _votingCenter = votingCenterService;
    }
    
    public void Vote(User user, int candidateId)
    {
        var (a, b) = BreakIntoMultipliers(candidateId);

        var encrypted1 = _votingCenter.Encrypt(a).ToByteArray();
        var encrypted2 = _votingCenter.Encrypt(b).ToByteArray();

        var signed1 = _votingCenter.SeparatedVotingStrategies[0].Sign(encrypted1);
        var signed2 = _votingCenter.SeparatedVotingStrategies[1].Sign(encrypted2);

        _votingCenter.SeparatedVotingStrategies[0].Accept(user.Id, encrypted1, signed1);
        _votingCenter.SeparatedVotingStrategies[1].Accept(user.Id, encrypted2, signed2);
    }

    private (int a, int b) BreakIntoMultipliers(int n)
    {
        for (int i = 2; i <= n; i++)
        {
            if (n % i == 0) return (i, n / i);
        }

        return (1, n);
    }
}