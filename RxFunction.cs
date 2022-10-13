using System;
using System.IO;
using System.Security.Cryptography;

namespace R3xLib
{
    public class RxFunction
    {
        public static string SHA256CheckSum(string filePath)
        {
            using (SHA256 sh = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return Convert.ToBase64String(sh.ComputeHash(fileStream));
            }
        }

        public static bool SHA256Verify(string path, string shaSum)
        {
            string sum = SHA256CheckSum(path);
            return sum == shaSum;
        }
    }
}
