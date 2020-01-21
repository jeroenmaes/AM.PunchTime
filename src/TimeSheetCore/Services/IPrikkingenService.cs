using System;
using TimeSheetCore.Model;

namespace TimeSheetCore.Services
{
    public interface IPrikkingenService
    {
        DayOverview GetDayOverview(int employeeId, DateTime workday);
        UserOverview GetUserOverview(int employeeId, DateTime workday);
    }
}
