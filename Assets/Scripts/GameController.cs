using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{

    bool playersInGame = false;
    public GameObject[] activePlayers;
    public int numPlayers;
    [SerializeField]
    private CamFollow cam;
    [SerializeField]
    private Enemy enemyPrefab;
    [SerializeField]
    private Transform spawnPoint;

    public List<Enemy> enemies;
    // Start is called before the first frame update
    void Start()
    {
        numPlayers = 0;
        activePlayers = new GameObject[4];
        enemies = new List<Enemy>();
        if(spawnPoint==null)
        {
           spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform;
        }
        StartCoroutine(SpawnEnemy());
    }
    private IEnumerator SpawnEnemy()
    {
        Enemy newEnemy;
        float randDiff;
        while(true)
        {
            yield return new WaitForSeconds(5f);
            if (cam.player != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    randDiff = Random.Range(-10, 30);
                    newEnemy = Instantiate(enemyPrefab, new Vector3(cam.player.transform.position.x + randDiff, cam.player.transform.position.y, cam.player.transform.position.z), Quaternion.identity);
                    enemies.Add(newEnemy);
                }
            }
        }
    }
    public void HitEnemies(Vector3 location)
    {
        Vector3 dist;

        foreach(Enemy enemy in enemies)
        {
            dist = enemy.transform.position - location;
            if(dist.magnitude < 2f)
            {
                StartCoroutine(enemy.OnDeath());
            }
        }
        enemies.RemoveAll(enemy => Vector3.Magnitude(enemy.transform.position - location) < 2f);
    }
    public void OnPlayerJoined(PlayerInput p)
    {
        if(!playersInGame)
        {
            playersInGame = true;
            cam.player = p.transform;

        }
        Debug.Log(p + " joined");
        p.transform.position = spawnPoint.position;
        activePlayers[numPlayers++] = p.gameObject;
        cam.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
