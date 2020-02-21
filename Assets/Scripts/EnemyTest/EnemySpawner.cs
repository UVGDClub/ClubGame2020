using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    float spawnTimer;
    Vector3 player;
    GameObject playerObj;
    public GameObject BigEnemy;
    public GameObject BasicEnemy;

    public float width;
    public float depth;

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = Time.fixedTime;
        playerObj = GameObject.Find("Player");
        player = playerObj.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        player = playerObj.transform.position;
        float currTimer = Time.fixedTime;
        if(currTimer - spawnTimer > 12)
        {
            /*float x = Random.Range(-width, width);
            float z = Random.Range(-depth, depth);
            Vector3 spawn = new Vector3(x, 1, z);
            Vector3 dist = spawn - player;
            while (dist.magnitude < 16)
            {
                x = Random.Range(-width, width);
                z = Random.Range(-depth, depth);
                spawn = new Vector3(x, 1, z);
                dist = spawn - player;
            }*/
            GameObject newEnemy = Instantiate(BigEnemy);
            spawnTimer = Time.fixedTime;
        }
    }
}
