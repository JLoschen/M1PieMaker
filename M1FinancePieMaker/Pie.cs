using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static M1FinancePieMaker.PieCreator;

namespace M1FinancePieMaker
{
    public class Pie
    {
        public float Score { get; set; }
        public string Name { get; set; }
        public List<RecGroup> Stocks { get; set; }

        public Pie(IGrouping<string, RecGroup> pie)
        {
            Name = pie.Key;
            Stocks = pie.ToList();
            Score = Stocks.Sum(s => s.TotalScore);
        }

        internal void DisplayBreakDown()
        {
            foreach (var stock in Stocks)
            {
                var percent = stock.TotalScore / Score;
                Console.WriteLine($"--------{percent:P1} {stock.Name}");
            }
        }
    }
}
