using UnityEngine;





// Fires several projectiles toward the player across a configurable horizontal spread.
public class BossRangedVolleyAttack : AttackBase
{
  [SerializeField] Projectile projectilePrefab;
  [SerializeField] Transform muzzle;
  [SerializeField, Min(0f)] float projectileSpeed = 12f;
  [SerializeField, Min(1)] int projectileCount = 3;
  [SerializeField, Min(0f)] float spreadAngle = 20f;

  // Plays the attack animation, fires the projectile volley, and completes the attack.
  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);
    FireVolley(enemy);
    RaiseAttackComplete();
  }

  // Creates projectiles at evenly spaced angles around the direction to the player.
  void FireVolley(EnemyController enemy)
  {
    if (projectilePrefab == null || enemy.Player == null) return;

    Transform spawnPoint = muzzle != null ? muzzle : enemy.transform;
    Vector3 baseDirection = enemy.Player.position - spawnPoint.position;
    baseDirection.y = 0f;

    float startAngle = -spreadAngle * 0.5f;
    float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;

    for (int i = 0; i < projectileCount; i++)
    {
      float angle = startAngle + (angleStep * i);
      Vector3 direction = Quaternion.Euler(0f, angle, 0f) * baseDirection;

      Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
      projectile.Launch(direction, projectileSpeed, Damage, enemy.gameObject);
    }
  }
}
