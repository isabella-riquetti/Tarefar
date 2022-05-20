using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using Tarefar.DB;
using Tarefar.DB.Models;
using Tarefar.Infra.UnitOfWork;
using Tarefar.Services;
using Tarefar.Services.Models.Events;
using Tarefar.Services.Services.Implementations;
using Xunit;

namespace Tarefar.Tests.Services.Services
{
    public class EventServiceTests
    {
        #region IsValid
        [Theory, MemberData(nameof(IsValidTests))]
        public void IsValidTest(IsValidTestInput test)
        {
            var uow = Substitute.For<IUnitOfWork>();

            var service = new EventService(uow);
            var response = service.IsValid(test.Event);

            response.Should().BeEquivalentTo(test.ExpectedResponse);
        }

        public static readonly TheoryData<IsValidTestInput> IsValidTests = new()
        {
            new IsValidTestInput()
            {
                TestName = "Fail, empty event",
                Event = null,
                ExpectedResponse = BaseResponse.CreateError("Events cannot be empty")
            },
            new IsValidTestInput()
            {
                TestName = "Fail, End Date greater than start date",
                Event = new Event()
                {
                    StartDate = new DateTime(2022,5,19,23,0,0),
                    AllDay = false
                },
                ExpectedResponse = BaseResponse.CreateError("Events that don't last the entire day must have End Date")
            },
            new IsValidTestInput()
            {
                TestName = "Fail, not all day and without enddate",
                Event = new Event()
                {
                    StartDate = new DateTime(2022,5,19,23,0,0),
                    EndDate = new DateTime(2022,5,19)
                },
                ExpectedResponse = BaseResponse.CreateError("Even End Date must be greater thatn Start Date")
            },
            new IsValidTestInput()
            {
                TestName = "Fail, weekly reocurence",
                Event = new Event()
                {
                    ReocurrecyType = ReocurrecyType.Week
                },
                ExpectedResponse = BaseResponse.CreateError("Events with weekly reocurrence need to have weekdays")
            },
            new IsValidTestInput()
            {
                TestName = "Fail, weekly reocurence",
                Event = new Event()
                {
                    ReocurrecyType = ReocurrecyType.Month
                },
                ExpectedResponse = BaseResponse.CreateError("Events with monthly reocurrence must select the reocurrence type")
            },
            new IsValidTestInput()
            {
                TestName = "Valid",
                Event = new Event()
                {
                    Name = "Code event service",
                    StartDate = new DateTime(2022,5,19,23,0,0),
                    EndDate = new DateTime(2022,5,20,1,0,0)
                },
                ExpectedResponse = BaseResponse.CreateSuccess()
            }
        };
        public class IsValidTestInput
        {
            public string TestName { get; set; }
            public Event Event { get; set; }
            public BaseResponse ExpectedResponse { get; set; }
        }
        #endregion IsValid


        #region Create
        [Theory, MemberData(nameof(CreateTests))]
        public async void CreateTest(CreateTestInput test)
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                   .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                   .Options;

            var apiContext = new ApiContext(options);
            var uow = new UnitOfWork(apiContext);

            var service = new EventService(uow);
            var result = service.Create(test.Event);

            int newEvents = await apiContext.Events.CountAsync();
            Assert.Equal(test.ExpectedNewEvents, newEvents);
        }

        public static readonly TheoryData<CreateTestInput> CreateTests = new()
        {
            new CreateTestInput()
            {
                TestName = "Basic",
                Event = new CreateEventModel()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20),
                    AllDay = true,
                    ReocurrecyType = ReocurrecyType.Never
                },
                ExpectedNewEvents = 1
            },
            new CreateTestInput()
            {
                TestName = "Daily reocurrence",
                Event = new CreateEventModel()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20),
                    AllDay = true,
                    ReocurrecyType = ReocurrecyType.Day
                },
                ExpectedNewEvents = 365
            }
        };
        public class CreateTestInput
        {
            public string TestName { get; set; }
            public Event Event { get; set; }
            public int ExpectedNewEvents { get; set; }
        }
        #endregion Create
    }
}
