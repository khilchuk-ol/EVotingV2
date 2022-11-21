namespace Server.Models;

public class VotingResultsModel
{
    public string CandidateName { get; set; }
    
    public int Score { get; set; }
    
    public override string ToString()
    {
        return $"{CandidateName}: {Score}";
    }
}