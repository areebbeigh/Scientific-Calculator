using System;

namespace Calculator
{
    /// <summary>
    /// Deals with conversion of angles to different units.
    /// </summary>
    class Angles
    {
        public enum units
        {
            RADIANS,  // Default unit
            DEGREES,
            GRADIANS
        }

        public class Converter
        {
            /// <summary>
            /// Converts the given angle to degrees
            /// </summary>
            /// /// <param name="angle">The angle value of type double.</param>
            /// <param name="angleUnit">An object of type AngleConverter.unit that specifies the units of the given angle.</param>
            /// <returns></returns>
            public static double degrees(double angle, units angleUnit)
            {
                if (angleUnit == units.RADIANS)
                    return angle * 180 / Math.PI;
                else if (angleUnit == units.GRADIANS)
                    return angle * 9 / 10;
                else if (angleUnit == units.DEGREES)
                    return angle;
                else
                {
                    Exception error = new Exception("Invalid parameters");
                    throw error;
                }
            }

            /// <summary>
            /// Converts the given angle to radians
            /// </summary>
            /// /// <param name="angle">The angle value of type double.</param>
            /// <param name="angleUnit">An object of type AngleConverter.unit that specifies the units of the given angle.</param>
            /// <returns></returns>
            public static double radians(double angle, units angleUnit)
            {
                // Avoid extra computation
                if (angleUnit == units.RADIANS)
                    return angle;

                return degrees(angle, angleUnit) * Math.PI / 180;
            }

            /// <summary>
            /// Converts the given angle to gradians
            /// </summary>
            /// /// <param name="angle">The angle value of type double.</param>
            /// <param name="angleUnit">An object of type AngleConverter.unit that specifies the units of the given angle.</param>
            /// <returns></returns>
            public static double gradians(double angle, units angleUnit)
            {
                // Avoid extra computation
                if (angleUnit == units.GRADIANS)
                    return angle;

                return degrees(angle, angleUnit) * 10 / 9;
            }
        }
    }
}
