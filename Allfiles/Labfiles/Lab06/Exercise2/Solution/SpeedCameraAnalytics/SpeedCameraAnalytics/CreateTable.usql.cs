using System;

namespace SpeedCameraAnalytics
{
    public class UDFs
    {
        public static DateTime ConvertStringToDate(string date)
        {
            // The input string is in the form "dd/mm/yyyy hh24:mi"
            char[] delimiters = { ' ', '/', ':' };
            string[] dateBits = date.Split(delimiters);
            int day = Convert.ToInt32(dateBits[0]);
            int month = Convert.ToInt32(dateBits[1]);
            int year = Convert.ToInt32(dateBits[2]);
            int hour = Convert.ToInt32(dateBits[3]);
            int minute = Convert.ToInt32(dateBits[4]);

            DateTime dt = new DateTime(year, month, day, hour, minute, 0);
            return dt;            
        }
    }
}
