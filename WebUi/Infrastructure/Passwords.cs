using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebUi.Infrastructure
{
    public static class Passwords
    {
        private const int SaltLength = 16;
        private const int HashRoundsCount = 5000;

        public static string Hash(string value, string salt)
        {
            //http://security.stackexchange.com/questions/4781/do-any-security-experts-recommend-bcrypt-for-password-storage
            var i = salt.IndexOf(';');
            var rounds = Int32.Parse(salt.Substring(0, i));
            var actualSalt = FromHex(salt.Substring(i + 1));
            var valueBytes = Encoding.UTF8.GetBytes(value);
            using (var b = new Rfc2898DeriveBytes(valueBytes, actualSalt, rounds))
            { //http://en.wikipedia.org/wiki/PBKDF2
                return ToHex(b.GetBytes(24));
            }
        }

        public static string Hash(string value, out string salt)
        {
            var saltLocal = ToHex(CreateRandomBytes(SaltLength));
            salt = String.Format("{0};{1}", HashRoundsCount, saltLocal);
            return Hash(value, salt);
        }

        public static byte[] FromHex(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            if (hex.Length % 2 != 0)
                throw new FormatException("Should be a hex string");

            var s = hex.ToLowerInvariant().Replace("-", "");
            var n = s.Length;
            var bytes = new byte[n / 2];
            for (var i = 0; i < n; i += 2)
                bytes[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return bytes;
        }

        public static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static byte[] CreateRandomBytes(int byteCount)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[byteCount];
                rng.GetBytes(bytes);
                return bytes;
            }
        }
    }
}