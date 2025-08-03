using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using System.Collections.ObjectModel;
using UnityEngine.Assertions;

namespace Components
{
    public class GameController : MonoBehaviour
    {
        public SpectatorCamera spectatorCamera;

        public bool IsSpawnFinished { get; private set; }

        public event Action<Teams.TeamType, int> AliveCountChanged;

        public struct TeamInfo
        {
            public Teams.TeamType Team;
            public int Alive;
        }

        public ReadOnlyCollection<TeamInfo> AllTeams => _teams.AsReadOnly();

        private readonly List<TeamInfo> _teams = new();

        private GameConstants _consts;
        private PlayerInput _input;

        private const string SpectatorMapName = "Spectator";
        private const string PlayerMapName = "Player";

        private void Start()
        {
            _consts = GetComponent<GameConstants>();
            Assert.IsNotNull(_consts);

            _input = GetComponent<PlayerInput>();
            Assert.IsNotNull(_input);
            _input.actions.FindActionMap(SpectatorMapName).Disable();
            _input.actions.FindActionMap(PlayerMapName).Enable();
            _input.SwitchCurrentActionMap(PlayerMapName);

            Physics.gravity = Vector3.down * 9.81f * _consts.gravityMultiplier;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            RegisterPlayer();

            int count = _consts.npcSpawnCount;
            for (int i = 0; i < count; ++i)
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
            RegisterCharacter(_consts.player);

            Health health = _consts.player.GetComponent<Health>();
            health.Died += OnPlayerDied;

            RelationshipsActor playerRelationships = _consts.player.GetComponent<RelationshipsActor>();
            if (playerRelationships)
            {
                RegisterRelationshipsActor(playerRelationships);
            }
        }

        private void OnPlayerDied()
        {
            Time.timeScale = 1f;

            Camera playerCamera = _consts.player.GetComponentInChildren<Camera>();
            playerCamera.enabled = false;

            spectatorCamera.gameObject.SetActive(true);
            spectatorCamera.transform.position = _consts.player.transform.position;
            spectatorCamera.transform.rotation = _consts.player.transform.rotation;
            Vector3 euler = _consts.player.transform.eulerAngles;
            spectatorCamera.LookPitch = euler.x;
            spectatorCamera.LookYaw = euler.y;

            _input.actions.FindActionMap(SpectatorMapName).Enable();
            _input.actions.FindActionMap(PlayerMapName).Disable();
            _input.SwitchCurrentActionMap(SpectatorMapName);
        }

        private void SpawnNpc(Teams.TeamType team)
        {
            Vector2 pos2 = Random.insideUnitCircle * _consts.gameFieldRadius;
            Vector3 pos3 = new(pos2.x, 1.5f, pos2.y);
            GeneralCharacterController npc = Instantiate(_consts.npcPrefab, pos3, Quaternion.identity);

            RegisterCharacter(npc);

            RelationshipsActor relationships = npc.GetComponent<RelationshipsActor>();
            if (relationships)
            {
                relationships.Team = team;
                RegisterRelationshipsActor(relationships);
            }
        }

        private void RegisterCharacter(GeneralCharacterController character)
        {
            character.gameConstants = _consts;
        }

        private void RegisterRelationshipsActor(RelationshipsActor actor)
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