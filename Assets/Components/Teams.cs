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
            Blue,
        }

        public static Color ToColor(TeamType type)
        {
            switch (type)
            {
                case TeamType.Green: return Color.green;
                case TeamType.Red: return Color.red;
                case TeamType.Blue: return Color.blue;
                default: return Color.gray2;
            }
        }
    }
}