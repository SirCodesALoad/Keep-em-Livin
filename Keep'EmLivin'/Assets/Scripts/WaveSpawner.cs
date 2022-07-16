using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{

    public GameMechanicManager manager;


    public List<EnemyType> enemies = new List<EnemyType>();
    public int currWave;
    private int waveValue;
    public List<GameObject> enemiesToSpawn = new List<GameObject>();

    public Transform spawnLocation;
    private float waveTimer = 0f;
    private float spawnInterval = 8f;
    private float spawnTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        GenerateWave();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (manager.gameState)
        {
            case GameState.BetweenWaves:
                break;
            case GameState.OnGoingWave:
                if (spawnTimer <= 0)
                {
                    //spawn an enemy
                    if (enemiesToSpawn.Count > 0)
                    {
                        Instantiate(enemiesToSpawn[0], spawnLocation.position, Quaternion.identity); // spawn first enemy in our list
                        enemiesToSpawn.RemoveAt(0); // and remove it
                        spawnTimer = spawnInterval;
                    }
                    else
                    {
                        waveTimer = 0; // if no enemies remain, end wave
                    }
                }
                else
                {
                    spawnTimer -= Time.fixedDeltaTime;
                }
                break;
        }
        
    }

    public void GenerateWave()
    {
        currWave++;
        waveValue = currWave * 4;
        GenerateEnemies();
        manager.enemiesRemaining = enemiesToSpawn.Count-1;


    }

    public void GenerateEnemies()
    {
        // Create a temporary list of enemies to generate
        // 
        // in a loop grab a random enemy 
        // see if we can afford it
        // if we can, add it to our list, and deduct the cost.

        // repeat... 

        //  -> if we have no points left, leave the loop

        List<GameObject> generatedEnemies = new List<GameObject>();
        while (waveValue > 0)
        {
            int randEnemyId = Random.Range(0, enemies.Count);
            int randEnemyCost = enemies[randEnemyId].cost;

            if (waveValue - randEnemyCost >= 0)
            {
                generatedEnemies.Add(enemies[randEnemyId].enemyPrefab);
                waveValue -= randEnemyCost;
            }
            else if (waveValue <= 0)
            {
                break;
            }
            else
            {
                generatedEnemies.Add(enemies[0].enemyPrefab);
                waveValue -= enemies[0].cost;
            }
        }
        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
    }
}

[System.Serializable]
public class EnemyType
{
    public GameObject enemyPrefab;
    public int cost;
}
