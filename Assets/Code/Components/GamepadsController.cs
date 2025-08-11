using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class GamepadsController : MonoBehaviour
{
    void Start()
    {
        List<Gamepad> xBoxGamepads = new();
        List<Gamepad> otherGamepads = new();

        ReadOnlyArray<Gamepad> gamepads = Gamepad.all;
        foreach (Gamepad gamepad in gamepads)
        {
            if (gamepad.name.StartsWith("XInput"))
            {
                xBoxGamepads.Add(gamepad);
            }
            else
            {
                otherGamepads.Add(gamepad);
            }
        }

        if (xBoxGamepads.Count > 0)
        {
            foreach (Gamepad gamepad in otherGamepads)
            {
                Debug.Log($"Removed gamepad: {gamepad.name} (Path: {gamepad.device.path})");
                InputSystem.RemoveDevice(gamepad.device);
            }
        }

        gamepads = Gamepad.all;
        foreach (Gamepad gamepad in gamepads)
        {
            Debug.Log($"Used gamepad: {gamepad.name} (Path: {gamepad.device.path})");
        }
    }
}