using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroyedBehaviour : StateMachineBehaviour
{

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.normalizedTime >= 1f)
            animator.GetComponent<Enemy>().Die();
    }

}
