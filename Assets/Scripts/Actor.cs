using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class Actor : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 2f;

    public Rigidbody2D rb;

    public InputActionAsset controls;

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        rb.velocity = controls["Move"].ReadValue<Vector2>();

        transform.right = controls["Turn"].ReadValue<Vector2>();

        if (controls["Attack"].triggered)
            Debug.Log("Attack");
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(rb.position, (transform.position + transform.right));
    }
}
