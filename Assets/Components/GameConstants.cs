using UnityEngine;

namespace Components
{
    public class GameConstants : MonoBehaviour
    {
        public GameObject player;
        public GeneralCharacterController playerController;
        public GeneralCharacterController npcPrefab;
        public int npcSpawnCount = 500;
        public float gameFieldRadius = 200;
        public float gravityMultiplier = 1.2f;
    }
}