using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroyedBehaviour : StateMachineBehaviour
{

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AudioSource>().Play();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.normalizedTime >= 1f)
            animator.GetComponent<Enemy>().Die();
    }

}
