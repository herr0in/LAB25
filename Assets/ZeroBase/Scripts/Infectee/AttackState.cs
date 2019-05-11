﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    public float attackTime = 0.7f;
    private Her0inEnemy myInfectee;
    private bool doAttack = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        doAttack = false;
        myInfectee = animator.transform.GetComponent<Her0inEnemy>();
		myInfectee.isAttack = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (doAttack)
            return;

        if (stateInfo.normalizedTime >= attackTime)
        {
            if (Vector3.Distance(PlayerCtrl.myPos.position, animator.transform.position) <= myInfectee.attackRange)
			{
          
				//myInfectee.damagedEffect.Hit();
				PlayerManager.ApplyDamage(10);
			}

			doAttack = true;
            myInfectee.isAttack = false;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
       
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
