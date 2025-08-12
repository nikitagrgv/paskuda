using System.Collections.Generic;
using UnityEngine;

namespace Code
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

        public static Vector3 WithSpread(this Vector3 direction, float spread)
        {
            if (direction == Vector3.zero) return Vector3.zero;

            Vector3 fwd = direction.normalized;

            Vector3 perp = Vector3.Cross(fwd, Vector3.forward);
            if (perp.sqrMagnitude < 0.0001f)
            {
                perp = Vector3.Cross(fwd, Vector3.up);
            }

            perp.Normalize();

            Vector3 perp2 = Vector3.Cross(fwd, perp).normalized;

            Vector2 spreadVec = Random.insideUnitCircle * spread;

            float angle = spreadVec.magnitude;
            if (angle == 0f) return direction;

            Vector3 axis = (spreadVec.x * perp + spreadVec.y * perp2).normalized;

            Quaternion spreadQuat = Quaternion.AngleAxis(angle, axis);
            return spreadQuat * direction;
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

        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = new TValue();
            dictionary.Add(key, value);
            return value;
        }
    }
}