using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System;

public class PlayerMoveScript : MonoBehaviour
{

	private ControlsMaster controls;
	// Start is called before the first frame update

	public float moveSpeed = 5f;
	public Rigidbody rb;

	// Variables for animation
	private Animator anim;
	int isSwinging;
	int isDead;
	int isDying;
	//-------------------------

	float moveX;
	float moveY;
	Vector2 direction;
	float root2;
	new AudioSource[] audio;
	private GameController gameController;
	private void Awake()
	{
		gameController = FindObjectOfType<GameController>();
		anim = gameObject.GetComponentInChildren<Animator>();

		/*controls = new ControlsMaster();
		controls.PlayerInput.Move.performed += context => Move(context.ReadValue<Vector2>());*/

	}
	private void OnEnable()
	{
		//controls.Enable();
	}
	private void OnDisable()
	{
		//controls.Disable();
	}
	
	void Start()
	{
		
		audio = GetComponents<AudioSource>();
		rb = GetComponent<Rigidbody>();
		// Variables for animation

		isSwinging = 0;
		isDead = 0;
		isDying = 60;
	}

	private void Attack()
	{
		
		if(!gameObject.activeInHierarchy)
		{
			return;
		}
		anim.SetInteger("Anim_isRunning", 0);
		anim.SetInteger("Anim_isSwinging", 1);
		StartCoroutine(swingBat());
		//rb.transform.position = new Vector3(rb.transform.position.x + (direction.x * moveSpeed), rb.transform.position.y, rb.transform.position.z + (direction.y * moveSpeed));
	}
	public void Move(InputAction.CallbackContext value)
	{ 
		
        if(!gameObject.activeInHierarchy)
		{
			return;
		}
		direction = value.ReadValue<Vector2>();
		anim.SetInteger("Anim_isRunning", 1);
		//Turning the player with the motion
		if (direction.x != 0)
		{
			transform.rotation = Quaternion.Euler(0, (direction.x * 90) - (direction.x * direction.y * 45), 0);
		}
		else if (direction.y != 0)
		{
			if (direction.y > 0)
			{
				transform.rotation = Quaternion.Euler(0, 0, 0);
			}
			if (direction.y < 0)
			{
				transform.rotation = Quaternion.Euler(0, 180, 0);
			}
		}
	}

	private IEnumerator swingBat()
    {
		/*
		Vector3 location = new Vector3(transform.position.x, 1f, transform.position.z);
		Vector3 direction = transform.forward;
		Ray ray = new Ray(location, direction);
		RaycastHit hit;
		Physics.Raycast(ray, out hit, maxDistance: 2f);


        if(hit.transform && hit.transform.CompareTag("Enemy"))
        {
            Destroy(hit.transform.gameObject);

		}*/
		//Wait for animation to be half finished
		yield return new WaitForSeconds(0.32f);
		gameController.HitEnemies(rb.transform.position);
		anim.SetInteger("Anim_isSwinging", 0);

	}

	// Update is called once per frame
	private void Update()
	{
		
		//movementInput = controls.PlayerInput.Move.ReadValue<Vector2>();

		moveX = direction.x;
		moveY = direction.y;
		//Debug.Log("moveX is " + moveX + " and moveY is " + moveY);
		if (Mathf.Abs(moveX) > 0 && Mathf.Abs(moveY) > 0) {
			root2 = 1f/Mathf.Sqrt(2);
		} else {
			root2 = 1f;
		}
		rb.velocity = new Vector3(moveX*moveSpeed*root2, rb.velocity.y, moveY*moveSpeed*root2);
		//Debug.Log(Input.GetAxis("Horizontal"));
		//transform.right = controls["Turn"].ReadValue<Vector2>();

		// Animation -------------------------------------------------------------


		if ((moveX == 0) && (moveY == 0) && anim.GetInteger("Anim_isRunning")==1) {
			anim.SetInteger ("Anim_isRunning", 0);
		}
		


	}
}