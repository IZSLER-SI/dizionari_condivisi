using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public static class DateExtensions
    {
        static DateTime epoca = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public static string OnlyDate(this DateTime? dt, string format)
        {
            if (dt == null)
                return "NONDEF";
            
            return new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, dt.Value.Hour, dt.Value.Minute, dt.Value.Second).ToString(format);
        }

        public static DateTime GetDateTimeFromdMy(string date)
        {
            return DateTime.ParseExact(date, "dMy", CultureInfo.InvariantCulture);
        }

        public static DateTime StartOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0);
        }
		
        public static DateTime EndOfDay(this DateTime dt)
        {
            return StartOfDay(dt).AddDays(1).AddMilliseconds(-1);
        }

        public static bool Between(this DateTime dt, DateTime d1, DateTime d2)
        {
            return dt >= d1 && dt < d2;
        }


        /*
         * Esempio sostituzione funzione ToUnixTimeSeconds
         *  var dateTime = new DateTime(2015, 05, 24, 10, 2, 0, DateTimeKind.Local);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalSeconds; 
         */
        public static double ToUnixTimestamp(this DateTime dt)
        {
            DateTime dataDaConvertire = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Month, dt.Second, DateTimeKind.Local);
            double unixTimeStamp = (dataDaConvertire.ToUniversalTime() - epoca).TotalSeconds;   //ToUnixTimeSeconds_Custom(dataDaConvertire, epoca);// ToUnixTimeSeconds();
            if (unixTimeStamp < 0) unixTimeStamp = 0;
            return unixTimeStamp;
        }

        /// <summary>
        /// Converte un timestamp unix UTC in DateTime.
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimestamp(this long unixTimeStamp)
        {
            //DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
            //return dto.DateTime;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(unixTimeStamp);
            return dt;
        }

        public static bool IsNullOrMin(this DateTime? dt)
        {
            return dt == null || ! dt.HasValue || dt.Value.Equals(DateTime.MinValue);
        }

        public static DateTime? ToNullIfMin(this DateTime? dt)
        {
            return dt.IsNullOrMin() ? null : dt;
        }

        public static DateTime? ToDateTime(this string dateTimeStr, string[] dateFmt)
        {
            // example: var dt="2011-03-21 13:26".toDate("yyyy-MM-dd HH:mm");
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            DateTime? result = null;
            DateTime dt;
            if (DateTime.TryParseExact(dateTimeStr, dateFmt,
                CultureInfo.InvariantCulture, style, out dt)) result = dt;
            return result;
        }

        public static DateTime? ToDateTime(this string dateTimeStr, string dateFmt)
        {
            // call overloaded function with string array param     
            return ToDateTime(dateTimeStr, new string[] { dateFmt });
        }

        public static DateTime ToDateTimeOrDefault(this string dateTimeStr, string[] dateFmt, DateTime defDate)
        {
            // example: var dt="2011-03-21 13:26".toDate("yyyy-MM-dd HH:mm");
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            DateTime dt;
            if (DateTime.TryParseExact(dateTimeStr, dateFmt,
                CultureInfo.InvariantCulture, style, out dt)) return dt;
            return defDate;
        }

        public static DateTime ToDateTimeOrDefault(this string dateTimeStr, string dateFmt, DateTime defDate)
        {
            // call overloaded function with string array param     
            return ToDateTimeOrDefault(dateTimeStr, new string[] { dateFmt }, defDate);
        }

        public static DateTime StringToDate( this string onlyDate, string onlyTime = "00:00:00")
        {
            // converte in datetime e concatena : format(dd/mm/yy hh:mm:ss)
            DateTime date;
            if (String.IsNullOrWhiteSpace(onlyDate))
                date = new DateTime(1900, 01, 01);
            else
                date = DateTime.ParseExact(onlyDate, "dd/MM/yy", CultureInfo.InvariantCulture);

            DateTime time;
            if (String.IsNullOrWhiteSpace(onlyTime))
                time = new DateTime(1900, 01, 01, 0, 0, 0);
            else
                time = DateTime.ParseExact(onlyTime, "HH:mm:ss", CultureInfo.CurrentCulture);

            return (date.Date.Add(time.TimeOfDay));
        }
    }
}
