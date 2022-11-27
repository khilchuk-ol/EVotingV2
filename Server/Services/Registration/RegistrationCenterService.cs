namespace Server.Services.Registration;

public class RegistrationCenterService
{
    private Dictionary<int, long> _usersRegNumbers = new();

    private HashSet<long> _regNumbers = new();
    
    private HashSet<long> _regNumbersBackup = new();

    public List<long> RegistrationNumbers => _regNumbersBackup.ToList();

    public RegistrationCenterService(DataProviderService dataProvider)
    {
        for (int i = 0; i < dataProvider.GetUsersCount() + dataProvider.GetUsersCount() / 4; i++)
        {
            long r = LongRandom(100000000000000000, 100000000000000050, new Random());

            while (!_regNumbers.Add(r))
            {
                r = LongRandom(100000000000000000, 100000000000000050, new Random());
            }

            _regNumbersBackup.Add(r);
        }
    }

    public long GenerateRegNumber(int id)
    {
        if (_usersRegNumbers.ContainsKey(id))
        {
            throw new Exception("user already got reg number");
        }

        var r = _regNumbers.First();
        _usersRegNumbers.Add(id, r);
        _regNumbers.Remove(r);

        return r;
    }

    public long GetExistingRegNumber(int id)
    {
        return _usersRegNumbers[id];
    }
    
    private long LongRandom(long min, long max, Random rand) {
        byte[] buf = new byte[8];
        rand.NextBytes(buf);
        long longRand = BitConverter.ToInt64(buf, 0);

        return (Math.Abs(longRand % (max - min)) + min);
    }
}