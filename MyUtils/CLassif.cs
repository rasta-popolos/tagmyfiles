using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyUtils
{
    public static class CLassif
    {
        public static List<long> classSize()
        {

            List<long> aListeSeuils = new List<long>();
            aListeSeuils.Add(long.Parse("0"));
            aListeSeuils.Add(long.Parse("100").KilobytesToBytes());
            aListeSeuils.Add(long.Parse("500").KilobytesToBytes());
            aListeSeuils.Add(long.Parse("1").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("10").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("20").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("50").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("100").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("200").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("300").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("400").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("500").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("600").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("700").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("800").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("900").MegabytesToBytes());
            aListeSeuils.Add(long.Parse("1").GigabytesToBytes());
            aListeSeuils.Add(long.Parse("2").GigabytesToBytes());
            aListeSeuils.Add(long.Parse("3").GigabytesToBytes());
            aListeSeuils.Add(long.Parse("4").GigabytesToBytes());
            aListeSeuils.Add(long.Parse("5").GigabytesToBytes());

            return aListeSeuils;
        }
    
        /**
        * fonction pour classer des nombres par la méthode quantile
        * @param array $vals : tableau de nombre unique ordonné
        * @param int $nbclasses : nombre de classe à utiliser
        * @return array $lesSeuils: un tableau des seuils 
        **/
        public static List<long> classQua(List<long> vals, int nbclasses)
        {
            List<long> lesSeuils = new List<long>();
            long max = vals.Max(t => long.Parse(t.ToString()));

            long seuilOld = vals.Min(t => long.Parse(t.ToString()));
            long seuil1;
            long seuil2;
            for (int i = 0; i < nbclasses; i++)
            {
                seuil1 = seuilOld;

                seuil2 = vals[((i+1) * (vals.Count / nbclasses))-1];
                seuilOld = seuil2;
            
                lesSeuils.Add(seuil1);
                lesSeuils.Add(seuil2);

            }
            if (lesSeuils[lesSeuils.Count - 1] <= max)
                lesSeuils[lesSeuils.Count - 1] = max + 1;
        
            return lesSeuils;
        } 
    
        /**
        * fonction pour classifier des nombres par la méthode moyenne
        * @param array $vals : tableau de nombre unique ordonné
        * @param int $nbclasses : nombre de classe à utiliser
        * @return array : un tableau des seuils 
        **/
        public static List<long> classMoyenne(List<long> vals, int nbclasses)
        {
            List<long> seuils = new List<long>();

            long min = vals[0];
            long max = vals[vals.Count - 1];

            int N = vals.Count;
            long SUMX = 0;
            long SUMX2 = 0;

            for (int j = 0; j < vals.Count ; j++)
            {
                if (vals[j] > 0)
                {
                    SUMX += vals[j]; //somme des valeurs
                    SUMX2 += (vals[j] * vals[j]); // somme des carrés
                }
            }
            double AVGX = SUMX / N;
            double STDVX = Math.Sqrt((SUMX2 / N)- (AVGX * AVGX));


            double factor = 1.5;
            while((AVGX - (factor * STDVX) < min) || (AVGX + (factor * STDVX) > max))
            {
                factor -= 0.1 ;
            }
            seuils.Add(min);
            seuils.Add((long) (AVGX - (factor * STDVX)));
            seuils.Add((long) (AVGX - (factor * STDVX)));
            seuils.Add((long) (AVGX - (factor/3 * STDVX)));
            seuils.Add((long) (AVGX - (factor/3* STDVX)));
            seuils.Add((long) (AVGX + (factor/3 * STDVX)));
            seuils.Add((long) (AVGX + (factor/3 * STDVX)));
            seuils.Add((long) (AVGX + (factor * STDVX)));
            seuils.Add((long) (AVGX + (factor * STDVX)));
            seuils.Add(max + 1);

            return(seuils);
        }

        /**
        * fonction pour classifier des nombres par la méthode amplitude
        * @param array $vals : tableau de nombre unique ordonné
        * @param int $nbclasses : nombre de classe à utiliser
        * @return array $lesSeuils: un tableau des seuils 
        **/
        public static List<long> classAmp(List<long> vals, int nbclasses)
        {
            List<long> lesSeuils = new List<long>();

            long min = vals[0];
            long max = vals[vals.Count - 1];
            long amplitude = max - min;
            long largeurClasse = amplitude / nbclasses;
            for (int i = 0; i < nbclasses; i++)
            {
                long seuil1 = min + (i * largeurClasse);
                long seuil2 = min + ((i + 1) * largeurClasse);
                lesSeuils.Add(seuil1);
                lesSeuils.Add(seuil2);
            }
            if (lesSeuils[lesSeuils.Count - 1] <= max)
                lesSeuils[lesSeuils.Count - 1] = max + 1;
        
            return lesSeuils;
        }
    }
}
