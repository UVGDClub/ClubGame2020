using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor2D
{
    public ControlsMaster controls;

    bool lockControls;
    float lockTimer;

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Awake()
    {
        Init();
    }

    //todo: make this be called by our game controller
    private void Init()
    {
        controls = new ControlsMaster();
        StartCoroutine(HandleInput());
    }

    public IEnumerator HandleInput()
    {
        for(; ; )
        {
            rb.velocity = moveSpeed * controls.PlayerInput.Move.ReadValue<Vector2>();
            
            Vector2 turn = controls.PlayerInput.Turn.ReadValue<Vector2>();
            transform.right = Vector2.MoveTowards(transform.right, turn.normalized, 0.1f);
            if ((transform.right.x < 0 && turn.x > 0) || (transform.right.x > 0 && turn.x < 0)
                || (transform.right.y < 0 && turn.y > 0) || (transform.right.y > 0 && turn.y < 0))
            {
                transform.right = turn.normalized;
            }

            if (lockControls)
            {
                while (lockControls)
                {
                    if (lockTimer < 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        while (Time.time < lockTimer)
                            yield return null;

                        lockControls = false;
                    }
                }
            }

            yield return null;
        }
    }
}
