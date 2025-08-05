using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro.EditorUtilities;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Code.Components
{
    public class BotController : MonoBehaviour
    {
        public VisibilityChecker visibilityChecker;
        public GeneralCharacterController ctrl;
        public RelationshipsActor relationshipsActor;
        public Health health;

        [Header("Movement")]
        public float moveSpeed = 5f;

        public float moveSpeedSprint = 9f;

        public float minPeriodUpdateYaw = 0.1f;
        public float maxPeriodUpdateYaw = 4f;
        public float speedRotateYaw = 200f;

        public float minPitch = -30f;
        public float maxPitch = 30f;

        public float minPeriodUpdatePitch = 0.1f;
        public float maxPeriodUpdatePitch = 4f;
        public float speedRotatePitch = 200f;

        public float minPeriodJump = 0.1f;
        public float maxPeriodJump = 5f;

        public float minPeriodDash = 0.1f;
        public float maxPeriodDash = 7f;

        public float minPeriodChangeWantedPosition = 0.1f;
        public float maxPeriodChangeWantedPosition = 20f;

        public float minPeriodWantFire = 0.1f;
        public float maxPeriodWantFire = 10f;

        public float forgetEnemyTimout = 10f;
        public float changeEnemyTimeoutMin = 1f;
        public float changeEnemyTimeoutMax = 10f;

        public float minWantedRadiusAroundEnemy = 5f;
        public float maxWantedRadiusAroundEnemy = 30f;

        public float minTimeToChangeWantedRadiusAroundEnemy = 1f;
        public float maxTimeToChangeWantedRadiusAroundEnemy = 40f;

        public float minPeriodChangeWantedPositionAroundEnemy = 0.1f;
        public float maxPeriodChangeWantedPositionAroundEnemy = 10f;

        private float _timerUpdateYaw;
        private float _targetYaw;

        private float _timerUpdatePitch;
        private float _targetPitch;

        private float _timerJump;

        private float _timerDash;

        private float _timerChangeWantedPosition;
        private Vector3 _wantedPosition;

        private float _timerWantFire;

        private GameConstants _gameConstants;

        private GeneralCharacterController _targetEnemy;
        private float _timerForgetEnemy = -1;

        private float _timerChangeEnemy = -1;

        private float _wantedRadiusAroundEnemy;
        private float _timerChangeWantedRadiusAroundEnemy;
        private float _timerChangeWantedPositionAroundEnemy;

        [Inject]
        public void Construct(GameConstants gameConstants)
        {
            _gameConstants = gameConstants;
        }

        private void Start()
        {
            RandomizeWantFire();

            visibilityChecker.BecomeVisible += OnBecomeVisible;
            visibilityChecker.BecomeInvisible += OnBecomeInvisible;

            visibilityChecker.IgnoreTeam = relationshipsActor.Team;
            relationshipsActor.TeamChanged += OnTeamChanged;

            health.HealthChanged += OnHealthChanged;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (!_targetEnemy)
            {
                UpdateTargetPosition(dt);
                UpdateRandomRotation(dt);
            }
            else
            {
                RandomizeWantedRadiusAroundEnemy(dt);
                UpdateTargetPositionAroundEnemy(dt);
                UpdateEnemyChange(dt);
                UpdateEnemyVisibility(dt);
                UpdateRotationToEnemy(dt);
            }

            UpdateJump(dt);
            UpdateDash(dt);
            UpdateFire(dt);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (_targetEnemy)
            {
                Gizmos.DrawWireSphere(_targetEnemy.transform.position, 1f);
            }
        }

        private void OnHealthChanged(Health.HealthChangeInfo info)
        {
            if (!info.IsHit)
            {
                return;
            }

            if (!info.Initiator)
            {
                return;
            }

            RelationshipsActor actor = info.Initiator.GetComponent<RelationshipsActor>();
            if (!actor)
            {
                return;
            }

            if (actor.Team == relationshipsActor.Team)
            {
                return;
            }

            float chanceToChange = _targetEnemy ? 0.4f : 0.99f;
            if (!MathUtils.TryChance(chanceToChange))
            {
                return;
            }

            _targetEnemy = actor.GetComponent<GeneralCharacterController>();
        }

        private void OnTeamChanged()
        {
            visibilityChecker.IgnoreTeam = relationshipsActor.Team;
        }

        private void OnBecomeVisible(GameObject go, VisibilityChecker.Info info)
        {
            if (info.RelationshipsActor.Team == relationshipsActor.Team)
            {
                return;
            }

            if (_targetEnemy)
            {
                if (info.Character == _targetEnemy)
                {
                    _timerForgetEnemy = -1f;
                }
            }
            else
            {
                _targetEnemy = info.Character;
                _timerChangeEnemy = Random.Range(changeEnemyTimeoutMin, changeEnemyTimeoutMax);
            }

            info.Health.Died += OnTargetDied;
        }

        private void OnBecomeInvisible(GameObject go, VisibilityChecker.Info info)
        {
            if (info.Character != _targetEnemy)
            {
                return;
            }

            _timerForgetEnemy = forgetEnemyTimout;
        }

        private void OnTargetDied()
        {
            _targetEnemy = null;
            _timerForgetEnemy = -1f;
            _timerChangeEnemy = -1f;
        }

        private void UpdateEnemyChange(float dt)
        {
            if (!_targetEnemy)
            {
                _timerChangeEnemy = -1f;
                return;
            }

            if (_timerChangeEnemy < 0)
            {
                return;
            }

            _timerChangeEnemy -= dt;
            if (_timerChangeEnemy < 0)
            {
                List<KeyValuePair<GameObject, VisibilityChecker.Info>> visible = visibilityChecker.VisibleObjects;
                if (visible.Count != 0)
                {
                    int index = Random.Range(0, visible.Count);
                    _targetEnemy = visible[index].Value.Character;
                }

                _timerChangeEnemy = Random.Range(minPeriodUpdatePitch, maxPeriodUpdatePitch);
            }
        }

        private void UpdateEnemyVisibility(float dt)
        {
            if (_timerForgetEnemy < 0)
            {
                return;
            }

            if (!_targetEnemy)
            {
                return;
            }

            _timerForgetEnemy -= dt;
            if (_timerForgetEnemy < 0)
            {
                _targetEnemy = null;
                _timerForgetEnemy = -1f;
            }
        }

        private void UpdateRotationToEnemy(float dt)
        {
            if (!_targetEnemy)
            {
                return;
            }

            Vector3 targetPos = _targetEnemy.transform.position;
            Vector3 myPos = transform.position;
            Vector3 dir = targetPos - myPos;

            Vector3 eulerAngles = Quaternion.LookRotation(dir).eulerAngles;
            ctrl.LookPitch = MathUtils.ToAngleFromNegative180To180(eulerAngles.x);
            ctrl.LookYaw = eulerAngles.y;
        }

        private void RandomizeWantedRadiusAroundEnemy(float dt)
        {
            _timerChangeWantedRadiusAroundEnemy -= dt;
            if (_timerChangeWantedRadiusAroundEnemy < 0)
            {
                _wantedRadiusAroundEnemy = Random.Range(minWantedRadiusAroundEnemy, maxWantedRadiusAroundEnemy);
                _timerChangeWantedRadiusAroundEnemy = Random.Range(minTimeToChangeWantedRadiusAroundEnemy,
                    maxTimeToChangeWantedRadiusAroundEnemy);
            }
        }

        private void UpdateTargetPositionAroundEnemy(float dt)
        {
            if (!_targetEnemy)
            {
                return;
            }

            _timerChangeWantedPositionAroundEnemy -= dt;
            if (_timerChangeWantedPositionAroundEnemy <= 0)
            {
                _timerChangeWantedPositionAroundEnemy = Random.Range(minPeriodChangeWantedPositionAroundEnemy,
                    maxPeriodChangeWantedPositionAroundEnemy);
                Vector2 rand = Random.insideUnitCircle * _wantedRadiusAroundEnemy;
                _wantedPosition = new Vector3(rand.x, 0, rand.y);
            }

            UpdateTargetVelocityFromWanted();
        }

        private void UpdateTargetVelocityFromWanted()
        {
            Vector3 curPosition = transform.position;
            Vector3 deltaPosition = _wantedPosition - curPosition;
            deltaPosition.y = 0;
            float deltaMagnitude = deltaPosition.magnitude;
            if (deltaMagnitude < 0.5f)
            {
                ctrl.TargetVelocity = Vector3.zero;
            }
            else
            {
                ctrl.TargetVelocity = deltaPosition.normalized * moveSpeed;
            }
        }

        private void UpdateRandomRotation(float dt)
        {
            // Yaw
            _timerUpdateYaw -= dt;
            if (_timerUpdateYaw <= 0)
            {
                _timerUpdateYaw = Random.Range(minPeriodUpdateYaw, maxPeriodUpdateYaw);
                _targetYaw = Random.Range(0, 360);
            }

            float totalDeltaYaw = _targetYaw - ctrl.LookYaw;
            if (totalDeltaYaw > 180) totalDeltaYaw -= 360;
            if (totalDeltaYaw < -180) totalDeltaYaw += 360;

            float deltaYawSign = Mathf.Sign(totalDeltaYaw);
            float deltaYaw = deltaYawSign * speedRotateYaw * dt;
            if (Mathf.Abs(deltaYaw) > Mathf.Abs(totalDeltaYaw))
            {
                ctrl.LookYaw = _targetYaw;
            }
            else
            {
                ctrl.LookYaw += deltaYaw;
            }

            // Pitch
            _timerUpdatePitch -= dt;
            if (_timerUpdatePitch <= 0)
            {
                _timerUpdatePitch = Random.Range(minPeriodUpdatePitch, maxPeriodUpdatePitch);
                _targetPitch = Random.Range(minPitch, maxPitch);
            }

            float totalDeltaPitch = _targetPitch - ctrl.LookPitch;
            float deltaPitchSign = Mathf.Sign(totalDeltaPitch);
            float deltaPitch = deltaPitchSign * speedRotatePitch * dt;
            if (Mathf.Abs(deltaPitch) > Mathf.Abs(totalDeltaPitch))
            {
                ctrl.LookPitch = _targetPitch;
            }
            else
            {
                ctrl.LookPitch += deltaPitch;
            }
        }

        private void UpdateJump(float dt)
        {
            _timerJump -= dt;
            if (_timerJump <= 0)
            {
                _timerJump = Random.Range(minPeriodJump, maxPeriodJump);
                ctrl.JumpRequest = GeneralCharacterController.ActionRequestType.TryNow;
            }
        }

        private void UpdateDash(float dt)
        {
            _timerDash -= dt;
            if (_timerDash <= 0)
            {
                _timerDash = Random.Range(minPeriodDash, maxPeriodDash);
                Vector2 dashDir2d = Random.insideUnitCircle;
                Vector3 dashDir = new(dashDir2d.x, 0, dashDir2d.y);
                ctrl.RequestDash(dashDir, GeneralCharacterController.ActionRequestType.TryNow);
            }
        }

        private void UpdateTargetPosition(float dt)
        {
            _timerChangeWantedPosition -= dt;
            if (_timerChangeWantedPosition <= 0)
            {
                _timerChangeWantedPosition = Random.Range(minPeriodChangeWantedPosition, maxPeriodChangeWantedPosition);
                Vector2 rand = Random.insideUnitCircle * _gameConstants.gameFieldRadius;
                _wantedPosition = new Vector3(rand.x, 0, rand.y);
            }

            UpdateTargetVelocityFromWanted();
        }

        private void UpdateFire(float dt)
        {
            _timerWantFire -= dt;
            if (_timerWantFire <= 0)
            {
                RandomizeWantFire();
                ctrl.FireRequest = GeneralCharacterController.ActionRequestType.DoWhenReadyAndFinish;
            }
        }

        private void RandomizeWantFire()
        {
            float mul = _targetEnemy ? 0.2f : 1f;
            _timerWantFire = Random.Range(minPeriodWantFire * mul, maxPeriodWantFire * mul);
        }
    }
}