namespace Server.Entity;

public class VoteResult
{
    public int UserId { get; }
    
    public int CandidateId { get; }

    public VoteResult(int userId, int candidateId)
    {
        UserId = userId;
        CandidateId = candidateId;
    }
}