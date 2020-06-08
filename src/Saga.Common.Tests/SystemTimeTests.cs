using System;
using Saga.Common.Utils;
using Xunit;

namespace Saga.Common.Tests
{
    public class SystemTimeTests
    {
        [Fact]
        public void Current_date_and_time_should_be_valid()
        {
            DateTime currentDateTime = SystemTime.Now;
            Assert.NotEqual(DateTime.MinValue, currentDateTime);
        }

        [Fact]
        public void Invalid_date_and_time_should_be_fixed_by_current_date_and_time()
        {
            SystemTime.SetCustomDate(DateTime.MinValue);
            Assert.NotEqual(DateTime.MinValue, SystemTime.Now);
        }

        [Fact]
        public void Custom_date_and_time_format_should_be_valid()
        {
            DateTime customDateTime = new DateTime(2020, 3, 24);

            SystemTime.SetCustomDate(customDateTime);

            Assert.Equal(SystemTime.Now, customDateTime);
        }
    }
}
