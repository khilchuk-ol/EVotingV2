namespace Server.Encoding;

public class GammaEncoder
{
    private byte[] gamma;

    public GammaEncoder(int gamma = 41)
    {
        this.gamma = BitConverter.GetBytes(gamma);
    }

    public byte[] EncodeBinary(byte[] message)
    {
        if (gamma.Length < message.Length)
        {
            Array.Resize(ref gamma, message.Length);
        }

        for (int i = 0; i < message.Length; i++)
        {
            message[i] = (byte)(message[i] ^ gamma[i]);
        }

        return message;
    }
    
    public byte[] DecodeBinary(byte[] message)
    {
        if (gamma.Length < message.Length)
        {
            Array.Resize(ref gamma, message.Length);
        }

        for (int i = 0; i < message.Length; i++)
        {
            message[i] = (byte)(message[i] ^ gamma[i]);
        }

        return message;
    }
}