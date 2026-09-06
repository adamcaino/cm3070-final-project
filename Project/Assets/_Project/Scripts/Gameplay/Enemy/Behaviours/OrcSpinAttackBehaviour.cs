using UnityEngine;

// Plays the spin attack sound when the associated animation state starts.
public class OrcSpinAttackBehaviour : StateMachineBehaviour
{

    // Finds the spin attack component and starts its animation-state sound.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<SpinAttack>().PlaySpinSfx();
    }
























}
