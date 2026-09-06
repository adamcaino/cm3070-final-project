using System.Collections;
using UnityEngine;

// Reveals a spawned pickup after the source's death animation reaches its end state.
public class DelayedPickupReveal : MonoBehaviour
{
  Renderer[] renderers;
  Collider[] colliders;

  // Hides the pickup and begins waiting for the source's death animation to finish.
  public void RevealAfterDieAnimation(Animator sourceAnimator, bool skipAnimationWait, float dieStateDetectTimeout)
  {
    renderers = GetComponentsInChildren<Renderer>();
    colliders = GetComponentsInChildren<Collider>();

    SetVisible(false);
    StartCoroutine(RevealRoutine(sourceAnimator, skipAnimationWait, dieStateDetectTimeout));
  }

  // Waits for the source animation unless skipped, then enables the pickup components.
  IEnumerator RevealRoutine(Animator sourceAnimator, bool skipAnimationWait, float dieStateDetectTimeout)
  {
    if (!skipAnimationWait)
    {
      yield return EnemyDeathAnimationWait.ForDieState(sourceAnimator, dieStateDetectTimeout);
    }

    SetVisible(true);
  }

  // Enables or disables every renderer and collider belonging to the pickup hierarchy.
  void SetVisible(bool visible)
  {
    foreach (Renderer r in renderers) r.enabled = visible;
    foreach (Collider c in colliders) c.enabled = visible;
  }
}
