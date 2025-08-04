using UnityEngine;

namespace Code.Components
{
    public static class MathUtils 
    {
        public const float MinPitch = -87f;
        public const float MaxPitch = 87f;

        public static float ToValidYaw(float angle)
        {
            angle %= 360f;
            if (angle < 0)
            {
                angle += 360f;
            }

            return angle;
        }

        public static float ToValidPitch(float angle)
        {
            return Mathf.Clamp(angle, MinPitch, MaxPitch);
        }
    }
}