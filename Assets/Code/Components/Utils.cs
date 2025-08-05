using UnityEngine;

namespace Code.Components
{
    public static class Utils
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

        public static bool TryChance(float chance)
        {
            return Random.value < chance;
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Vector3 Moved(this Vector3 vector, float x, float y, float z)
        {
            return new Vector3(x + vector.x, y + vector.y, z + vector.z);
        }

        public static Vector3 WithX(this Vector3 vector, float v)
        {
            return new Vector3(v, vector.y, vector.z);
        }

        public static Vector3 WithY(this Vector3 vector, float v)
        {
            return new Vector3(vector.x, v, vector.z);
        }

        public static Vector3 WithZ(this Vector3 vector, float v)
        {
            return new Vector3(vector.x, vector.y, v);
        }
    }
}