using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using StateMachine;

public class Actor2D : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Rigidbody2D rb;
    public void OnDrawGizmos()
    {
        Gizmos.DrawLine(rb.position, (transform.position + transform.right));
    }
}
