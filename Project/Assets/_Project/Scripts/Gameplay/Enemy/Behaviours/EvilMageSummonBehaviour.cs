using UnityEngine;

// Starts and stops the summon charge effect with the animation state.
public class EvilMageSummonBehaviour : StateMachineBehaviour
{

    // Starts the summon charge visual effect when the animation state begins.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<SummonAttack>()?.PlaySummonAnimation(true);
    }








    // Stops the summon charge visual effect when the animation state ends.
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<SummonAttack>()?.PlaySummonAnimation(false);
    }












}
