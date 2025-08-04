using UnityEngine;

namespace Code.Components
{
    public class GameConstants : MonoBehaviour
    {
        public GeneralCharacterController player;

        public GeneralCharacterController npcPrefab;
        public int npcSpawnCount = 500;
        public float gameFieldRadius = 200;

        public float gravityMultiplier = 1.2f;
    }
}