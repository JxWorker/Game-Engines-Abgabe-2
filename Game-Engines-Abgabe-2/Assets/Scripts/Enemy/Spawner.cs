using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnEnemy;
    [SerializeField] private bool stopSpawning;
    [SerializeField] private float spawnTime;
    [SerializeField] private float spwanDelay;
    
    void Start()
    {
        InvokeRepeating("SpawnObject", spawnTime, spwanDelay);
    }

    public void SpawnObject()
    {
        if (stopSpawning)
        {
            CancelInvoke("SpawnObject");
        }
        Instantiate(spawnEnemy, transform.position, transform.rotation);
    }
}
