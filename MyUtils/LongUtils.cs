using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyUtils
{
    public static class LongUtils
    {
        /// <summary>
        /// Format seconde en hh mm ss
        /// </summary>
        /// <param name="lSize"></param>
        /// <returns></returns>
        public static string FormatSecond(this float s)
        {
            TimeSpan time = TimeSpan.FromSeconds(s);

            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            return time.ToString(@"hh\:mm\:ss\:fff");
        }

        /// <summary>
        /// Format un long pour au format B lisible
        /// </summary>
        /// <param name="lSize"></param>
        /// <returns></returns>
        public static string FileSizeFormat(this long lSize)
        {
            double size = lSize;
            int index = 0;
            for (; size > 1024; index++)
                size /= 1024;
            return size.ToString("0.000 " + new[] { "B", "KB", "MB", "GB", "TB" }[index]);
        }
        
        /// <summary>
        /// Format un long pour au format B lisible
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ToFileSize(this long size)
        {
            if (size < 1024)
            {
                return (size).ToString("F0") + " bytes";
            }
            else if (size < Math.Pow(1024, 2))
            {
                return (size / 1024).ToString("F0") + " KB";
            }
            else if (size < Math.Pow(1024, 3))
            {
                return (size / Math.Pow(1024, 2)).ToString("F0") + " MB";
            }
            else if (size < Math.Pow(1024, 4))
            {
                return (size / Math.Pow(1024, 3)).ToString("F0") + " GB";
            }
            else if (size < Math.Pow(1024, 5))
            {
                return (size / Math.Pow(1024, 4)).ToString("F0") + " TB";
            }
            else if (size < Math.Pow(1024, 6))
            {
                return (size / Math.Pow(1024, 5)).ToString("F0") + " PB";
            }
            else
            {
                return (size / Math.Pow(1024, 6)).ToString("F0") + " EB";
            }
        }

        /// <summary>
        /// Convert bytes to megabytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double BytesToMegabytes(this long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        /// <summary>
        /// Convert kilobytes to megabytes.
        /// </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static double KilobytesToMegabytes(this long kilobytes)
        {
            return kilobytes / 1024f;
        }

        /// <summary>
        /// Convert kilobytes to bytes.
        /// </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static long KilobytesToBytes(this long kilobytes)
        {
            return kilobytes * 1024;
        }

        /// <summary>
        /// Convert megabytes to bytes.
        /// </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static long MegabytesToBytes(this long megabytes)
        {
            return megabytes * 1024 * 1024;
        }

        /// <summary>
        /// Convert gigabytes to bytes.
        /// </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static long GigabytesToBytes(this long gigabytes)
        {
            return gigabytes * 1024 * 1024 * 1024;
        }
    }
}
