using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Orange_AI : Base_AI
{
    /// <summary>
    /// Orange AI or follower AI is an Ai that followers a Leader that can eb an AI or the player, it follows 2 orders:
    ///     Order Attack -> If the Ai is following the Leader it will listen to this command and charge at a target or
    /// in a a direction when commanded by the leader, the ai will fight any/all enemies at the location and return once it has been idle for a moment
    ///     Order Recall ->  If the AI is not following the leader it will listen to this command and return to its leader
    ///     
    /// The Patrolling State is overriden & treated insead like a Follower State
    /// </summary>
    [SerializeField] bool playerFollower;
    [SerializeField] Leader leader;
    [SerializeField] float runDistance;
    [SerializeField] MeshRenderer ballHead;
    [SerializeField] Material playerColourMat;

    protected override void Start() {
        base.Start();
        transform.parent = null;

        if(leader != null) {
            if (!leader.AddFollower(this)) {
                Debug.Log("Leader: " + leader.name + " is at max followers");
                Destroy(gameObject);
            }
            else {
                Debug.Log("Added: " + name + " to " + leader.name +" followers");
                ai_State = AI_State.Patrolling;
            }
        }
    }

    public void SetPlayerFollower(bool t) { //Called before start
        playerFollower = t;
        ballHead.material = playerColourMat;
        leader = (Leader) FindObjectOfType<PlayerLeader>();
    }

    protected override void SetToPatrolling() {
        if(leader != null) {
            ai_State = AI_State.Patrolling;
            animator.SetBool("Punching", false);
            baseTarget = null;
        }
        else {
            animator.SetBool("Walking", true);
            animator.SetBool("Running", false);
            navMeshAgent.speed = walkingSpeed;
            Transform p = GameObject.FindGameObjectWithTag("FleePoints").transform;
            navMeshAgent.SetDestination(p.GetChild(Random.Range(0, p.childCount)).position);
        }
    }

    protected override void Patrolling() {//The leader will provide the positions where the Orange ai shoudl stand
        if(leader != null) {
            float distance = Helpers.Vector3Distance(navMeshAgent.destination, transform.position);
            if (distance <= orderComlpletion) {
                animator.SetBool("Walking", false);
                animator.SetBool("Running", false);
                navMeshAgent.speed = 0;
            }
            else if (distance > runDistance) {//Run if far from the leader
                animator.SetBool("Walking", true);
                animator.SetBool("Running", true);
                navMeshAgent.speed = runningSpeed;
            }
            else {//Walk if close to the destination
                animator.SetBool("Walking", true);
                animator.SetBool("Running", false);
                navMeshAgent.speed = walkingSpeed;
            }
        }
        else {
            if (Helpers.Vector3Distance(transform.position, navMeshAgent.destination)  <= orderComlpletion) {

                SetToPatrolling();
            }
        }
    }

    public bool isPlayerFollower() {
        return playerFollower;
    }

    public bool isFollowing() {
        if (ai_State == AI_State.Patrolling) {
            return true;
        }
        return false;
    }

    public bool isFollowing(Vector3 pos) { 
        if (ai_State == AI_State.Patrolling) {
            navMeshAgent.SetDestination(pos);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Override changes to detect player, ai's with Leader scripts & other Orange_Minions
    /// </summary>
    /// <param name="visibleObjects"></param>
    protected override void ObjectsDetected(List<Transform> visibleObjects) {
        if(leader != null) {
            if (ai_State == AI_State.Patrolling)
                return; //Will not fight enemies if it is following its leader, waits for a command instead
        }
        foreach (Transform obj in visibleObjects) {
            if (obj == null)
                continue;

            if (obj.TryGetComponent(out Orange_AI oai)) {
                if(oai.isPlayerFollower() != playerFollower) {
                    Debug.Log("Seeking Enemy Orange AI");
                    if (ai_State != AI_State.Attacking)
                        SetToSeeking(obj);
                    sightInvest = true;
                    return;
                }
            }
            else if(playerFollower && obj.TryGetComponent(out AI_Leader ail)){
                Debug.Log("Seeking Enemy Leader");
                if (ai_State != AI_State.Attacking)
                    SetToSeeking(obj);
                sightInvest = true;
                return;
            }
            else if (!playerFollower && obj.TryGetComponent(out PlayerLeader pL)) {
                Debug.Log("Seeking Player Leader");
                if (ai_State != AI_State.Attacking)
                    SetToSeeking(obj);
                sightInvest = true;
                return;
            }
            else if (playerFollower && obj.TryGetComponent(out Base_AI bai)) {
                Debug.Log("Seeking Enemy ai");
                if (ai_State != AI_State.Attacking)
                    SetToSeeking(obj);
                sightInvest = true;
                return;
            }
        }
        if (baseTarget != null) {
            baseTargetPosition = baseTarget.position;
            Debug.Log("Lost Target");
        }
        baseTarget = null;
    }

    override protected void OnTriggerEnter(Collider other) {
        bool validTarget = false;
        if (other.TryGetComponent(out Orange_AI oai)) {
            if (oai.isPlayerFollower() != playerFollower) {
                validTarget = true;
            }
        }
        else if (playerFollower && other.TryGetComponent(out AI_Leader ail)) {
            validTarget = true;
        }
        else if (!playerFollower && other.TryGetComponent(out PlayerLeader pL)) {
            validTarget = true;
        }
        else if (playerFollower && other.TryGetComponent(out Base_AI bai)) {
            validTarget = true;
        }

        if (validTarget && other.TryGetComponent(out Health hp)) {
            hp.HealthChange(-mAttackDamage);
        }
    }

    protected override void Investigating() {
        if (Helpers.Vector3Distance(transform.position, baseTargetPosition) <= 3f) {
            SetToPatrolling();
            sightInvest = false;
            soundInvest = false;
            hitInvest = false;
        }
    }

    protected override void SetToInvestigating() {
        if (ai_State == AI_State.Patrolling)
            return;
        base.SetToInvestigating();
    }

    protected override void Die() {
        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
        animator.SetBool("Shooting", false);
        animator.SetBool("Punching", false);
        base.Die();
        if(leader != null)
            leader.FollowerDeath(this);
    }

    public bool CommandedForward(Transform target) {
        if (ai_State != AI_State.Patrolling)
            return false;
        baseTarget = target;
        baseTargetPosition = target.position;
        ai_State = AI_State.Investigating;
        SetToInvestigating();
        return true;
    }

    public bool CommandedRecall() {
        if (ai_State == AI_State.Patrolling)
            return false;
        SetToPatrolling();
        return true;
    }

    protected override void SetToIdle() {
        SetToPatrolling();
    }

    public void WipeLeader() {
        leader = null;
    }
}