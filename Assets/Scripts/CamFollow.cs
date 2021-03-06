﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    Vector3 offset;

	Camera cam;

    public Transform player;
    public float smoothing = 5f;
	public float offsetX = 0f;
	public float offsetY = 10f;
	public float offsetZ = -10f;
	public float angleX = 45f;
	public float angleY = 0f;
	public float angleZ = 0f;

	public GameObject leftEdge;
	public GameObject rightEdge;
	public float edgeOffset = 0.1f;

	public float FoV = 60f;

	private void Start()
	{
		cam = GetComponent<Camera>();

		leftEdge.transform.position = cam.ViewportToWorldPoint(new Vector3(edgeOffset, 0.5f, player.position.z));
		rightEdge.transform.position = cam.ViewportToWorldPoint(new Vector3(1 - edgeOffset, 0.5f, player.position.z));
	}

	private void Update()
    {
		cam.fieldOfView = FoV;
		offset = new Vector3(offsetX, offsetY, offsetZ);
		transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if (player != null)
		{
			Vector3 targetPos = player.position + offset;

			transform.position = Vector3.Lerp(transform.position, targetPos, smoothing * Time.deltaTime);
		}
    }
}
