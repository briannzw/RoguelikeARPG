using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerAction playerAction;
    public static event Action<InputActionMap> actionMapChange;

    private void Awake()
    {
        playerAction = new PlayerAction();
        ToggleActionMap(playerAction.Gameplay);
    }

    public static void ToggleActionMap(InputActionMap actionMap)
    {
        if (actionMap.enabled) return;
        playerAction.Disable();
        actionMapChange?.Invoke(actionMap);
        actionMap.Enable();
    }
}
