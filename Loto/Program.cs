using System.Reflection.Metadata;

namespace Loto
{
    internal class Program
    {
        // https://www.loterieplus.com/euromillions/resultat/derniers-resultats.php
        // var t = [...document.getElementsByClassName("lp4")[0].childNodes[0].childNodes]
        // t.filter(x => x.className == 'ligne1' || x.className == 'ligne2').map(x => `${x.firstElementChild.lastChild.textContent}, ${[...x.firstElementChild.nextSibling.nextSibling.firstElementChild.firstElementChild.firstElementChild.childNodes].map(x => x.innerText).join('.')}`)

        string[] result50 = new string[]
        {
            "23/02/2024, 24.27.28.30.49.1.12", "20/02/2024, 23.31.37.42.48.3.7", "16/02/2024, 8.13.14.24.26.1.2", "13/02/2024, 13.17.18.20.46.4.9", 
            "09/02/2024, 23.24.35.37.45.9.12", "06/02/2024, 2.7.21.28.45.5.11", "02/02/2024, 13.20.23.27.42.5.9", "30/01/2024, 5.10.19.27.30.5.6", 
            "26/01/2024, 8.19.32.41.42.9.12", "23/01/2024, 14.23.39.48.50.3.12", "19/01/2024, 27.28.44.48.50.7.12", "16/01/2024, 10.18.21.33.45.8.12", 
            "12/01/2024, 16.17.18.45.49.9.12", "09/01/2024, 2.9.12.39.40.1.3", "05/01/2024, 4.7.18.39.50.3.8", "02/01/2024, 7.15.18.46.49.10.12", 
            "29/12/2023, 2.3.19.36.37.6.9", "26/12/2023, 8.27.30.35.47.9.10", "22/12/2023, 6.14.34.44.49.4.12", "19/12/2023, 10.20.41.43.45.2.12", 
            "15/12/2023, 2.13.37.38.48.5.9", "12/12/2023, 6.28.37.39.43.9.12", "08/12/2023, 17.30.42.48.50.4.8", "05/12/2023, 4.6.20.24.25.5.9",
            "01/12/2023, 4.10.14.38.50.9.12", "28/11/2023, 12.16.27.33.44.7.8", "24/11/2023, 15.20.29.39.48.1.7", "21/11/2023, 19.29.34.46.47.2.3", 
            "17/11/2023, 2.24.26.46.50.2.7", "14/11/2023, 13.16.36.44.50.3.5", "10/11/2023, 10.21.30.38.42.2.12", "07/11/2023, 8.10.11.30.39.4.10", 
            "03/11/2023, 8.21.31.39.47.5.9", "31/10/2023, 5.7.20.40.50.2.10", "27/10/2023, 29.33.35.48.49.3.8", "24/10/2023, 8.16.18.31.34.6.9", 
            "20/10/2023, 2.20.28.40.45.1.5", "17/10/2023, 10.17.20.35.40.3.4", "13/10/2023, 21.26.28.40.41.2.4", "10/10/2023, 18.20.22.33.43.3.9", 
            "06/10/2023, 21.29.31.34.43.2.9", "03/10/2023, 6.20.22.24.45.4.5", "29/09/2023, 9.11.13.21.32.2.7", "26/09/2023, 2.6.14.19.23.5.7", 
            "22/09/2023, 3.23.24.34.35.5.8", "19/09/2023, 10.15.31.41.42.2.5", "15/09/2023, 12.14.21.45.48.8.11", "12/09/2023, 5.14.36.40.42.2.11", 
            "08/09/2023, 10.20.21.26.33.3.4", "05/09/2023, 1.7.24.41.48.10.12"
        };

        class Tirage
        {
            public DateTime Date;
            public int[] Numbers;

            public int[] NumOrdered => Numbers.Take(5).OrderBy(x => x).ToArray();

            public int NumSum => Numbers.Take(5).Sum();
        }

        static List<Tirage> Tirages = new List<Tirage>();
        static int[] Histogram = new int[51];
        static int[] HistogramStart = new int[13];
        static int[] HistogramSum = new int[50+49+48+47+46];
        static void ReadCSV(string name, int idx)
        {
            var csv = new StreamReader(File.OpenRead(@"C:\Users\Aesga\Downloads\euromillions\" + name));
            csv.ReadLine();
            for (; ;)
            {
                var line = csv.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;
                var data = line.Split(';');
                var date = data[2];
                var bl1 = int.Parse(data[idx]);
                var bl2 = int.Parse(data[idx+1]);
                var bl3 = int.Parse(data[idx+2]);
                var bl4 = int.Parse(data[idx+3]);
                var bl5 = int.Parse(data[idx+4]);
                var et1 = int.Parse(data[idx+5]);
                var et2 = int.Parse(data[idx+6]);
                var tr = new Tirage
                {
                    Date = ParseDate(date),
                    Numbers = new int[] { bl1, bl2, bl3, bl4, bl5, et1, et2 },
                };
                Tirages.Add(tr);
                Histogram[bl1]++;
                Histogram[bl2]++;
                Histogram[bl3]++;
                Histogram[bl4]++;
                Histogram[bl5]++;
                HistogramStart[et1]++;
                HistogramStart[et2]++;
                HistogramSum[tr.NumSum]++;
            }
        }

        private static DateTime ParseDate(string date)
        {
            if (date.Length == 8 && date.StartsWith("20"))
            {
                return new DateTime(int.Parse(date[0..4]), int.Parse(date[4..6]), int.Parse(date[6..8]));
            }
            if (date.Length == 10 && date[2] == '/' && date[5] == '/' && date[6] == '2' && date[7] == '0')
            {
                return new DateTime(int.Parse(date[6..10]), int.Parse(date[3..5]), int.Parse(date[0..2]));
            }
            if (date.Length == 8 && date[2] == '/' && date[5] == '/' && date[0] == '2')
            {
                return new DateTime(2000 + int.Parse(date[0..2]), int.Parse(date[3..5]), int.Parse(date[6..8]));
            }
            throw new Exception();
        }

        static void Main(string[] args)
        {
            ReadCSV("euromillions.csv", 4);
            ReadCSV("euromillions_2.csv", 4);
            ReadCSV("euromillions_3.csv", 4);
            ReadCSV("euromillions_4.csv", 5);
            ReadCSV("euromillions_201902.csv", 5);
            ReadCSV("euromillions_202002.csv", 5);
        }
    }
}
