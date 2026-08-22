using UnityEngine;

public class RangedAttack : AttackBase
{
  [SerializeField] Projectile projectilePrefab;
  [SerializeField] Transform muzzle;
  [SerializeField, Min(0f)] float projectileSpeed = 12f;
  [SerializeField, Min(1)] int damage = 1;

  protected override void OnExecute(EnemyController enemy)
  {
    PlayAttackAnimation(enemy);

    if (projectilePrefab != null && enemy.Player != null)
    {
      Transform spawnPoint = muzzle != null ? muzzle : enemy.transform;
      Vector3 direction = enemy.Player.position - spawnPoint.position;
      direction.y = 0f;

      Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
      projectile.Launch(direction, projectileSpeed, damage, enemy.gameObject);
    }

    RaiseAttackComplete();
  }
}
