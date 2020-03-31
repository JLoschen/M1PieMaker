using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace M1FinancePieMaker
{
    public class PieCalculator
    {
        //private static string _bestBuysNow = @"C:\Users\Josh\Documents\Investing\BestBuysNow.csv";
        //private static string _buyRecs = @"C:\Users\Josh\Documents\Investing\BuyRecs.csv";
        //private static string _joshPics = @"C:\Users\Josh\Documents\Investing\JoshsPics.csv";
        //private static string _convictionPath = @"C:\Users\Josh\Documents\Investing\Conviction.csv";
        //private static string _bestBuysNow = @"..\BestBuysNow.csv";
        //private static string _buyRecs = @"C:\Users\Josh\Documents\Investing\BuyRecs.csv";
        //private static string _joshPics = @"C:\Users\Josh\Documents\Investing\JoshsPics.csv";
        private static string _convictionPath = @"C:\Users\Josh\Documents\Investing\Conviction.csv";
        private const float _bestBuyPoints = 1;
        private const float _buyRecPoints = 5.3F;

        public void Run()
        {
            //var dir = Directory.GetCurrentDirectory();
            //var buyRecsPath = Path.GetFullPath(Path.Combine(dir, @"..\..\..\BuyRecs.csv"));
            //var bestBuys = GetRecs(_bestBuysNow, _bestBuyPoints);
            //var bestBuys = GetRecs(buyRecsPath, _bestBuyPoints);
            var bestBuys = GetRecs(GetLocalPath("BuyRecs.csv"), _bestBuyPoints);
            //var buyRecs = GetRecs(_buyRecs, _buyRecPoints);
            var buyRecs = GetRecs(GetLocalPath("BestBuysNow.csv"), _buyRecPoints);
            //var joshPics = GetMyPersonalPics(_joshPics);
            var myPics = GetMyPersonalPics(GetLocalPath("MyPics.csv"));

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
                //var value = String.Format("{00:00%}", percent);
                Console.WriteLine($"{value} : {pie.Name}");
                pie.DisplayBreakDown();
            }
        }

        private string GetLocalPath(string fileName)
        {
            var dir = Directory.GetCurrentDirectory();
            //return Path.GetFullPath(Path.Combine(dir, @$"..\..\..\..\{fileName}"));
            return Path.GetFullPath(Path.Combine(dir, @$"..\..\..\..\Data\{fileName}"));
        }

        private List<MotleyFoolRec> GetRecs(string path, float points)
        {
            var bestBuys = new List<MotleyFoolRec>();
            using (var file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        bestBuys.Add(new MotleyFoolRec(line,points));
                }
            }
            return bestBuys;
        }

        private List<MotleyFoolRec> GetMyPersonalPics(string path)
        {
            var bestBuys = new List<MotleyFoolRec>();
            using (var file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        bestBuys.Add(new MotleyFoolRec(line));
                }
            }
            return bestBuys;
        }

        private Dictionary<string,Conviction> GetConvictions()
        {
            var convictions = new Dictionary<string, Conviction>();
            using (var file = new StreamReader(_convictionPath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if(Enum.TryParse(values[1], out Conviction myConviction))
                        convictions.Add(values[0], myConviction);
                }
            }
            return convictions;
        }

        public class MotleyFoolRec
        {
            public string Ticker { get; set; }
            public string Name { get; set; }
            public string PieName { get; set; }
            public float Score { get; set; }
            public DateTime RecDate { get; set; }
            public float Conviction { get; set; }

            //Josh Pic
            public MotleyFoolRec(string line)
            {
                var props = line.Split(",");
                Name = props[0];
                Ticker = props[1];
                RecDate = DateTime.Parse(props[2]);
                PieName = props[3];
                Score = float.Parse(props[4]);
            }

            public MotleyFoolRec(string line, float points)
            {
                var props = line.Split(",");
                Name = props[0];
                Ticker = props[1];
                RecDate = DateTime.Parse(props[2]);
                PieName = props[3];
                Score = points;
            }
        }

        public enum Conviction
        {
            None,
            Negative,
            Neutral,
            Positive,
            High
        }

        public class Pie
        {
            public float Score { get; set; }
            public string Name { get; set; }
            public List<RecGroup> Stocks { get; set; }

            public Pie(IGrouping<string,RecGroup> pie)
            {
                Name = pie.Key;
                Stocks = pie.ToList();
                Score = Stocks.Sum(s => s.TotalScore);
            }

            internal void DisplayBreakDown()
            {
                foreach(var stock in Stocks)
                {
                    var percent = stock.TotalScore / Score;
                    Console.WriteLine($"--------{percent:P1} {stock.Name}");
                }
            }
        }

        public class RecGroup 
        {
            private float _weeklyDecay = 0.008F;
            public string Ticker { get; set; }
            public string Name { get; set; }
            public float TotalScore { get; set; } = 0;
            public string PieName { get; set; }
            public Conviction Conviction { get; set; }

            public RecGroup(IGrouping<string,MotleyFoolRec> group, Conviction conviction)
            {
                var allRecs = group.ToList();

                Conviction = conviction;
                foreach(var rec in allRecs)
                {
                    var weeks = (DateTime.Today - rec.RecDate).Days / 7;
                    var decay = 1 - (weeks * _weeklyDecay);
                    TotalScore += decay * rec.Score;
                }
                
                var recCount = allRecs.Count;
                Ticker = group.Key.PadRight(4);
                PieName = allRecs.First().PieName;
                TotalScore = ModifyForConviction(TotalScore);
                Name = allRecs.First().Name;
                //Console.WriteLine($"{Ticker} Points:{TotalScore.ToString("N4")}: Count:{recCount} Pie:{PieName} Conviction:{Conviction}");
            }

            private float ModifyForConviction(float value)
            {
                switch (Conviction)
                {
                    case Conviction.None:
                        return value * 0.9f;
                    case Conviction.Negative:
                        return value * 0.95f;
                    case Conviction.Neutral:
                        return value * 1.0f;
                    case Conviction.Positive:
                        return value * 1.05f;
                    case Conviction.High:
                        return value * 1.1f;
                    default:
                        return value;
                }
            }
        }
    }
}
