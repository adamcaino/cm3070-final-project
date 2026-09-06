using UnityEngine;

// Fires one projectile toward the player when the ranged attack executes.
public class RangedAttack : AttackBase
{
  [SerializeField] Projectile projectilePrefab;
  [SerializeField] Transform muzzle;
  [SerializeField, Min(0f)] float projectileSpeed = 12f;

  // Plays the attack animation, launches the projectile, and completes the attack.
  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);

    if (projectilePrefab != null && enemy.Player != null)
    {
      Transform spawnPoint = muzzle != null ? muzzle : enemy.transform;
      Vector3 direction = enemy.Player.position - spawnPoint.position;
      direction.y = 0f;

      Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
      projectile.Launch(direction, projectileSpeed, Damage, enemy.gameObject);
    }

    RaiseAttackComplete();
  }
}
