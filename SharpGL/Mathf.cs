using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL
{
    /// <summary>
    /// Floating point math helper
    /// </summary>
    public static class Mathf
    {
        private static Random rnd = new Random();
        /// <summary>
        /// Constant pi
        /// </summary>
        public const float PI = (float)Math.PI;
        /// <summary>
        /// Constant pi*2
        /// </summary>
        public const float PI2 = PI * 2;
        /// <summary>
        /// Returns a random floating point between 0 and 1
        /// </summary>
        public static float Rnd
        {
            get
            {
                return (float)rnd.NextDouble();
            }
        }
        /// <summary>
        /// Returns a random floating point number between 0 and factor
        /// </summary>
        /// <param name="factor">The upper limit of the result</param>
        /// <returns>A random floating point number between 0 and factor</returns>
        public static float Random(float factor)
        {
            return Rnd * factor;
        }
        /// <summary>
        /// Generates a random floating point number
        /// </summary>
        /// <param name="from">The minimum value of the result</param>
        /// <param name="to">The maximum value of the result</param>
        /// <returns>A random number between from and to</returns>
        public static float Random(float from, float to)
        {
            return (Rnd - from) * (to - from);
        }
        /// <summary>
        /// Interpolates an angle in radians
        /// </summary>
        /// <param name="start">The angle in radians from which to start</param>
        /// <param name="end">The angle in radians for which to go</param>
        /// <param name="amount">The amount of interpolation between start and end</param>
        /// <returns></returns>
        public static float LerpAngle(float start, float end, float amount)
        {
            return start + amount * WrapMP(end - start);
        }
        /// <summary>
        /// Calculates the angular difference in radians between two values in the range 0 to 2pi
        /// </summary>
        /// <param name="a">Value a in radians</param>
        /// <param name="b">Value b in radians</param>
        /// <returns>The anglular difference from a to b in radians between 0 and 2pi</returns>
        public static float AngleDif(float a, float b)
        {
            return Math.Abs(WrapMP(b - a));
        }
        public static float WrapMP(float angle)
        {
            while (angle < -PI)
                angle += PI2;
            while (angle > PI)
                angle -= PI2;
            return angle;
        }
        public static float Cos(float angle)
        {
            return (float)Math.Cos(angle);
        }
        public static float Sin(float angle)
        {
            return (float)Math.Sin(angle);
        }
        public static float Tan(float angle)
        {
            return (float)Math.Tan(angle);
        }
        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }
        public static float Deg2Rad(float angle)
        {
            return (angle * PI) / 180.0f;
        }
        public static float Rad2Deg(float rad)
        {
            return (rad / PI) * 180.0f;
        }
        public static float Sqrt(float rad)
        {
            return (float)Math.Sqrt(rad);
        }
    }
}
