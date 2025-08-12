using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ColorUtility = UnityEngine.ColorUtility;

namespace Code.Components
{
    public class UIOverlay : MonoBehaviour
    {
        public GeneralCharacterController playerController;
        public Health playerHealth;
        public Image healthImage;
        public Image dashImage;
        public Image dashLoadingImage;
        public Image reloadProgressImage;
        public TextMeshProUGUI scoreText;
        public CanvasGroup hitmark;
        public CanvasGroup playerOverlay;
        public TextMeshProUGUI ammoText;

        public CanvasGroup diedScreen;

        public AnimationCurve diedAnimationCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.3f, 1f),
            new Keyframe(2f, 0f))
        {
            postWrapMode = WrapMode.Clamp,
            preWrapMode = WrapMode.Clamp
        };

        public TextMeshProUGUI fpsText;

        public AnimationCurve HitmarkAlphaCurve
        {
            get => hitmarkCurve;
            set
            {
                hitmarkCurve = value;
                _hitmarkCurveCacheDirty = true;
            }
        }

        private static readonly Color NorReadyColor = new(1f, 0.5f, 0.5f);
        private static readonly Color ReadyColor = new(0.5f, 1f, 0.5f);

        private static readonly Color ReloadColor = new(0f, 0.4f, 0.9f);
        private static readonly Color CooldownColor = new(0f, 0.9f, 0.4f);

        private bool _dashReady;

        [SerializeField]
        private AnimationCurve hitmarkCurve = new(
            new Keyframe(0f, 1f),
            new Keyframe(0.1f, 0f))
        {
            postWrapMode = WrapMode.Clamp,
            preWrapMode = WrapMode.Clamp
        };

        private bool _hitmarkCurveCacheDirty = true;
        private float _hitmarkProgress;
        private float _hitmarkCurveEndTime;

        private float _fpsCounterTime;
        private int _fpsCounterNumFrames;

        private bool _hasPlayer;

        private float _diedAnimationCurTime = -1f;
        private float _diedAnimationEndTime = -1f;

        private bool lastIsReloading = false;
        private float lastReloadProgress;

        private bool _needUpdateScore = true;

        private int _lastAmmoInMagazine = -1;
        private int _lastAmmoNotInMagazine = -1;

        private GameController _gameController;
        private TimeController _timeController;

        [Inject]
        public void Construct(GameController gameController, TimeController timeController)
        {
            _gameController = gameController;
            _timeController = timeController;
        }

        private void Start()
        {
            diedScreen.gameObject.SetActive(false);

            _gameController.AliveCountChanged += (_, _) => _needUpdateScore = true;
            playerHealth.HealthChanged += OnPlayerHealthChanged;
            playerHealth.BeforeDied += OnPlayerBeforeDied;
            UpdateDash(true);
            UpdateReloadProgress(true);
            StopHitmark();
            UpdateScore();

            Health.AnyHealthChanged += OnAnyHealthChanged;

            _hasPlayer = true;
        }

        private void OnDestroy()
        {
            Health.AnyHealthChanged -= OnAnyHealthChanged;
        }

        private void LateUpdate()
        {
            if (_needUpdateScore)
            {
                _needUpdateScore = false;
                UpdateScore();
            }

            if (_hasPlayer)
            {
                UpdateDash(false);
                UpdateHitmark();
                UpdateReloadProgress(false);
                UpdateAmmo();
            }

            UpdateDiedScreenAnimation();
            UpdateFps();
        }

        private void UpdateFps()
        {
            float dt = Time.unscaledDeltaTime;
            _fpsCounterTime += dt;
            _fpsCounterNumFrames++;

            if (_fpsCounterTime > 1f)
            {
                float fps = _fpsCounterNumFrames / _fpsCounterTime;
                fpsText.text = $"FPS: {fps:F1}";

                _fpsCounterTime = 0f;
                _fpsCounterNumFrames = 0;
            }
        }

        private void UpdateReloadProgress(bool force)
        {
            bool isReloading = playerController.IsReloading;
            float reloadProgress = playerController.RemainingFireTimeNormalized;

            if (force || reloadProgress != lastReloadProgress)
            {
                lastReloadProgress = reloadProgress;
                reloadProgressImage.fillAmount = reloadProgress;
            }

            if (force || isReloading != lastIsReloading)
            {
                lastIsReloading = isReloading;
                Color color = isReloading ? ReloadColor : CooldownColor;
                reloadProgressImage.color = color;
            }
        }

        private void OnAnyHealthChanged(Health health, Health.HealthChangeInfo info)
        {
            if (info.Initiator == playerController.gameObject)
            {
                RunHitmark();
            }
        }

        private void OnPlayerBeforeDied()
        {
            _hasPlayer = false;
            playerController = null;
            playerHealth = null;

            playerOverlay.gameObject.SetActive(false);

            Health.AnyHealthChanged -= OnAnyHealthChanged;

            diedScreen.gameObject.SetActive(true);
            _diedAnimationCurTime = 0f;
            _diedAnimationEndTime = diedAnimationCurve[diedAnimationCurve.length - 1].time;
            UpdateDiedScreenAnimation();
        }

        private void UpdateDiedScreenAnimation()
        {
            if (_diedAnimationCurTime < 0)
            {
                return;
            }

            _diedAnimationCurTime += Time.unscaledDeltaTime;

            float value = diedAnimationCurve.Evaluate(_diedAnimationCurTime);
            diedScreen.alpha = value;
            if (_diedAnimationCurTime <= _diedAnimationEndTime)
            {
                _timeController.SetTimeScale(1f - value);
            }
            else
            {
                diedScreen.gameObject.SetActive(false);
                _diedAnimationCurTime = -1f;
                _timeController.ResetTimeScale();
            }
        }

        private void OnPlayerHealthChanged(Health.HealthChangeInfo info)
        {
            healthImage.fillAmount = playerHealth.CurrentHealthPercentage;
        }

        private void UpdateDash(bool force)
        {
            bool ready = playerController.IsDashReady;
            if (force || ready != _dashReady)
            {
                dashImage.color = ready ? ReadyColor : NorReadyColor;
                _dashReady = ready;
            }

            float dashPercentage = playerController.RemainingDashTimeNormalized;
            dashLoadingImage.fillAmount = dashPercentage;
        }

        private void UpdateScore()
        {
            IReadOnlyCollection<GameController.TeamInfo> allTeams = _gameController.AllTeams;

            StringBuilder builder = new();
            builder.Append("<color=\"white\">");
            foreach (GameController.TeamInfo teamInfo in allTeams)
            {
                Color color = Teams.ToColor(teamInfo.Team);
                string colorString = ColorUtility.ToHtmlStringRGB(color);
                int numAlive = teamInfo.Alive;
                builder.Append($"<color=#{colorString}>{numAlive}</color>");
                builder.Append(':');
            }

            builder.Remove(builder.Length - 1, 1);
            builder.Append("</color>");

            string score = builder.ToString();
            scoreText.text = score;
        }

        private void UpdateAmmo()
        {
            int ammoInMagazine = playerController.AmmoInMagazine;
            int ammoNotInMagazine = playerController.AmmoNotInMagazine;
            if (_lastAmmoInMagazine == ammoInMagazine && _lastAmmoNotInMagazine == ammoNotInMagazine)
            {
                return;
            }

            _lastAmmoInMagazine = ammoInMagazine;
            _lastAmmoNotInMagazine = ammoNotInMagazine;

            ammoText.text = $"<color=\"white\">{ammoInMagazine}/{ammoNotInMagazine}</color>";
        }

        private void UpdateHitmarkCurveCache()
        {
            _hitmarkCurveEndTime = hitmarkCurve[hitmarkCurve.length - 1].time;
        }

        private void RunHitmark()
        {
            _hitmarkProgress = 0f;
            UpdateHitmarkAlpha();
        }

        private void StopHitmark()
        {
            _hitmarkProgress = -1f;
            SetHitmarkAlpha(0f);
        }

        private void UpdateHitmark()
        {
            if (_hitmarkCurveCacheDirty)
            {
                UpdateHitmarkCurveCache();
            }

            if (_hitmarkProgress < 0f)
            {
                return;
            }

            _hitmarkProgress += Time.unscaledDeltaTime;
            if (_hitmarkProgress > _hitmarkCurveEndTime)
            {
                StopHitmark();
                return;
            }

            UpdateHitmarkAlpha();
        }

        private void UpdateHitmarkAlpha()
        {
            float alpha = hitmarkCurve.Evaluate(_hitmarkProgress);
            SetHitmarkAlpha(alpha);
        }

        private void SetHitmarkAlpha(float alpha)
        {
            hitmark.alpha = alpha;
        }
    }
}