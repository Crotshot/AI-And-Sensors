using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushController : StateMachineBehaviour
{
    [SerializeField] GameObject grenade;
    Red_AI ai;
    Vector3 ambushLocation, ambushDestination;
    bool grenadeThrown;

    //OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("State Enter");
        ai = animator.GetComponent<Red_AI>();
        ambushLocation = ai.GetAmbushLocation();
        ambushDestination = ai.GetAmbushDestination();
        ai.GoToLocation(ambushDestination);
    }

    //OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (Time.timeScale == 0)
            return;
        Debug.Log("State Update");

        if (stateInfo.IsName("throwing") && !grenadeThrown) {
            grenadeThrown = true;
            Debug.Log("Grenade thrown");
            Rigidbody rb = Instantiate(grenade, ai.transform.position + Vector3.up * 2f, ai.transform.rotation).GetComponent<Rigidbody>();
            rb.AddForce(ai.transform.forward * 350f + Vector3.up * 350f);
            animator.SetBool("Ambush_Complete", true);
            ai.AmbushComplete();
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
