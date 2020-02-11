using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    Vector3 offset;

    public Transform player;
    public float smoothing = 5f;
	public float offsetX = 0f;
	public float offsetY = 10f;
	public float offsetZ = -10f;
	public float angleX = 45f;
	public float angleY = 0f;
	public float angleZ = 0f;

    private void Update()
    {
		offset = new Vector3(offsetX, offsetY, offsetZ);
		transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPos = player.position + offset;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothing * Time.deltaTime);
    }
}
