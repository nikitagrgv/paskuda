using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

namespace Components
{
    public class UIOverlay : MonoBehaviour
    {
        public GameController gameController;
        public ProjectileManager projectileManager;
        public GeneralCharacterController playerController;
        public Health playerHealth;
        public Image healthImage;
        public Image dashImage;
        public Image dashLoadingImage;
        public Image reloadProgressImage;
        public TextMeshProUGUI scoreText;
        public CanvasGroup hitmark;

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

        private readonly Color _norReadyColor = new(1f, 0.5f, 0.5f);
        private readonly Color _readyColor = new(0.5f, 1f, 0.5f);

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

        private void Start()
        {
            gameController.AliveCountChanged += (_, _) => UpdateScore();
            playerHealth.HealthChanged += OnPlayerHealthChanged;
            UpdateDash(true);
            StopHitmark();
            UpdateScore();

            Health.AnyHealthChanged += OnAnyHealthChanged;
        }

        private void LateUpdate()
        {
            UpdateDash(false);
            UpdateHitmark();
            UpdateReloadProgress();
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

        private void UpdateReloadProgress()
        {
            float reload = playerController.RemainingReloadTimeNormalized;
            reloadProgressImage.fillAmount = reload;
        }

        private void OnAnyHealthChanged(Health health, Health.HealthChangeInfo info)
        {
            if (info.Initiator == playerController.gameObject)
            {
                RunHitmark();
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
                dashImage.color = ready ? _readyColor : _norReadyColor;
                _dashReady = ready;
            }

            float dashPercentage = playerController.RemainingDashTimeNormalized;
            dashLoadingImage.fillAmount = dashPercentage;
        }

        private void UpdateScore()
        {
            if (!gameController.IsSpawnFinished)
            {
                return;
            }

            IReadOnlyCollection<GameController.TeamInfo> allTeams = gameController.AllTeams;

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