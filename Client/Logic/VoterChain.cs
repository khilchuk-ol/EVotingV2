using System.Text;
using ElGamalExt;
using Server.Entity;

namespace Client.Logic;

public class VoterChain
{
    private Dictionary<int, string> usersRandomStrings = new Dictionary<int, string>();

    private List<(byte[] msg, byte[], int id)> bulletins = new();

    public VoterChain? Next { get; set; }
    
    public VoterChain? Prev { get; set; }
    
    private User user { get; set; }
    
    private byte[] myBulletin { get; set; }

    private ElGamalManaged _elgamal;

    public VoterChain(User u)
    {
        user = u;

        _elgamal = new ElGamalManaged();
    }

    public byte[] EncryptRSA(byte[] msg)
    {
        var cur = user.ApplyPublicKey(msg);

        return Next?.EncryptRSA(cur) ?? cur;
    }
    
    public byte[] EncryptRSAWithStr(byte[] msg, int id)
    {
        var randomStr = RandomStringGenerator.Generate();
        var randomStrBytes = Encoding.UTF8.GetBytes(randomStr);

        var newBulletin = msg.Concat(randomStrBytes).ToArray();
        
        usersRandomStrings.Add(id, randomStr);
        
        var cur = user.ApplySecondPublicKey(newBulletin);

        if (Prev != null)
        {
            Prev.SaveMyBulletin(cur, id);
        }

        return Next?.EncryptRSAWithStr(cur, id) ?? cur;
    }
    
    public (byte[] msg, byte[] signed, int id)[] DencryptRSAAndSignBatch((byte[] msg, byte[] signed, int id)[] bulletins)
    {
        var foundMy = false;
        var signed = new List<(byte[] msg, byte[] signed, int id)>();

        foreach (var b in bulletins)
        {
            if (b.id == user.Id)
            {
                foundMy = true;
            }

            if (Next != null && !Next.VerifySign(b.msg, b.signed))
            {
                throw new Exception("invalid sign");
            }
            
            var decrypted = user.ApplyPrivateKey(b.msg);
            var signedMsg = Sign(decrypted);
            
            signed.Add((decrypted, signedMsg, b.id));
        }

        if (myBulletin != null && !foundMy) throw new Exception("where is my bulletin? sign step");

        var signedArr = Shuffle(signed.ToArray());

        return Prev != null ? Prev.DencryptRSAAndSignBatch(signedArr) : signedArr;
    }
    
    private byte[] DencryptRSARemoveStr(byte[] msg, int id)
    {
        var decrypted = user.ApplySecondPrivateKey(msg);

        var str = usersRandomStrings[id];
        if (String.IsNullOrEmpty(str))
        {
            throw new Exception("no random string");
        }
        
        var strBytes = Encoding.UTF8.GetBytes(str);
        
        if (decrypted.TakeLast(strBytes.Length).Except(strBytes).Count() != 0)
        {
            throw new Exception("missing bulletins");
        }

        var withoutStrBytes = decrypted.Take(decrypted.Length - strBytes.Length).ToArray();
        
        return withoutStrBytes;
    }

    public (byte[] msg, byte[], int id)[] DencryptRSARemoveStrBatch((byte[] msg, byte[], int id)[] batch)
    {
        List<(byte[] msg, byte[], int id)> newBatches = new();
        
        foreach (var b in batch)
        {
            var decrypted = DencryptRSARemoveStr(b.msg, b.id);

            newBatches.Add((decrypted, null, b.id));
        }

        if (myBulletin != null && !newBatches.Select(b => b.msg).Contains(myBulletin))
        {
            throw new Exception("where is my bulletin?");
        }

        var newBatchesArr = Shuffle(newBatches.ToArray());
        
        return Prev != null ? Prev.DencryptRSARemoveStrBatch(newBatchesArr) : newBatchesArr;
    }

    public byte[] Sign(byte[] msg)
    {
        //return _elgamal.Sign(msg);
        return user.SignWithPrivateKey(msg);
    }
    
    public bool VerifySign(byte[] msg, byte[] signature)
    {
        //return _elgamal.VerifySignature(msg, signature);
        return user.CheckIfSigned(signature, msg);
    }

    public void AcceptBulletin(byte[] msg, int id)
    {
        if (bulletins.Any(b => b.id == id))
        {
            throw new Exception("user already voted");
        }
        
        bulletins.Add((msg, null, id));
    }
    
    public List<(byte[] msg, byte[], int id)> GetBulletins()
    {
        return bulletins;
    }

    public void SaveMyBulletin(byte[] msg, int id)
    {
        if (id != user.Id)
        {
            return;
        }

        myBulletin = msg;
    }

    private (byte[] msg, byte[] signed, int id)[] Shuffle((byte[] msg, byte[] signed, int id)[] arr)
    {
        Random random = new Random();
        arr = arr.OrderBy(x => random.Next()).ToArray();

        return arr;
    }
}