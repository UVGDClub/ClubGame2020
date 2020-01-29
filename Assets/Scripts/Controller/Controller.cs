using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public JoystickAlias[] joystickAlias;
    public ButtonAlias[] buttonAlias;
    public Actor actor;
    void Awake()
    {
        for (int i = 0; i < joystickAlias.Length; i++)
        {
            if(joystickAlias[i].name == "Move")
            {
                joystickAlias[i].OnInput = actor.Move;
            }
            else if(joystickAlias[i].name == "Turn")
            {
                joystickAlias[i].OnInput = actor.Turn;
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < buttonAlias.Length; i++)
        {
            if (Input.GetKeyDown(buttonAlias[i].keyCode))
            {
                buttonAlias[i].OnInput();
            }
        }

        for (int i = 0; i < joystickAlias.Length; i++)
        {
            joystickAlias[i].OnInput(Input.GetAxis(joystickAlias[i].x_axis),
                                     Input.GetAxis(joystickAlias[i].y_axis));
        }
    }
}