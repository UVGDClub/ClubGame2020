using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Joystick Alias", menuName = "Input Alias/Joystick Alias")]
public class JoystickAlias : ScriptableObject
{
    public string x_axis;
    public string y_axis;

    public delegate void InputAction(float x, float y);
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
