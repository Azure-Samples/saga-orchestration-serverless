using System;

namespace Saga.Common.Utils
{
    public class SystemTime
    {
        private static DateTime date;

        static SystemTime()
        {
            date = DateTime.MinValue;
        }

        public static DateTime Now
        {
            get
            {
                if (date != DateTime.MinValue)
                {
                    return date;
                }
                return DateTime.Now;
            }
        }

        public static void SetCustomDate(DateTime custom)
        {
            date = custom;
        }
    }
}
