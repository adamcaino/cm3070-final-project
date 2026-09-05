using UnityEngine;

public class OrcSpinAttackBehaviour : StateMachineBehaviour
{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<SpinAttack>().PlaySpinSfx();
    }
























}
