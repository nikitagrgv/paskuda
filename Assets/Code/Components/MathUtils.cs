using UnityEngine;

namespace Code.Components
{
    public static class MathUtils 
    {
        public static float ToValidYaw(float angle)
        {
            angle %= 360f;
            if (angle < 0)
            {
                angle += 360f;
            }

            return angle;
        }
    }
}