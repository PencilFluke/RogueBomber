using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] static public int wave = 1;
    [SerializeField] private Bounds spawnBounds;
    private bool waveCleared;
    private bool waveStarted = true;
    [SerializeField] private int waveCountdown = 3;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] public float difficultyScale = 1f;
    [SerializeField] private float difficultyMultiplier = 0.1f;
    GameObject[] enemies;

    void Awake()
    {
        spawnBounds = GetComponentInChildren<MeshRenderer>().bounds;
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
            difficultyScale *= 1 + difficultyMultiplier;
            int enemyCount = Mathf.Min(Enemy.maxCount, wave);
            for (int i = 1; i < enemyCount; i++)
            {
                GameObject enemy = Instantiate(enemyPrefab, GetSpawnPosition(), Quaternion.identity);
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                enemyScript.maxHealth *= difficultyScale;
                enemyScript.drops.ForEach(d => d.amount = Mathf.CeilToInt(difficultyMultiplier * wave * d.amount));
            }


        }
    }

    private Vector3 GetSpawnPosition()
    {
        float radius = Mathf.Sqrt(Mathf.Pow(spawnBounds.extents.x, 2f) + Mathf.Pow(spawnBounds.extents.x, 2f));
        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Vector2 randomPointInRadius = randomDirection * radius;
        Vector3 randomPointInBounds = spawnBounds.ClosestPoint(new Vector3(randomPointInRadius.x, 0f, randomPointInRadius.y));
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
        return waveCleared;
    }


}
