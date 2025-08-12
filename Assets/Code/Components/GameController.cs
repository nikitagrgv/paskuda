using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using System.Collections.ObjectModel;
using UnityEngine.Assertions;
using System.Collections;
using Code.Factories;
using UnityEngine.SceneManagement;
using Zenject;

namespace Code.Components
{
    public class GameController : MonoBehaviour
    {
        public SpectatorCamera spectatorCamera;

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

        private Coroutine _rumbleCoroutine;

        private const string SpectatorMapName = "Spectator";
        private const string PlayerMapName = "Player";

        private ActorFactory _actorFactory;
        private TimeController _timeController;
        private CameraManager _cameraManager;

        [Inject]
        public void Construct(GameConstants consts, [InjectOptional] ActorFactory actorFactory,
            TimeController timeController, CameraManager cameraManager)
        {
            _consts = consts;
            _actorFactory = actorFactory;
            _timeController = timeController;
            _cameraManager = cameraManager;

            if (_actorFactory)
            {
                _actorFactory.Spawned += OnActorSpawned;
            }
        }

        private void Start()
        {
            _input = GetComponent<PlayerInput>();
            Assert.IsNotNull(_input);
            _input.actions.FindActionMap(SpectatorMapName).Disable();
            _input.actions.FindActionMap(PlayerMapName).Enable();
            _input.SwitchCurrentActionMap(PlayerMapName);

            Physics.gravity = Vector3.down * 9.81f * _consts.gravityMultiplier;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            RegisterPlayer();
        }

        private void Update()
        {
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void RegisterPlayer()
        {
            _cameraManager.ActiveCamera = _consts.player.GetComponent<PlayerController>().playerCamera;

            Health health = _consts.player.GetComponent<Health>();
            health.BeforeDied += OnPlayerBeforeDied;
            health.HealthChanged += OnPlayerHealthChanged;

            RelationshipsActor playerRelationships = _consts.player.GetComponent<RelationshipsActor>();
            if (playerRelationships)
            {
                RegisterRelationshipsActor(playerRelationships);
            }
        }

        private void OnPlayerHealthChanged(Health.HealthChangeInfo info)
        {
            if (info.Delta < 0)
            {
                float time = 0.25f;
                float power = Mathf.Clamp01(-info.Delta / 20f);
                RunRumbleGamepad(power, time);
            }
        }

        private void RunRumbleGamepad(float power, float time)
        {
            if (_rumbleCoroutine != null)
            {
                StopCoroutine(_rumbleCoroutine);
            }

            _rumbleCoroutine = StartCoroutine(RumbleGamepad(power, time));
        }


        private IEnumerator RumbleGamepad(float power, float time)
        {
            Gamepad pad = Gamepad.current;
            if (pad == null)
                yield break;

            pad.SetMotorSpeeds(1f * power, 1f * power);
            yield return new WaitForSecondsRealtime(time);
            pad.SetMotorSpeeds(0, 0);
            _rumbleCoroutine = null;
        }

        private void OnPlayerBeforeDied()
        {
            _timeController.ResetTimeScale();

            Camera playerCamera = _consts.player.GetComponentInChildren<Camera>();
            playerCamera.enabled = false;

            _cameraManager.ActiveCamera = spectatorCamera.spectatorCamera;
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

        private void OnActorSpawned(RelationshipsActor actor)
        {
            RegisterRelationshipsActor(actor);
        }

        private void RegisterRelationshipsActor(RelationshipsActor actor)
        {
            AddTeamAlive(actor.Team, 1);
            actor.GetComponent<Health>().BeforeDied += () => OnActorDied(actor);
        }

        private void OnActorDied(RelationshipsActor actor)
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