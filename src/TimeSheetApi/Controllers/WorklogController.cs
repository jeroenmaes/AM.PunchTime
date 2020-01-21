using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TimeSheetCore.Helpers;
using TimeSheetCore.Model;
using TimeSheetCore.Services;


namespace TimeSheetApi.Controllers
{
    [Route("api/[controller]")]
    public class WorklogController : Controller
    {
        private readonly IPrikkingenService _prikkingenService;
        private readonly ILogger<WorklogController> _logger;

        public WorklogController(ILogger<WorklogController> logger, IPrikkingenService prikkingenService)
        {
            _logger = logger;
            _prikkingenService = prikkingenService;
        }
                
        [HttpGet]
        public WeekOverview Get([FromQuery]int employeeId, [FromQuery]int? workWeek=-1, [FromQuery]int? year = -1)
        {
            var weekoverview = new WeekOverview();

            try
            {
                _logger.LogInformation($"Get WeekOverview for EmployeeId '{employeeId}', Workweek '{workWeek}', Year '{year}'");
                var workweek = DateTimeExtensions.GetDateTimesOfWorkWeek(workWeek.Value, year.Value);


                foreach (var workday in workweek)
                {
                    var wordayOverview = _prikkingenService.GetDayOverview(employeeId, workday);
                    weekoverview.DayOverviewCollection.Add(wordayOverview);

                    try
                    {
                        //try to get userinfo
                        weekoverview.UserOverview = _prikkingenService.GetUserOverview(employeeId, workday);
                    }
                    catch (Exception)
                    {
                        //Ignore UserNotFoundException
                    }
                }

                weekoverview.CalculateTotal();
                weekoverview.CalculateEnd();

            }
            catch (Exception e)
            {
                _logger.LogError("Error on Get WeekOverview", e);
                weekoverview.Error = e.Message;
            }

            return weekoverview;
        }
    }
}
