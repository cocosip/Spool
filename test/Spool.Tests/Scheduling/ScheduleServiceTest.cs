using Microsoft.Extensions.Logging;
using Moq;
using Spool.Scheduling;
using System.Threading;
using Xunit;

namespace Spool.Tests.Scheduling
{
    public class ScheduleServiceTest
    {
        private readonly Mock<ILogger<ScheduleService>> _mockLogger;
        public ScheduleServiceTest()
        {
            _mockLogger = new Mock<ILogger<ScheduleService>>();
        }

        [Fact]
        public void Start_Stop_Test()
        {
            int count1 = 0;
            IScheduleService scheduleService = new ScheduleService(_mockLogger.Object);
            scheduleService.StartTask("t1", () => { Interlocked.Increment(ref count1); }, 100, 100);
            Thread.Sleep(200);
            Assert.True(count1 > 0);
            int count2 = count1;
            scheduleService.StopTask("t1");
            Thread.Sleep(200);
            Assert.Equal(count1, count2);
        }

    }
}
