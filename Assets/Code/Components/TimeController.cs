using UnityEngine;

namespace Code.Components
{
    public class TimeController : MonoBehaviour
    {
        private bool _scaleChanged = false;

        public bool IsTimeScaleChanged() => _scaleChanged;

        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
            _scaleChanged = true;
        }

        public void ResetTimeScale()
        {
            Time.timeScale = 1;
            _scaleChanged = false;
        }

        private void OnDisable()
        {
            ResetTimeScale();
        }
    }
}