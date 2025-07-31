using UnityEngine;
using UnityEngine.InputSystem;

namespace Components
{
    public class GameController : MonoBehaviour
    {
        public GameObject npcPrefab;
        public int npcSpawnCount = 100;
        public float npcSpawnRadius = 50;

        public float gravityMultiplier = 1.5f;

        private void Start()
        {
            Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            for (int i = 0; i < npcSpawnCount; ++i)
            {
                int teamTypeNum = i % 3;
                Teams.TeamType team;
                switch (teamTypeNum)
                {
                    case 0: team = Teams.TeamType.Red; break;
                    case 1: team = Teams.TeamType.Green; break;
                    default: team = Teams.TeamType.Blue; break;
                }

                SpawnNpc(team);
            }
        }

        private void SpawnNpc(Teams.TeamType team)
        {
            Vector2 pos2 = Random.insideUnitCircle * npcSpawnRadius;
            Vector3 pos3 = new Vector3(pos2.x, 1.5f, pos2.y);
            GameObject obj = Instantiate(npcPrefab, pos3, Quaternion.identity);
            RelationshipsActor relationships = obj.GetComponent<RelationshipsActor>();
            if (relationships)
            {
                relationships.Team = team;
            }
        }

        private void Update()
        {
            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                if (Mathf.Approximately(Time.timeScale, 1f))
                {
                    Time.timeScale = 0.1f;
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }
        }
    }
}