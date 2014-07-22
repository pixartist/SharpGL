using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL
{
    public static class Mathf
    {
		private static Random rnd = new Random();
        public const float PI = (float)Math.PI;
        public const float PI2 = PI * 2;
		public static float Rnd
		{
			get
			{
				return (float)rnd.NextDouble();
			}
		}
		public static float Random(float factor)
		{
			return Rnd * factor;
		}
		public static float Random(float from, float to)
		{
			return (Rnd - from) * (to - from);
		}
        public static float LerpAngle(float start, float end, float amount)
        {
            return start + amount * WrapMP(end - start);
        }
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
