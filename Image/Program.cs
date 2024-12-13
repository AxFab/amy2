using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography;

namespace Image
{
    internal class Program
    {

        class ImgRow
        {
            public List<Color> Pixels = new List<Color>();
            public string Hash;
        }

        static string dir = @"C:\Users\Aesga\Dropbox\Chargements appareil photo\";

        static List<ImgRow> AllRows = new List<ImgRow>();
        private static readonly int MinRowMatch = 10;

        static void Add(string file)
        {
            var img = Bitmap.FromFile(dir + file) as Bitmap;
            var rows = new List<ImgRow>();
            for (int i = 0; i < img.Height; i++)
            {
                var sha = SHA1.Create();
                var row = new ImgRow();
                for (int j = 0; j < img.Width; j++)
                {
                    var px = img.GetPixel(j, i);
                    row.Pixels.Add(px);
                    sha.TransformBlock([px.R, px.G, px.B], 0, 3, null, 0);
                }
                sha.TransformFinalBlock([], 0, 0);
                row.Hash = string.Join("", sha.Hash.Select(x => $"{x:x2}"));
                // Console.WriteLine($"Row{i} : {row.Hash}");

                rows.Add(row);
            }

            if (AllRows.Count == 0) {
                AllRows.AddRange(rows);
                return;
            }

            // Faire le diff entre A & B Mark les bandes identiques
            var matches = new List<int[]>();
            for (var i = 0; i <rows.Count - 2; ++i)
            {

                for (var j = 0; j < AllRows.Count; ++j)
                {
                    if (rows[i].Hash != AllRows[j].Hash)
                        continue;

                    // Start at i-j;
                    var l = 1;
                    while (i + l < rows.Count && j + l < AllRows.Count)
                    {
                        if (AllRows[j + l].Hash == rows[i + l].Hash)
                            l++;
                        else
                            break;
                    }

                    if (matches.Any(x => x[1] < j && x[1] + x[2] >= j + l)  || matches.Any(x => x[0] < i && x[0] + x[2] >= i + l) || l < MinRowMatch)
                        continue;

                    Console.WriteLine($"Find match of row {i} against {j}, for {l} rows");
                    matches.Add([i, j, l]);
                }

            }


        }

        static void Export()
        {
            var img = new Bitmap(AllRows.First().Pixels.Count, AllRows.Count);
            for (var i = 0; i < AllRows.Count; ++i)
            {
                var row = AllRows[i];
                for (int j = 0; j < img.Width; j++)
                {
                    img.SetPixel(j, i, row.Pixels[j]);
                }
            }

            img.Save(dir + "merged.png");
        }


        static void Main(string[] args)
        {
            // "2024-02-24 11.50.02.jpg"


            // Tinder
            Add("2024-02-24 11.48.24.jpg");
            Add("2024-02-24 11.48.30.jpg");


            // Add("2024-02-24 11.50.12.jpg");
            // Add("2024-02-24 11.50.21.jpg");
            // Add("2024-02-24 11.50.26.jpg");
            // Add("2024-02-24 11.50.30.jpg");
            // Add("2024-02-24 11.50.35.jpg");
            // Add("2024-02-24 11.50.41.jpg");
            // Add("2024-02-24 11.50.47.jpg");
            // Add("2024-02-24 11.50.51.jpg");
            Export();
        }

    }
}
