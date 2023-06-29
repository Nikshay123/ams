using System.Linq;
using System;
using System.Security.Cryptography;
using System.Text;

namespace TenantManagement.Common
{
    public class CryptoUtils
    {
        public static string GetRandomString(int size = 16)
        {
            Random random = new();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return new string(Enumerable.Repeat(chars, size)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateHash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sBuilder = new StringBuilder();

                // convert to hex
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static string GenerateHashV2(string input)
        {
            using (SHA512 sha512Hash = SHA512.Create())
            {
                byte[] data = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sBuilder = new StringBuilder();

                // convert to hex
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static string HashPassword(string username, string userpass, string salt, bool legacy = false)
        {
            if (legacy)
            {
                return GenerateHash($"{username.ToLower()}:{userpass}:{salt}");
            }

            return GenerateHashV2($"{username.ToLower()}:{userpass}:{salt}");
        }
    }
}