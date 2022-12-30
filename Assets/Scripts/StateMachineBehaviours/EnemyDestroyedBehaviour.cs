using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroyedBehaviour : StateMachineBehaviour
{
    private bool _toldSpawnManager;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AudioSource>().Play();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!_toldSpawnManager && stateInfo.normalizedTime > .4f && stateInfo.normalizedTime < 1f)
        {
            _toldSpawnManager = true;
            animator.GetComponent<Enemy>().InformSpawnManager();
        }
        if(stateInfo.normalizedTime >= 1f)
            animator.GetComponent<Enemy>().Die();
    }

}
