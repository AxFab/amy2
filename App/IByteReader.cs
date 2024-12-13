namespace App
{
    public interface IByteReader
    {
        byte NextByte();
        bool Continue();
        int ReadLength();
        byte[] ReadLast();
    }
}
