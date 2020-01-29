using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Button Alias", menuName = "Input Alias/Button Alias")]
public class ButtonAlias : ScriptableObject
{
    public KeyCode keyCode;
    public delegate void InputAction();
    public InputAction OnInput;

    public void AddAction(InputAction a)
    {
        OnInput += a;
    }

    public void AssignAction(InputAction a)
    {
        OnInput = a;
    }
    public void ClearActions()
    {
        OnInput = null;
    }

    public void RemoveAction(InputAction a)
    {
        OnInput -= a;
    }
}
