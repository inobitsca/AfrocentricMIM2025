using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MIMModels
{
    public class InoUtils
    {

        /// <summary>
        /// Calculates a standard Sha256 Hash from a string
        /// This algorith is standards based, and creates the same hash as in NodeJS, Java, PHP
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        internal static MVEntry[] FindMVEntries(string attribute_to_check, string checkedName, int v)
        {
            throw new NotImplementedException();
        }
    }
}
