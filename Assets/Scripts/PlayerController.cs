using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController
{
    public InputAction move;

    public void Init()
    {
        move = new InputAction(binding: "*/Move");
    }
}
