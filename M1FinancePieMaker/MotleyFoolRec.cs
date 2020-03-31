using System;

namespace M1FinancePieMaker
{
    public class MotleyFoolRec
    {
        public string Ticker { get; set; }
        public string Name { get; set; }
        public string PieName { get; set; }
        public float Score { get; set; }
        public DateTime RecDate { get; set; }
        public float Conviction { get; set; }

        public MotleyFoolRec(string line, float? points)
        {
            var props = line.Split(",");
            Name = props[0];
            Ticker = props[1];
            RecDate = DateTime.Parse(props[2]);
            PieName = props[3];
            Score = points ?? float.Parse(props[4]); //if no points provide then assume points provided by .csv
        }
    }
}
