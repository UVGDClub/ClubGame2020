using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerMoveScript : MonoBehaviour
{
	// Start is called before the first frame update

	public float moveSpeed = 5f;
	public Rigidbody rb;

	// Variables for animation
	private Animator anim;
	int isSwinging;
	int isDead;
	int isDying;
	//-------------------------

	int moveX;
	int moveY;
	float root2;
	new AudioSource[] audio;

	void Start()
	{
		audio = GetComponents<AudioSource>();
		rb = GetComponent<Rigidbody>();
		// Variables for animation
		anim = gameObject.GetComponentInChildren<Animator>(); 
		isSwinging = 0;
		isDead = 0;
		isDying = 60;
	}

	// Update is called once per frame
	private void Update()
	{
		moveX = (int)Input.GetAxisRaw("Horizontal");
		moveY = (int)Input.GetAxisRaw ("Vertical");
		if(Mathf.Abs(moveX) > 0 && Mathf.Abs(moveY) > 0) {
			root2 = 1f/Mathf.Sqrt(2);
		} else {
			root2 = 1f;
		}
		rb.velocity = new Vector3(moveX*moveSpeed*root2, rb.velocity.y, moveY*moveSpeed*root2);
		//Debug.Log(Input.GetAxis("Horizontal"));
		//transform.right = controls["Turn"].ReadValue<Vector2>();

		// Animation -------------------------------------------------------------
		if(Input.GetMouseButtonDown(0)) {
			anim.SetInteger ("Anim_isSwinging", 1);
		}
		else {
			anim.SetInteger ("Anim_isSwinging", 0);
		}

		if ((moveX != 0) || (moveY != 0)) {
			anim.SetInteger ("Anim_isRunning", 1);
		} else {
			anim.SetInteger ("Anim_isRunning", 0);
		}
		//Turning the player with the motion
		if (moveX != 0) {
			transform.rotation = Quaternion.Euler (0, (moveX * 90) - (moveX*moveY*45), 0);
		} else if (moveY != 0) {
			if (moveY > 0) {
				transform.rotation = Quaternion.Euler (0, 0, 0);
			}
			if (moveY < 0) {
				transform.rotation = Quaternion.Euler (0, 180, 0);
			}
		}


	}
}