using System.Collections;
using UnityEngine;

// Shared timing helper so multiple death reactions (VFX, loot) can wait for the same
// Animator "Die" state playback instead of duplicating polling logic.
public static class EnemyDeathAnimationWait
{
  const string DieStateName = "Die";

  // The "dead" bool that drives the Die transition is set the same frame death is triggered, so the
  // Animator hasn't necessarily entered Die yet - poll until it does (or bail after the timeout, or if
  // the Animator's GameObject is destroyed first) and then hold for its actual playback length.
  // Waits for the Die state to begin and then waits for that state's playback length.
  public static IEnumerator ForDieState(Animator animator, float timeout)
  {
    float elapsed = 0f;
    while (elapsed < timeout)
    {
      if (animator == null) yield break;

      AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
      if (stateInfo.IsName(DieStateName))
      {
        yield return new WaitForSeconds(stateInfo.length);
        yield break;
      }

      elapsed += Time.deltaTime;
      yield return null;
    }
  }
}
