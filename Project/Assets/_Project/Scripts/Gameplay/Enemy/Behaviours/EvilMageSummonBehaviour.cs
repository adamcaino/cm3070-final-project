using UnityEngine;

public class EvilMageSummonBehaviour : StateMachineBehaviour
{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<SummonAttack>()?.PlaySummonAnimation(true);
    }








    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<SummonAttack>()?.PlaySummonAnimation(false);
    }












}
