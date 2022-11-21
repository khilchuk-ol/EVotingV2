using Server.Entity;

namespace Server.DataSource;

public class InMemoryDataSource
{
    public List<User> Users = new();

    public List<Candidate> Candidates = new();
    
    internal List<Bulletin> Bulletins = new();
    
    internal List<VoteResult> VoteResults = new();

    public InMemoryDataSource()
    {
        Seed();
    }

    public int GenerateBulletinId(int userId, int candidateId)
    {
        return userId * Candidates.Count + candidateId;
    }

    public (int UserId, int CandidateId) UnwrapBulletinId(int id)
    {
        return (id / Candidates.Count, id % Candidates.Count);
    }

    private void Seed()
    {
        Candidates.AddRange(new[] 
        {
            new Candidate() { Id = 1, Name = "Thelensky" },
            new Candidate() { Id = 2, Name = "Prytula" },
            new Candidate() { Id = 4, Name = "Sternenko" },
        });

        Users.AddRange(new[]
        {
            new User
            {
                Id = 11111111,
                CanVote = true
            },
            new User
            {
                Id = 11111112,
                CanVote = true
            },
            new User
            {
                Id = 11111113,
                CanVote = true
            },
            new User
            {
                Id = 11111114,
                CanVote = true
            },
            new User
            {
                Id = 11111115,
                CanVote = true
            },
            new User
            {
                Id = 11111116,
                CanVote = false
            },
            new User
            {
                Id = 11111117,
                CanVote = false
            },
            new User
            {
                Id = 11111118,
                CanVote = true
            },
            new User
            {
                Id = 11111119,
                CanVote = true
            },
        });
    }
}