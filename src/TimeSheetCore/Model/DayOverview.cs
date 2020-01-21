using System;
using System.Collections.Generic;

namespace TimeSheetCore.Model
{
    public class DayOverview
    {
        public List<Prikking> Prikkingen { get; internal set; }
        public TimeSpan Total { get; internal set; }
        public TimeSpan LunchTime { get; internal set; }
        public bool LunchBreak { get; set; }
        public bool Absence { get; set; }
        public DateTime Date { get; set; }
        public bool Completed { get; set; }
        public DateTime? PredictedEndOfDay { get; internal set; }
        public TimeSpan? Overtime { get; internal set; }

        public void CalculateTotal()
        {
            if (Absence)
            {
                Total = TimeSpan.FromHours(8);
            }

            if (Prikkingen.Count == 1)
            {
                Prikkingen.Add(new Prikking { Datum = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss")), Zone = "DUMMY" });
            }

            if (Prikkingen.Count == 3)
            {
                Prikkingen.Add(new Prikking { Datum = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss")), Zone = "DUMMY" });
                LunchBreak = true;
            }
            
            if (Prikkingen.Count == 2 || Prikkingen.Count >= 4)
            {
                DateTime dtFrom1 = Prikkingen[0].Datum;
                DateTime dtTo1 = Prikkingen[1].Datum;

                TimeSpan ts1 = dtTo1 - dtFrom1;
                Total = ts1;
            }


            if (Prikkingen.Count >= 4)
            {
                DateTime dtFrom2 = Prikkingen[2].Datum;
                DateTime dtTo2 = Prikkingen[3].Datum;

                TimeSpan ts2 = dtTo2 - dtFrom2;
                Total += ts2;
                LunchBreak = true;
                LunchTime = Prikkingen[2].Datum - Prikkingen[1].Datum;
                
                PredictedEndOfDay = Prikkingen[0].Datum + TimeSpan.FromHours(8) + LunchTime;

                Overtime = Total - TimeSpan.FromHours(8);
                if (Overtime < TimeSpan.FromMinutes(0))
                    Overtime = TimeSpan.FromMinutes(0);
            }
        }
    }
}
