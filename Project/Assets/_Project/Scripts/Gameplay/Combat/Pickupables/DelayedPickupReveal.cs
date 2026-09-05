using System.Collections;
using UnityEngine;

// Hides a freshly spawned pickup's renderers and collider until the source's death animation
// finishes. Lets a spawner capture a drop position and instantiate immediately - independent of
// the spawner's own lifetime, since the source may be destroyed once its animation completes.
public class DelayedPickupReveal : MonoBehaviour
{
  Renderer[] renderers;
  Collider[] colliders;

  public void RevealAfterDieAnimation(Animator sourceAnimator, bool skipAnimationWait, float dieStateDetectTimeout)
  {
    renderers = GetComponentsInChildren<Renderer>();
    colliders = GetComponentsInChildren<Collider>();

    SetVisible(false);
    StartCoroutine(RevealRoutine(sourceAnimator, skipAnimationWait, dieStateDetectTimeout));
  }

  IEnumerator RevealRoutine(Animator sourceAnimator, bool skipAnimationWait, float dieStateDetectTimeout)
  {
    if (!skipAnimationWait)
    {
      yield return EnemyDeathAnimationWait.ForDieState(sourceAnimator, dieStateDetectTimeout);
    }

    SetVisible(true);
  }

  void SetVisible(bool visible)
  {
    foreach (Renderer r in renderers) r.enabled = visible;
    foreach (Collider c in colliders) c.enabled = visible;
  }
}
