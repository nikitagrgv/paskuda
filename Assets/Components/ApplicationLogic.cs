using UnityEditor;

namespace Components
{
    public static class ApplicationLogic
    {
#if UNITY_EDITOR
        [MenuItem("Window/MaximizeCurrentWindow _F2")]
        static void ToggleCurrentWindowMaximized()
        {
            var window = EditorWindow.focusedWindow;
            if (window == null)
            {
                return;
            }

            window.maximized = !window.maximized;
        }
#endif
    }
}