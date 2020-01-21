using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TimeSheetCore.Helpers
{
    public static class DateTimeExtensions
    {
        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime FirstDateOfWeekIso8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
        }
        
        public static List<DateTime> GetDateTimesOfWorkWeek(int weekNumber, int year)
        {
            if (year == -1)
                year = DateTime.Today.Year;

            var startDate = weekNumber == -1 ? DateTime.Today : FirstDateOfWeekIso8601(year, weekNumber);

            startDate = startDate.AddDays(-(((startDate.DayOfWeek - DayOfWeek.Monday) + 7) % 7));

            var endDate = startDate.AddDays(5);

            //the number of days in our range of dates
            var numDays = (int)((endDate - startDate).TotalDays);

            List<DateTime> myDates = Enumerable
                //creates an IEnumerable of ints from 0 to numDays
                .Range(0, numDays)
                //now for each of those numbers (0..numDays), 
                //select startDate plus x number of days
                .Select(x => startDate.AddDays(x))
                //and make a list
                .ToList();
            return myDates;
        }
    }
}
