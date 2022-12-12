using Server.DataSource;
using Server.Entity;
using Server.Models;

namespace Server.Services;

public class DataProviderService
{
    private InMemoryDataSource source;

    public DataProviderService(InMemoryDataSource dataSource)
    {
        source = dataSource;
    }

    public List<Candidate> GetAllCandidates() => source.Candidates;
    
    internal List<VoteResult> GetAllVoteResults() => source.VoteResults;

    public int GetUsersCount() => source.Users.Count;
    
    public User? GetUserById(int id) => source.Users.FirstOrDefault(u => u.Id == id);
    
    public Candidate? GetCandidateById(int id) => source.Candidates.FirstOrDefault(c => c.Id == id);

    public int GenerateBulletinId(int userId, int candidateId) => source.GenerateBulletinId(userId, candidateId);

    public (int UserId, int CandidateId) UnwrapBulletinId(int id) => source.UnwrapBulletinId(id);

    internal void SaveVoteResult(VoteResult vr) => source.VoteResults.Add(vr);
    
    internal void SaveBulletin(Bulletin b) => source.Bulletins.Add(b);

    public List<User> GetVoters(int limit = -1) => source.Users.FindAll(u => u.CanVote).Take(limit).ToList();
    
    public IEnumerable<VotingResultsModel> GetVotingResults()
    {
        var res = source.VoteResults
            .GroupBy(b => b.CandidateId,
                b => b.CandidateId,
                (key, g) => new { CandidateId = key, Score = g.Count() })
            .ToList();

        return res.Select(r => new VotingResultsModel
        {
            Score = r.Score,
            CandidateName = source.Candidates.FirstOrDefault(c => c.Id == r.CandidateId)?.Name
        });
    }
}