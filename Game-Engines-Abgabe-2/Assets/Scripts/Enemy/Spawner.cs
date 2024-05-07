using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnEnemy;
    [SerializeField] private bool stopSpawning;
    [SerializeField] private float spawnTime;
    [SerializeField] private float spwanDelay;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnObject", spawnTime, spwanDelay);
    }

    public void SpawnObject()
    {
        Instantiate(spawnEnemy, transform.position, transform.rotation);
        if (stopSpawning)
        {
            CancelInvoke("SpawnObject");
        }
    }
}
