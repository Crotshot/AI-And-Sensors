using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Red_AI : Green_AI
{
    /// <summary>
    /// Red AI -> The Sniper
    /// Does not invesigate player noise, smell
    /// Looks in direction if shot
    /// Does not seek player
    /// Shoots at player from position
    /// If player comes within minimumRange neter fleemode
    /// 
    /// If player enters ambush area
    /// </summary>
    [SerializeField] protected float minimumRange;
    float spookedTimer;
    Vector3 ambushPosition, ambushLocation;

    protected override void Ambushing() {
        
    }

    virtual protected void SetToAmbushing() {
        animator.SetTrigger("Ambush");
    }

    public void AmbushTrigger() {
        SetToAmbushing();
    }

    override protected void ObjectsDetected(List<Transform> visibleObjects) {
        if (ai_State == AI_State.Ambushing)
            return;
        base.ObjectsDetected(visibleObjects);
    }
    override protected void Damaged(Vector3 damageOrigin) {
        if (ai_State == AI_State.Ambushing)
            return;
        base.Damaged(damageOrigin);
    }
    override protected void SoundDetected(Vector3 calculatedOrigin) {
        if (ai_State == AI_State.Ambushing)
            return;
        base.SoundDetected(calculatedOrigin);
    }

    override protected void Fleeing() {
        sightInvest = false;
        hitInvest = false;
        //Flee timer decreases
        fleeTimer -= Time.deltaTime;
        shortFleeTimer -= Time.deltaTime;

        if (shortFleeTimer <= 0) {
            SetToWandering();
        }

        if (Helpers.Vector3Distance(navMeshAgent.destination, transform.position) <= orderComlpletion) {//Ammo pack/health pack is pickup, will immediately go to a desired pack if it sees one
            if (FleeCheck()) {
                Debug.Log("Fleeing");
                SetToFleeing();//AI will pick another random point away from the last player target position and go there.
            }
            else {
                SetToPatrolling();
            }
        }
    }

    protected override void SetToInvestigating() {
        if (ai_State == AI_State.Fleeing)
            return;
        ai_State = AI_State.Investigating;
        transform.LookAt(baseTargetPosition);
        navMeshAgent.SetDestination(transform.position);
    }

    protected override void SetToSeeking(Transform targ) {
        if (ai_State == AI_State.Fleeing)
            return;
        base.SetToSeeking(targ);
    }

    protected override void SetToAttacking() {
        if (ai_State == AI_State.Fleeing)
            return;
        base.SetToAttacking();
    }


    override protected bool SetToFleeing() {
        ai_State = AI_State.Fleeing;
        animator.SetBool("Running", true);
        animator.SetBool("Walking", true);
        navMeshAgent.speed = runningSpeed;

        if (wantsHealth)
            packTarget = Helpers.FindClosestTransform(healthPacks, transform.position);
        else if(wantsAmmo)
            packTarget = Helpers.FindClosestTransform(ammoPacks, transform.position);

        if (packTarget != null) {
            packTargetPosition = packTarget.position;
            navMeshAgent.SetDestination(packTargetPosition);
            return true; //Fleeing to a health pack if one is available
        }
        else {
            SetToPointAwayFrombaseTargetPosition();
            return true;
        }
    }

    protected override void Attacking() {
        if (ai_State == AI_State.Fleeing)
            return;
        base.Attacking();
    }

    protected override void SetToAttacking_Ranged() {
        if (ai_State == AI_State.Fleeing)
            return;
        if (Helpers.Vector3Distance(transform.position, baseTarget.position) <= minimumRange) {
            SetToFleeing();
            return;
        }
        base.SetToAttacking_Ranged();
    }
}
