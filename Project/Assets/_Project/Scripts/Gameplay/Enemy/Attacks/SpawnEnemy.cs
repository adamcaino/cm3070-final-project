using System;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] AudioClip spawnSfx;

    // Lets callers (e.g. SummonAttack) track the actual enemy this VFX wrapper spawns, rather than the
    // wrapper itself - the wrapper self-destructs with its particle system well before the enemy does.
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
