using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using System.Collections.ObjectModel;

namespace Components
{
    public class GameController : MonoBehaviour
    {
        public GameObject player;

        public GameObject npcPrefab;
        public int npcSpawnCount = 100;
        public float npcSpawnRadius = 50;

        public float gravityMultiplier = 1.5f;

        public bool IsSpawnFinished { get; private set; }

        public event Action<Teams.TeamType, int> AliveCountChanged;

        public struct TeamInfo
        {
            public Teams.TeamType Team;
            public int Alive;
        }

        public ReadOnlyCollection<TeamInfo> AllTeams => _teams.AsReadOnly();

        private readonly List<TeamInfo> _teams = new();

        private void Start()
        {
            Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            RegisterPlayer();

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

            IsSpawnFinished = true;
        }

        private void RegisterPlayer()
        {
            RelationshipsActor playerRelationships = player.GetComponent<RelationshipsActor>();
            if (!playerRelationships) return;

            RegisterCharacter(playerRelationships);
        }

        private void SpawnNpc(Teams.TeamType team)
        {
            Vector2 pos2 = Random.insideUnitCircle * npcSpawnRadius;
            Vector3 pos3 = new(pos2.x, 1.5f, pos2.y);
            GameObject obj = Instantiate(npcPrefab, pos3, Quaternion.identity);

            RelationshipsActor relationships = obj.GetComponent<RelationshipsActor>();
            if (!relationships) return;

            relationships.Team = team;
            RegisterCharacter(relationships);
        }

        private void RegisterCharacter(RelationshipsActor actor)
        {
            AddTeamAlive(actor.Team, 1);
            actor.Died += () => OnCharacterDied(actor);
        }

        private void OnCharacterDied(RelationshipsActor actor)
        {
            AddTeamAlive(actor.Team, -1);
        }

        private void AddTeamAlive(Teams.TeamType team, int delta)
        {
            int index = _teams.FindIndex(ts => ts.Team == team);
            if (index < 0)
            {
                _teams.Add(new TeamInfo { Team = team, Alive = 0 });
                index = _teams.Count - 1;
            }

            TeamInfo teamInfo = _teams[index];
            teamInfo.Alive += delta;
            _teams[index] = teamInfo;

            NotifyAliveCountChanged(team, delta);
        }

        private void NotifyAliveCountChanged(Teams.TeamType team, int score)
        {
            AliveCountChanged?.Invoke(team, score);
        }
    }
}