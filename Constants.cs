using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clickerheroes.autoplayer
{
    /// <summary>
    /// Contains constants used by the program
    /// </summary>
    class Constants
    {
        private static double[] OnePointSevenToPowerArray;
        private static double[] SumZeroToNOnePointSevenToPowerArray;

        /// <summary>
        /// Calculates 1.07^n
        /// </summary>
        /// <param name="power">The power to raise to</param>
        /// <returns>The result</returns>
        public static double OnePointSevenToPower(int power)
        {
            // Special case
            if (power == -1)
            {
                return 0;
            }

            if (OnePointSevenToPowerArray == null)
            {
                //Size 4100 array will have problems with leveling a hero up to level 4100
                OnePointSevenToPowerArray = new double[4201];
                double opstop = 1E0;
                for (int i = 0; i < OnePointSevenToPowerArray.Count(); i++)
                {
                    OnePointSevenToPowerArray[i] = opstop;
                    opstop *= 1.07;
                }
            }

            return OnePointSevenToPowerArray[power];
        }

        /// <summary>
        /// Calculates the sum (1.07)^0 + (1.07)^2 + ... + (1.07)^n
        /// </summary>
        /// <param name="power">The power of the largest summand</param>
        /// <returns>The result</returns>
        public static double SumZeroToNOnePointSevenToPower(int power)
        {
            // Special case to make certain calculations easier
            if (power == -1)
            {
                return 0;
            }

            if (OnePointSevenToPowerArray == null)
            {
                OnePointSevenToPower(0);
            }

            if (SumZeroToNOnePointSevenToPowerArray == null)
            {
                SumZeroToNOnePointSevenToPowerArray = new double[OnePointSevenToPowerArray.Count()];
                double cursum = 0;

                for (int i = 0; i < SumZeroToNOnePointSevenToPowerArray.Count(); i++)
                {
                    cursum += OnePointSevenToPowerArray[i];
                    SumZeroToNOnePointSevenToPowerArray[i] = cursum;
                }
            }

            return SumZeroToNOnePointSevenToPowerArray[power];
        }
    }
}
