using System;
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


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, gameFieldRadius);
        }
    }
}