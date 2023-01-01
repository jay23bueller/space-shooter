using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroyedBehaviour : StateMachineBehaviour
{

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AudioSource>().Play();
        animator.GetComponent<Enemy>().InformSpawnManager(stateInfo.length * .4f);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if(stateInfo.normalizedTime >= 1f)
            animator.GetComponent<Enemy>().Die();
    }

}
