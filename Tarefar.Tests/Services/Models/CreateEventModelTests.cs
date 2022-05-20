using FluentAssertions;
using System;
using Tarefar.DB.Models;
using Tarefar.Services.Models.Events;
using Xunit;

namespace Tarefar.Tests.Services.Models
{
    public class CreateEventModelTests
    {
        #region CreateEventModelToEvent
        [Theory, MemberData(nameof(CreateEventModelToEventTests))]
        public void CreateEventModelToEventTest(CreateEventModelToEventTestInput test)
        {
            var response = (Event)test.CreateEventModel;

            response.Should().BeEquivalentTo(test.ExpectedRespose);
        }

        public static readonly TheoryData<CreateEventModelToEventTestInput> CreateEventModelToEventTests = new()
        {
            new CreateEventModelToEventTestInput()
            {
                TestName = "Basic",
                CreateEventModel = new CreateEventModel()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20,0,21,0),
                    AllDay = false,
                    ReocurrecyType = ReocurrecyType.Day,
                    ReocurrencyFrequency = 3
                },
                ExpectedRespose = new Event()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20,0,21,0),
                    AllDay = false,
                    ReocurrecyType = ReocurrecyType.Day,
                    ReocurrencyFrequency = 3
                }
            },
            new CreateEventModelToEventTestInput()
            {
                TestName = "All day should ignore time",
                CreateEventModel = new CreateEventModel()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20,0,21,0),
                    EndDate = new DateTime(2022,5,21,13,57,0),
                    AllDay = true
                },
                ExpectedRespose = new Event()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20),
                    EndDate = new DateTime(2022,5,21),
                    AllDay = true
                }
            },
            new CreateEventModelToEventTestInput()
            {
                TestName = "Reocurrence Frequency should not be 0 if the event reocur",
                CreateEventModel = new CreateEventModel()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20,0,21,0),
                    AllDay = false,
                    ReocurrecyType = ReocurrecyType.Day
                },
                ExpectedRespose = new Event()
                {
                    Name = "New event",
                    CreatedAt = DateTime.Now.Date,
                    StartDate = new DateTime(2022,5,20,0,21,0),
                    AllDay = false,
                    ReocurrecyType = ReocurrecyType.Day,
                    ReocurrencyFrequency = 1
                }
            }
        };
        public class CreateEventModelToEventTestInput
        {
            public string TestName { get; set; }
            public CreateEventModel CreateEventModel { get; set; }
            public Event ExpectedRespose { get; set; }
        }
        #endregion CreateEventModelToEvent
    }
}
