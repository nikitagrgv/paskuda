using UnityEngine;

namespace Components
{
    public class GameConstants : MonoBehaviour
    {
        public GeneralCharacterController player;

        public GeneralCharacterController npcPrefab;
        public int npcSpawnCount = 100;
        public float npcSpawnRadius = 50;

        public float gravityMultiplier = 1.2f;
    }
}