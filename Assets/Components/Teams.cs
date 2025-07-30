using UnityEngine;

namespace Components
{
    public static class Teams
    {
        public enum TeamType
        {
            None,
            Green,
            Red,
        }

        public static Color ToColor(TeamType type)
        {
            switch (type)
            {
                case TeamType.Green: return Color.green;
                case TeamType.Red: return Color.red;
                default: return Color.gray2;
            }
        }
    }
}