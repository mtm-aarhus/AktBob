using System.Security.Cryptography;
using System.Text;

namespace AktBob.Database.Extensions;
internal static class StringExtensions
{
    public static string GetHash(this string str)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));

            var sb = new StringBuilder();
            foreach (var b in hashedBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
