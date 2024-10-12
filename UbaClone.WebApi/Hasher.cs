using System.Security.Cryptography;
using System.Text; // To use HMACSHA512


namespace UbaClone.WebApi;

public class Hasher
{
    public static void CreateValueHash(string value, out byte[] valueHash, out byte[] valueSalt)
    {
        using (var hmac = new HMACSHA512())
        {
           valueSalt = hmac.Key; // generate a salt (a random key)
            valueHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value)); // hash password
        }
    }

    public static bool VerifyValue(string value, byte[] valueHash, byte[] valueSalt)
    {
        using (var hmac = new HMACSHA512(valueSalt))
        {
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            return computeHash.SequenceEqual(valueHash);
        }
    }


}
