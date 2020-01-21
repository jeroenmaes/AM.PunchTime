using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TimeSheetApi.Controllers;
using TimeSheetCore;
using TimeSheetCore.Helpers;
using TimeSheetCore.Model;
using TimeSheetCore.Services;
using Xunit;

namespace TimeSheetApiTest
{
    public class WorklogControllerTest
    {
        private readonly IPrikkingenService _service;

        public WorklogControllerTest()
        {
            var services = new ServiceCollection();

            var appConfig = new AppConfig { ClientBaseUrl = ""};

            services.AddTransient<IPrikkingenService, PrikkingenService>();
            
            services.AddHttpClient<IPrikkingenService, PrikkingenService>(c =>
                    c.BaseAddress = new Uri(appConfig.ClientBaseUrl))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                });

            var provider = services.AddLogging().BuildServiceProvider();

            _service = (IPrikkingenService)provider.GetService(typeof(IPrikkingenService));
        }

        [Fact]
        public void Get_CurrentWorkweek_Ok()
        {
            var employeeId = 951629;

            var logger = Mock.Of<ILogger<WorklogController>>();
            var controller = new WorklogController(logger, _service);
            var result = controller.Get(employeeId, -1,  -1);
                        
            Assert.IsType<WeekOverview>(result);

            Assert.Equal(employeeId, result.UserOverview.Id);
            Assert.Equal(string.Empty, result.Error);
        }

        [Fact]
        public void Get_CurrentWorkweek_UserNotFound_Error()
        {
            var employeeId = -1;

            var logger = Mock.Of<ILogger<WorklogController>>();
            var controller = new WorklogController(logger, _service);
            var result = controller.Get(employeeId, -1, -1);

            Assert.IsType<WeekOverview>(result);
            Assert.Null(result.UserOverview);
        }

        [Fact]
        public void Get_PreviousWorkweek_Ok()
        {
            var employeeId = 951629;

            var currentWeek = DateTime.Now.GetIso8601WeekOfYear();
            var previousWeek = currentWeek - 1;

            var logger = Mock.Of<ILogger<WorklogController>>();
            var controller = new WorklogController(logger, _service);
            var result = controller.Get(employeeId, previousWeek, -1);

            Assert.IsType<WeekOverview>(result);

            Assert.Equal(employeeId, result.UserOverview.Id);
            Assert.Equal(previousWeek, result.WeekNumber);

            Assert.Equal(string.Empty, result.Error);
            Assert.True(result.WeekCompleted);
        }

        [Fact]
        public void Get_SpecificWorkweek_Ok()
        {
            var employeeId = 951629;

            var week = 2;
            var year = 2020;

            var logger = Mock.Of<ILogger<WorklogController>>();
            var controller = new WorklogController(logger, _service);
            var result = controller.Get(employeeId, week, year);

            Assert.IsType<WeekOverview>(result);

            Assert.Equal(employeeId, result.UserOverview.Id);
            Assert.Equal(week, result.WeekNumber);

            Assert.Equal(string.Empty, result.Error);
            Assert.True(result.WeekCompleted);
        }
    }
}
