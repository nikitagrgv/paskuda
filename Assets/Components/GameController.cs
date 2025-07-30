using UnityEngine;

namespace Components
{
    public class GameController : MonoBehaviour
    {
        public GameObject enemy;
        public int enemySpawnCount = 100;
        public float enemySpawnRadius = 50;

        public float gravityMultiplier = 1.5f;

        private void Start()
        {
            Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            for (int i = 0; i < enemySpawnCount; ++i)
            {
                Vector2 pos2 = Random.insideUnitCircle * enemySpawnRadius;
                Vector3 pos3 = new Vector3(pos2.x, 1.5f, pos2.y);
                Instantiate(enemy, pos3, Quaternion.identity);
            }
        }
    }
}