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
    [SerializeField] Transform ambushDest, ambushLocation;

    protected override void Start() {
        if (weap == null) {
            Debug.LogError("Red AI: " + name + " needs to have a weapon assigned before creation!");
        }
        ambushDest.parent = null;
        ambushLocation.parent = null;
        base.Start();
    }

    protected override void Ambushing() {
        if (Helpers.RoundVector3(navMeshAgent.destination) != Helpers.RoundVector3(ambushDest.position)) {
            Debug.Log(navMeshAgent.destination + "    :   " + ambushDest.position);
            Debug.Log("(" + navMeshAgent.destination.x  + ", " + navMeshAgent.destination.y + ", " + navMeshAgent.destination.z + ")    :   (" + ambushDest.position.x + ", " + ambushDest.position.y + ", " + ambushDest.position.z + ")");
            return;
        }
        if (Helpers.Vector3Distance(navMeshAgent.destination, transform.position) <= orderComlpletion) {
            transform.LookAt(ambushLocation);
            Debug.Log("Ready to throw grenade");
            animator.SetTrigger("Ambush_Ready");
        }
        else {
            Debug.Log("Going to ambush destination");
        }
    }

    virtual protected void SetToAmbushing() {
        ai_State = AI_State.Ambushing;
        navMeshAgent.speed = runningSpeed;
        animator.SetTrigger("Ambush");
        animator.SetBool("Shooting", false);
    }

    public void AmbushTrigger() {
        if (ai_State == AI_State.Ambushing || animator.GetBool("Ambush_Complete")) {
            Debug.Log("Aborting Ambush");
            return;
        }
        SetToAmbushing();
    }

    override protected void ObjectsDetected(List<Transform> visibleObjects) {
        if (ai_State == AI_State.Ambushing || ai_State == AI_State.Fleeing)
            return;
        base.ObjectsDetected(visibleObjects);
    }
    override protected void Damaged(Vector3 damageOrigin) {
        if (ai_State == AI_State.Ambushing || ai_State == AI_State.Fleeing)
            return;
        base.Damaged(damageOrigin);
    }
    override protected void SoundDetected(Vector3 calculatedOrigin) {
        if (ai_State == AI_State.Ambushing || ai_State == AI_State.Fleeing)
            return;
        base.SoundDetected(calculatedOrigin);
    }

    override protected void Fleeing() {
        sightInvest = false;
        hitInvest = false;

        shortFleeTimer -= Time.deltaTime;

        if (shortFleeTimer <= 0) {
            SetToPatrolling();
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
        if (ai_State == AI_State.Fleeing || ai_State == AI_State.Ambushing)
            return;
        ai_State = AI_State.Investigating;
        animator.SetBool("Running", false);
        animator.SetBool("Walking", false);
        transform.LookAt(baseTargetPosition);
        navMeshAgent.SetDestination(transform.position);
    }

    protected override void SetToSeeking(Transform targ) {
        if (ai_State == AI_State.Fleeing || ai_State == AI_State.Ambushing)
            return;
        base.SetToSeeking(targ);
    }

    protected override void SetToAttacking() {
        if (ai_State == AI_State.Fleeing || ai_State == AI_State.Ambushing)
            return;
        base.SetToAttacking();
    }


    override protected bool SetToFleeing() {
        if (ai_State == AI_State.Ambushing)
            return false;

        ai_State = AI_State.Fleeing;
        animator.SetBool("Running", true);
        animator.SetBool("Walking", true);
        animator.SetBool("Shooting",false);
        navMeshAgent.speed = runningSpeed;


        shortFleeTimer = shortFleeTime + Random.Range(-shortFleeTimeDeviation, shortFleeTimeDeviation);

        if (wantsHealth)
            packTarget = Helpers.FindClosestTransform(healthPacks, transform.position);
        else if (wantsAmmo)
            packTarget = Helpers.FindClosestTransform(ammoPacks, transform.position);
        else
            packTarget = null;

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
        if (ai_State == AI_State.Fleeing || ai_State == AI_State.Ambushing)
            return;
        base.Attacking();
    }

    protected override void SetToAttacking_Ranged() {
        if (ai_State == AI_State.Fleeing || ai_State == AI_State.Ambushing)
            return;
        if (Helpers.Vector3Distance(transform.position, baseTarget.position) <= minimumRange) {
            SetToFleeing();
            return;
        }
        base.SetToAttacking_Ranged();
    }

    virtual public void GoToLocation(Vector3 destination) {
        navMeshAgent.SetDestination(destination);
    }

    virtual public Vector3 GetAmbushLocation() {
        return ambushLocation.position;
    }

    virtual public Vector3 GetAmbushDestination() {
        return ambushDest.position;
    }

    virtual public void AmbushComplete() {
        SetToIdle();
    }
}
