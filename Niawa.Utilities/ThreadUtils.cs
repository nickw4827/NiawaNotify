using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.Utilities
{
    class ThreadUtils2
    {
        ///// <summary>
        ///// Calculates the thread health by subtracting the thread health datetime from the current system datetime.
        ///// Returns a thread health value.
        ///// </summary>
        ///// <param name="threadHealthDate"></param>
        ///// <returns></returns>
        //public static double CalculateThreadHealth(DateTime threadHealthDate)
        //{
        //    double threadHealth = 0;
        //    if (threadHealthDate > DateTime.MinValue)
        //    {
        //        TimeSpan timespan = (DateTime.Now - threadHealthDate);
        //        threadHealth = timespan.TotalSeconds + 0.01; //adding 0.01 so good thread health isn't confused with 0 (0 means invalid date)
        //    }

        //    return threadHealth;

        //}

        ///// <summary>
        ///// Calculates the thread health by subtracting the thread health datetime from the current system datetime.  
        ///// Returns in a key value pair that includes the supplied description and the thread health value.
        ///// </summary>
        ///// <param name="description"></param>
        ///// <param name="threadHealthDate"></param>
        ///// <returns></returns>
        //public static KeyValuePair<string, double> CalculateThreadHealth(string description, DateTime threadHealthDate)
        //{
        //    return new KeyValuePair<string, double>(description, CalculateThreadHealth(threadHealthDate));
        //}

        ///// <summary>
        ///// Calculate sthe thread health by subtracting the thread health datetime from the current system datetime.
        ///// Return in a keyvaluepair that includes the supplied description, the thread health value, and a boolean value indicating if the upper limit has been breached.
        ///// </summary>
        ///// <param name="description"></param>
        ///// <param name="upperLimit"></param>
        ///// <param name="threadHealthDate"></param>
        ///// <returns></returns>
        //public static KeyValuePair<string, KeyValuePair<double, bool>> CalculateThreadHealth(string description, double upperLimit, DateTime threadHealthDate)
        //{
        //    double health = CalculateThreadHealth(threadHealthDate);
        //    if (health - upperLimit > 0)
        //    {
        //        //upper limit has been breached
        //        //health failed
        //        return new KeyValuePair<string, KeyValuePair<double, bool>>(description, new KeyValuePair<double, bool>(health, true));

        //    }
        //    else
        //    {
        //        //upper limit has not been breached
        //        //health passed
        //        return new KeyValuePair<string, KeyValuePair<double, bool>>(description, new KeyValuePair<double, bool>(health, false));

        //    }

        //}

    }
}
