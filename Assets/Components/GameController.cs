using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Keyboard.current is not { } keyboard)
        {
            return;
        }
    }
}