using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyUtils
{
    public static class StringUtils
    {
        /// <summary>
        /// Uppercase first char
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string UcFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        /// <summary>
        /// Test si un string représente un nombre
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string s)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(s), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        /// <summary>
        /// Converti un string en int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ToInt(this string s)
        {
            int iRes;
            bool res = int.TryParse(s, out iRes);
            if (res == false)
            {
                return 0;
            }
            return iRes;
        }

        /// <summary>
        /// Remplace pas un RegEx
        /// </summary>
        /// <param name="str">Subject</param>
        /// <param name="regmask">pattern</param>
        /// <param name="strreplace">replacement</param>
        /// <returns></returns>
        public static string RegReplace(this string str, string pattern, string replacement)
        {
            Regex rgx = new Regex(pattern, RegexOptions.Multiline);
            return rgx.Replace(str, replacement);
        }

        /// <summary>
        /// Split with RegExp
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<string> RegSplit(this string str, string pattern)
        {
            Regex rgx = new Regex(pattern, RegexOptions.Multiline);
            return rgx.Split(str).ToList();
        }

        /// <summary>
        /// Split with Regex ignore empty parts
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<string> RegSplitNoEmpty(this string str, string pattern)
        {
            Regex rgx = new Regex(pattern, RegexOptions.Multiline);
            return rgx.Split(str).Where(s => s != String.Empty).ToList();
        }

        /// <summary>
        /// Rtounre la chaine md5 d'un string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5(this string str)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                //string hash = GetMd5Hash(md5Hash, str);
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }
}
