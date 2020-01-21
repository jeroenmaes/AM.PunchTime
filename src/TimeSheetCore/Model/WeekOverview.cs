using System;
using System.Collections.Generic;
using TimeSheetCore.Helpers;

namespace TimeSheetCore.Model
{
    public class WeekOverview
    {
        public UserOverview UserOverview { get; set; }
        public List<DayOverview> DayOverviewCollection { get;  set; }

        public TimeSpan Total { get; internal set; }

        public TimeSpan TotalRemaining { get; internal set; }

        public DateTime? PredictedEndOfWeek { get; internal set; }

        public string TotalString { get;  set; }
        public string TotalRemainingString { get; set; }
        public int WeekNumber { get; internal set; }

        public bool WeekCompleted { get; internal set; }

        public WeekOverview()
        {
            Error = string.Empty;
            DayOverviewCollection = new List<DayOverview>();
        }

        public void CalculateTotal()
        {

            Total = DayOverviewCollection.Sum(x => x.Total);
            TotalRemaining = TimeSpan.FromHours(40) - Total;
            GetWeekNumber();

            TotalString = "Total Hours for Week " + WeekNumber + ": " + (int)Total.TotalHours + " TotalMinutes: " + Total.Minutes;
            TotalRemainingString = $"Total Hours remaining for Week {WeekNumber}: {(int)TotalRemaining.TotalHours} TotalMinutes: {TotalRemaining.Minutes}";

            if (TotalRemaining <= TimeSpan.FromSeconds(0))
                WeekCompleted = true;
        }

        private void GetWeekNumber()
        {
            WeekNumber = -1;

            foreach (var DayOverview in DayOverviewCollection)
            {
                try
                {
                    WeekNumber = DayOverview.Prikkingen[0].Datum.GetIso8601WeekOfYear();
                    break;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public void CalculateEnd()
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            {
                var now = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss"));
                PredictedEndOfWeek = now.Add(TotalRemaining);
            }
            else
            {
                PredictedEndOfWeek = null;
            }
        }

        public string Error { get; set; }
    }
}
