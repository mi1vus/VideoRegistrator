namespace MyHomeSecureWeb.Utilities
{
    public interface IPasswordHash
    {
        string CreateToken(int size);
        byte[] CreateSalt(int size);
        byte[] Hash(string value, byte[] salt);
        byte[] Hash(byte[] value, byte[] salt);
    }
}