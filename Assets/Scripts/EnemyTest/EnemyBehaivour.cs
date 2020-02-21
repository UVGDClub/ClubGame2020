using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaivour : MonoBehaviour
{
    public float viewDistance;
    public GameObject player;
    NavMeshAgent agent;
    bool triggered;

    float timer;

    public float minGrowelTime;
    public float maxGrowelTime;
    float nextGrowel;

	// Variables for animation
	private Animator anim;
	int Anim_isDead;
	int Anim_isRunning;
	int Anim_wander;

    //new AudioSource[] audio;

	void Update() {
		
	}
	
	
    void Start()
    {
        //audio = GetComponents<AudioSource>();

        agent = GetComponent<NavMeshAgent>();
        triggered = false;
        player = GameObject.Find("Player");
		
		anim = gameObject.GetComponentInChildren<Animator> (); 

		// Random Position Offset upon spawn
        if (player != null)
        {

            Vector3 dist = transform.position - player.transform.position;
            RaycastHit hit;
            do
            {
                float x = Random.Range(-50, 50);
                float z = Random.Range(-50, 50);
                //transform.position = new Vector3( x, player.transform.position.y, z);
                Vector3 location = new Vector3(x, 3, z);
                Vector3 direction = new Vector3(0, -100f, 0);
                Ray ray = new Ray(location, direction);
                Physics.Raycast(ray, out hit);
                //transform.position = new Vector3(x, player.transform.position.y, z);
                Debug.Log(hit.transform.tag);
                dist = transform.position - player.transform.position;
            } while (dist.magnitude < 20);
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;

        if (player != null && Physics.Raycast(transform.position, player.transform.position - transform.position, out hit)
            && hit.distance <= viewDistance && hit.transform.gameObject == player)
        {
            triggered = true;
            int triggerSound = Random.Range(4, 6);
        }

        if (Time.time > nextGrowel)
        {
            int Growel = Random.Range(0, 4);
            //audio[Growel].Play();
            nextGrowel = Time.time + Random.Range(minGrowelTime, maxGrowelTime);
        }

        if (triggered)
        {
            agent.destination = player.transform.position;
			anim.SetInteger ("Anim_isRunning", 1);
        }
        else
        {
			anim.SetInteger ("Anim_isRunning", 0);
            if (Time.fixedTime - timer > 5)
            {
                float x = Random.Range(-1f, 1f);
                float z = Random.Range(-1f, 1f);
                Vector3 direction = new Vector3(x, 0f, z);
                Ray ray = new Ray(transform.position, direction);

                RaycastHit hit_;
                if (Physics.Raycast(transform.position, direction, out hit_))
                {
                    agent.destination = hit_.point - direction.normalized;
                    Debug.Log("Enemy random Movement");
                }
                timer = Time.fixedTime;
				if(Vector3.Distance(agent.destination,transform.position) > 1)
				{
					anim.SetInteger ("Anim_wander", 1);
					Debug.Log("Enemy random Movement");
				}
				else 
					anim.SetInteger ("Anim_wander", 0);
            }

        }
    }
}
