using System.Text;
using Client.Logic;
using Server.Entity;
using Server.Services;

namespace Client.Services.Strategies;

public class UsersVotingStrategy
{
    private DataProviderService _dataProvider;

    private VoterChain _voterChain;

    private Dictionary<int, string> usersStrings = new();

    public UsersVotingStrategy(DataProviderService dataProviderService, VoterChain votersChain)
    {
        _dataProvider = dataProviderService;
        _voterChain = votersChain;
    }
    
    public void Vote(User user, int candidateId)
    {
        var bulletinId = _dataProvider.GenerateBulletinId(user.Id, candidateId);
        var randomStr = RandomStringGenerator.Generate();

        if (usersStrings.ContainsKey(user.Id)) throw new Exception("user already voted");
        
        usersStrings.Add(user.Id, randomStr);

        var bulletinStr = bulletinId + randomStr;

        var encrypted = _voterChain.EncryptRSA(Encoding.UTF8.GetBytes(bulletinStr));
        var encryptedWithStrings = _voterChain.EncryptRSAWithStr(encrypted, user.Id);

        var accepter = _voterChain;
        while (accepter.Next != null)
        {
            accepter = accepter.Next;
        }
        
        accepter.AcceptBulletin(encryptedWithStrings, user.Id);
    }
    
    public Dictionary<int, int> GetResults()
    {
        var results = new Dictionary<int, int>();
        
        var accepter = _voterChain;
        while (accepter.Next != null)
        {
            accepter = accepter.Next;
        }

        var bulletins = accepter.GetBulletins();
        var withoutStrBatch = accepter.DencryptRSARemoveStrBatch(bulletins.ToArray());

        var signed = accepter.DencryptRSAAndSignBatch(withoutStrBatch);

        foreach (var signedBulletin in signed)
        {
            if (!_voterChain.VerifySign(signedBulletin.msg, signedBulletin.signed))
            {
                throw new Exception("invalid sign");
            }

            var str = usersStrings[signedBulletin.id];
            
            var withoutStr = Encoding.UTF8.GetString(signedBulletin.msg);

            if (!withoutStr.Contains(str))
            {
                throw new Exception("missing bulletins");
            }

            withoutStr = withoutStr.Replace(str, "");
            int bulletinId;

            if (!Int32.TryParse(withoutStr, out bulletinId))
            {
                throw new Exception("something went wrong");
            }

            var bulletin = _dataProvider.UnwrapBulletinId(bulletinId);

            if (results.ContainsKey(bulletin.CandidateId))
            {
                results[bulletin.CandidateId] += 1;
            }
            else
            {
                results.Add(bulletin.CandidateId, 1);
            }
        }

        return results;
    }
}