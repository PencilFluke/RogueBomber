using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private int wave = 1;
    [SerializeField] private int startingEnemyCount = 1;
    [SerializeField] private Bounds spawnBounds;
    private bool waveCleared;
    private bool waveStarted = true;
    [SerializeField] private int waveCountdown = 3;
    [SerializeField] private GameObject enemyPrefab;
    GameObject[] enemies;

    void Awake()
    {
        spawnBounds = GetComponentInChildren<MeshRenderer>().bounds;
        Debug.Log(spawnBounds.extents);
        InvokeRepeating("SpawnWave", waveCountdown, 1);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    private void SpawnWave()
    {
        if (isWaveCleared())
        {
            waveStarted = true;
            int enemyCount = wave + startingEnemyCount;
            for (int i = 1; i < enemyCount; i++)
            {
                Instantiate(enemyPrefab, GetSpawnPosition(), Quaternion.identity);
            }

        }
    }

    private Vector3 GetSpawnPosition()
    {
        float radius = Mathf.Sqrt(Mathf.Pow(spawnBounds.extents.x, 2f) + Mathf.Pow(spawnBounds.extents.x, 2f));
        Vector2 randomDirection = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized;
        Vector2 randomPointInRadius = randomDirection * radius;
        Vector3 randomPointInBounds = spawnBounds.ClosestPoint(new Vector3(randomPointInRadius.x, 0f, randomPointInRadius.y));
        Debug.Log(randomPointInBounds);
        return randomPointInBounds;
    }

    private bool isWaveCleared()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0 && waveStarted)
        {
            waveCleared = true;
            wave++;
        }
        else waveCleared = false;

        Debug.Log("Wave cleared: " + waveCleared);
        Debug.Log("Wave started: " + waveStarted);
        Debug.Log("Enemy count: " + enemies.Length);
        return waveCleared;
    }

}
