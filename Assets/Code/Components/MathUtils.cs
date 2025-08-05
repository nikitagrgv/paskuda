using UnityEngine;

namespace Code.Components
{
    public static class MathUtils
    {
        public static float ToAngleFrom0To360(float angle)
        {
            angle %= 360f;
            if (angle < 0)
            {
                angle += 360f;
            }

            return angle;
        }

        public static float ToAngleFromNegative180To180(float angle)
        {
            angle %= 360f;
            switch (angle)
            {
                case < -180f:
                    return angle + 360f;
                case >= 180f:
                    return angle - 360f;
                default:
                    return angle;
            }
        }
    }
}