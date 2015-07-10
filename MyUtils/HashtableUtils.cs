using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagMyFiles
{
    public static class HashtableUtils
    {
        /// <summary>
        /// Return an ArrayList of Hashtable keys
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ArrayList GetKeys(this Hashtable table)
        {
            return (new ArrayList(table.Keys));
        }

        /// <summary>
        /// Return an ArrayList of Hashtable values
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ArrayList GetValues(this Hashtable table)
        {
            return (new ArrayList(table.Values));
        }

        public static Hashtable KeySort(this Hashtable table)
        {
            Hashtable hash = new Hashtable();
            ArrayList keys = table.GetKeys();
            keys.Sort();
            foreach (object obj in keys)
            {
                hash[obj] = table[obj];
            }
            return hash;
        }

        public static void TestSortKeyValues()
        {
            // Define a hashtable object 
            Hashtable hash = new Hashtable();
            hash.Add(2, "two");
            hash.Add(1, "one");
            hash.Add(5, "five");
            hash.Add(4, "four");
            hash.Add(3, "three");
            // Get all the keys in the hashtable and sort them
            ArrayList keys = GetKeys(hash);
            keys.Sort();
            // Display sorted key list
            foreach (object obj in keys)
                Console.WriteLine("Key: " + obj + " Value: " + hash[obj]);
            // Reverse the sorted key list 
            Console.WriteLine();
            keys.Reverse();
            // Display reversed key list 
            foreach (object obj in keys)
                Console.WriteLine("Key: " + obj + " Value: " + hash[obj]);
            // Get all the ...
        }
    }
}
