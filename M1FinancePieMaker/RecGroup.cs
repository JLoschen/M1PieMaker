using System;
using System.Linq;

namespace M1FinancePieMaker
{
    public class RecGroup
    {
        private float _weeklyDecay = 0.008F;
        public string Ticker { get; set; }
        public string Name { get; set; }
        public float TotalScore { get; set; } = 0;
        public string PieName { get; set; }
        public Conviction Conviction { get; set; }

        public RecGroup(IGrouping<string, MotleyFoolRec> group, Conviction conviction)
        {
            var allRecs = group.ToList();

            Conviction = conviction;
            foreach (var rec in allRecs)
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
