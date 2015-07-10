using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

using MyUtils;

namespace MyUtils
{
    public static class Globalisation
    {
        // Globalisation
        static ResourceManager _resourceManager;
        static CultureInfo _cultureinfo = CultureInfo.CurrentCulture;

        public static ResourceManager ResourceManager
        {
            get
            {
                return _resourceManager;
            }
            set
            {
                _resourceManager = value;
            }
        }

        public static CultureInfo Cultureinfo
        {
            get
            {
                return _cultureinfo;
            }
            set
            {
                _cultureinfo = value;
            }
        }

        /// <summary>
        /// Retourne la valeur du clé dans la resource langue
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            try
            {
                if (!String.IsNullOrEmpty(key))
                {
                    Regex rgx = new Regex("^&");
                    if (rgx.IsMatch(key))
                    {
                        return "&" + Globalisation.ResourceManager.GetString(key.RegReplace("^&", ""), Globalisation.Cultureinfo);
                    }
                    else
                    {
                        return Globalisation.ResourceManager.GetString(key, Globalisation.Cultureinfo);
                    }
                }
                else
                {
                    return "";
                }
            }
            catch {
                return key;
            }
        }

    }
}
