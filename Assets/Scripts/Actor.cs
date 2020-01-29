using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 2f;

    public Rigidbody2D rb;

    public void Move(float x, float y)
    {
        if (Mathf.Abs(x) <= 0.1f && Mathf.Abs(y) <= 0.1f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Debug.Log("move");
        rb.velocity = new Vector2(x, y).normalized * moveSpeed;
    }

    public void Turn(float x, float y)
    {
        Debug.Log("turn: " + x + ", " + y);

        if (Mathf.Abs(x) <= 0.3333f)
            x = 0;
        if(Mathf.Abs(y) <= 0.3333f)
            y = 0;

        if (x == 0 && y == 0)
            return;

        transform.right = new Vector3(x, y, 0);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(rb.position, (transform.position + transform.right));
    }
}
