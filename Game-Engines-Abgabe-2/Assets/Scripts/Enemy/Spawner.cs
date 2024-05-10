using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnEnemy;
    [SerializeField] private bool stopSpawning;
    [SerializeField] private float spawnTime;
    [SerializeField] private float spawnDelay;
    [SerializeField] private KeyCode toggleSpawner = KeyCode.T;
    private int _enemyCount;
    private bool _startInvoke;

    void Start()
    {
        InvokeRepeating("SpawnObject", spawnTime, spawnDelay);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleSpawner))
        {
            stopSpawning = !stopSpawning;
        }

        if (_startInvoke && !stopSpawning)
        {
            InvokeRepeating("SpawnObject", spawnTime, spawnDelay);
            _startInvoke = false;
        }
    }

    public void SpawnObject()
    {
        if (stopSpawning)
        {
            CancelInvoke("SpawnObject");
            _startInvoke = true;
        }

        Instantiate(spawnEnemy, transform.position, transform.rotation);
        _enemyCount++;
    }
}