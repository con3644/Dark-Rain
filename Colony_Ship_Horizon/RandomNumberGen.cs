using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colony_Ship_Horizon
{
    class RandomNumberGen
    {
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        /// <summary>
        /// Returns a random number between the min and max values given. Next will change the internal state
        /// of the Random instance, effectively making it more random. It is syncronized to prevent
        /// access to it at the same time from different threads, since Random does not guarantee thread safety.
        /// </summary>
        /// <param name="minimum value">The smallest possible number</param>
        /// <param name="maximum value">One greater than the largest possible number</param>
        /// <returns></returns>
        public int NextNumber(int min, int max)
        {
            return GetNumber(min, max);
        }

        /// <summary>
        /// Called in next number
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
    }
}
