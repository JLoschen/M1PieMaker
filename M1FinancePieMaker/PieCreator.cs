using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace M1FinancePieMaker
{
    public class PieCreator
    {
        private const float _bestBuyPoints = 1;
        private const float _buyRecPoints = 5.3F;

        public void Create()
        {
            var bestBuys = GetRecs(GetLocalPath("BuyRecs.csv"), _bestBuyPoints);
            var buyRecs = GetRecs(GetLocalPath("BestBuysNow.csv"), _buyRecPoints);
            var myPics = GetRecs(GetLocalPath("MyPics.csv"), null);

            var combined = bestBuys.Concat(buyRecs).Concat(myPics).ToList();

            var grouped = combined.GroupBy(rec => rec.Ticker).ToList();

            var groups = new List<RecGroup>();
            var convictions = GetConvictions();
            foreach(var group in grouped)
            {
                if(convictions.ContainsKey(group.Key))
                    groups.Add(new RecGroup(group, convictions[group.Key]));
                else
                    groups.Add(new RecGroup(group, Conviction.Neutral));
            }

            var pies = groups.GroupBy(g => g.PieName).ToList();
            var pieList = new List<Pie>();

            foreach(var pie in pies)
            {
                pieList.Add(new Pie(pie));
            }

            var totalPoints = pieList.Sum(p => p.Score);

            foreach(var pie in pieList.OrderBy(p => p.Score).Reverse())
            {
                var percent = pie.Score / totalPoints;
                var value = percent.ToString("P1").PadLeft(6);
                Console.WriteLine($"{value} : {pie.Name}");
                pie.DisplayBreakDown();
            }
        }

        private string GetLocalPath(string fileName)
        {
            var dir = Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(dir, @$"..\..\..\..\Data\{fileName}"));
        }

        private List<MotleyFoolRec> GetRecs(string path, float? points)
        {
            var bestBuys = new List<MotleyFoolRec>();
            using (var file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        bestBuys.Add(new MotleyFoolRec(line, points));
                }
            }
            return bestBuys;
        }

        private Dictionary<string,Conviction> GetConvictions()
        {
            var convictions = new Dictionary<string, Conviction>();
            using (var file = new StreamReader(GetLocalPath("Conviction.csv")))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (Enum.TryParse(values[1], out Conviction myConviction))
                        convictions.Add(values[0], myConviction);
                }
            }
            return convictions;
        }
    }
}
