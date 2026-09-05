using System;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] AudioClip spawnSfx;



    public event Action<GameObject> OnEnemySpawned;

    void Start()
    {
        if (enemyPrefab != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            OnEnemySpawned?.Invoke(enemy);
        }

        if (spawnSfx != null)
        {
            GetComponent<AudioSource>()?.PlayOneShot(spawnSfx);
        }
    }
}
