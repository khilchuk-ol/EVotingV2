using Client.Services;
using Server.Services;

namespace Client.Console
{
    internal class Terminal
    {
        private readonly ClientVotingService votingService;

        private readonly DataProviderService dataProvider;

        public Terminal(ClientVotingService votingService, DataProviderService dataProvider)
        {
            this.votingService = votingService;
            this.dataProvider = dataProvider;
        }

        public void Loop()
        {
            while (true)
            {
                WelcomeMessage();

                try
                {
                    var input = ReadInputs();
                    votingService.Vote(input.UserId, input.CandidateId);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("\nError: " + e.Message);
                }
                
                System.Console.WriteLine("Press [f/F] - to see the results; [r/R] - to restart session; [e/E] - to exit");
                var k = System.Console.ReadKey().KeyChar.ToString().ToLower();

                switch (k)
                {
                    case "e":
                        return;
                    case "r":
                        System.Console.Clear();

                        break;
                    case "f":
                        Results();

                        return;
                }
            }
        }

        private void Results()
        {
            var res = dataProvider.GetVotingResults();
            
            System.Console.Clear();
            System.Console.WriteLine("\n--------- RESULTS ---------");
            foreach (var r in res)
            {
                System.Console.WriteLine(r);
            }
            
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }

        private (int UserId, int CandidateId) ReadInputs()
        {
            var idStr = ReadString("Identificator (IPN)");
            int userId;

            while (!int.TryParse(idStr, out userId))
            {
                System.Console.WriteLine("Invalid input, try again");
                System.Console.Write("Enter your personal id: ");
                idStr = System.Console.ReadLine();
            }

            var candidateId = ReadCandidate();
            
            return (userId, candidateId);
        }

        private int ReadCandidate()
        {
            var candidates = dataProvider.GetAllCandidates();
            
            foreach (var item in candidates)
            {
                System.Console.WriteLine(item);
            }
            
            System.Console.Write("Enter Id of candidate you want to vote for: ");
            
            int candidateId;
            var input = System.Console.ReadLine();
            while (!int.TryParse(input, out candidateId))
            {
                System.Console.WriteLine("Invalid input, try again");
                System.Console.Write("Enter Id of candidate you want to vote for: ");
                input = System.Console.ReadLine();
            }
            
            return candidateId;
        }

        private DateOnly ReadDate(string v)
        {
            System.Console.Write($"Enter your {v}: ");
            
            DateOnly date;
            var input = System.Console.ReadLine();
            while (!DateOnly.TryParse(input, out date))
            {
                System.Console.WriteLine("Invalid input, try again");
                System.Console.Write($"Enter your {v}: ");
                input = System.Console.ReadLine();
            }
            
            return date;
        }

        private string ReadString(string v)
        {
            System.Console.Write($"Enter your {v}: ");
            
            var input = System.Console.ReadLine();
            while (input == null)
            {
                System.Console.WriteLine("Invalid input, try again");
                System.Console.Write($"Enter your {v}: ");
                input = System.Console.ReadLine();
            }
            
            return input;
        }

        private void WelcomeMessage()
        {
            System.Console.WriteLine("Welcome");
        }
    }
}

