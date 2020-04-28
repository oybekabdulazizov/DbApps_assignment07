using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project01.Helpers
{
    public class Generator
    {
        public static string CreateSalt() 
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        public static string Hash(string password, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2
                                        (password: password, 
                                         salt: Encoding.UTF8.GetBytes(salt), 
                                         prf: KeyDerivationPrf.HMACSHA512, 
                                         iterationCount: 20000,
                                         numBytesRequested: 256/8);

            return Convert.ToBase64String(valueBytes);
        }
    }
}
