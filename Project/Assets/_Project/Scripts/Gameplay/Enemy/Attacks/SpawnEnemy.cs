using System;
using UnityEngine;
using UnityEngine.AI;

// Spawns one enemy on the local NavMesh and reports the spawned object.
public class SpawnEnemy : MonoBehaviour
{
    const float NavMeshSampleDistance = 2f;

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] AudioClip spawnSfx;



    public event Action<GameObject> OnEnemySpawned;

    // Samples a valid NavMesh position, spawns the enemy, raises the event, and plays the SFX.
    void Start()
    {
        if (enemyPrefab != null)
        {
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit navMeshHit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                Debug.LogWarning($"{nameof(SpawnEnemy)} skipped {enemyPrefab.name} at {transform.position} because no NavMesh was found within {NavMeshSampleDistance} units.", this);
                return;
            }

            GameObject enemy = Instantiate(enemyPrefab, navMeshHit.position, Quaternion.identity);
            OnEnemySpawned?.Invoke(enemy);
        }

        if (spawnSfx != null)
        {
            GetComponent<AudioSource>()?.PlayOneShot(spawnSfx);
        }
    }
}
