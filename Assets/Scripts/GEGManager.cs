using UnityEngine;
using GEGFramework;
using UnityEngine.Events;
using System.Collections.Generic;

public class GEGManager : MonoBehaviour {
    // public UnityEvent onPlayerKilled;
    // public UnityEvent onEnemyKilled;
    public UnityEvent onDifficultyChanged; // triggered when new difficulty level is computed

    static GEGPackedData packedData;
    static GEGScoreManager scoreManager;
    static GEGLevelController controller;

    [SerializeField, Range(0, 10)] int defaultDiff; // prompt for default difficulty level for this scene
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] List<Transform> enemySpawnPoints;
    [SerializeField] int minHealth = 10, maxHealth = 100;
    [SerializeField] int minSpeed = 1, maxSpeed = 10;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] bool randomSpawn = false;
    [SerializeField] List<GEGTypeContainer> playerData;
    [SerializeField] List<GEGTypeContainer> enemyData;

    float spawnTimer;
    float diffEvalTimer; // countdown timer for difficulty evaluation

    void Start() {
        packedData = new GEGPackedData();
        controller = new GEGLevelController();
        diffEvalTimer = GEGPackedData.diffEvalInterval;
        scoreManager = new GEGScoreManager(defaultDiff); // test values
        spawnTimer = spawnInterval; // init timer
    }

    void Update() {
        diffEvalTimer -= Time.deltaTime;

        spawnTimer -= Time.deltaTime; // timer countdown
        if (spawnTimer <= 0f)
        {
            if (randomSpawn)
            {
                int randEnemy = Random.Range(0, enemyPrefabs.Count);
                int randSpawnPoint = Random.Range(0, enemySpawnPoints.Count);
                Instantiate(enemyPrefabs[randEnemy], enemySpawnPoints[randSpawnPoint].position,
                    transform.rotation); // instantiate a random enemy at random position
            }
            else
            {
                Instantiate(enemyPrefabs[0], enemySpawnPoints[0].position, transform.rotation);
            }
            spawnTimer = spawnInterval; // reset spawn timer
        }

        if (diffEvalTimer <= 0) {
            int newDiffLevel = scoreManager.GetDifficulty(3,5,3); // test values
            List<int> res = controller.RunExample(newDiffLevel);
            string s = "";
            Debug.Log("-------------------------");
            for (int i = 0; i < res.Count; ++i)
            {
                s += res[i].ToString() + ",";
            }
            Debug.Log(s);
            Debug.Log(newDiffLevel);
            Debug.Log("-------------------------");
            diffEvalTimer = GEGPackedData.diffEvalInterval;
            //onDifficultyChanged.Invoke();
        }
    }
}
