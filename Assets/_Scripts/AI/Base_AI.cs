using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Base_AI : MonoBehaviour
{
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] Weapon weap;
    [SerializeField] float aiIdleTime, aiIdleTimeDeviation, stunTime, orderComlpletion;
    #region Non Serialized
    Vector3 targetPosition;
    Transform target;
    Animator animator;
    CapsuleCollider capsuleCollider;
    NavMeshAgent navMeshAgent;
    Health health;
    NoiseListener noiseListener;
    /// <summary>
    /// Idle -> The unit is standing around doing nothing
    /// Patrolling -> The unit is moving form one point to another
    /// Wandering -> The unit is wandering aimlessly
    /// Investigating -> The unit is investigating an area where it detected the player 
    /// Seeking -> The unit knows where the player is and is hunting them
    /// Attacking -> The unit is within range and is attempting to harm the player
    /// Fleeing -> The unit is fleeing from combat & seeking ammo or health
    /// Searching -> The unit is seeking ammo and health
    /// </summary>
    private enum AI_State { Idle, Patrolling, Wandering, Investigating, Seeking, Attacking, Fleeing, Searching}
    private enum Combat_Type {Melee, Ranged}
    AI_State ai_State;
    Combat_Type combat_Type;
    float timer, stunTimer;
    int currentPoint;
    bool dead;
    #endregion
    private void Start() {
        combat_Type = weap == null ? Combat_Type.Melee : Combat_Type.Ranged;
        ai_State = AI_State.Idle;

        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        health.death.AddListener(Die);
        health.damaged.AddListener(Damaged);
        noiseListener = GetComponent<NoiseListener>();

        SetToIdle();
    }

    void Reset_Unit() {
        capsuleCollider.enabled = true;
        health.enabled = true;

        noiseListener.enabled = true;

        //Position then enable 
        navMeshAgent.enabled = true;
        dead = false;
        SetToIdle();
    }

    private void Update() {
        if (Time.timeScale == 0 || dead)
            return;
        if(stunTimer > 0) {
            stunTimer -= Time.deltaTime;
            if(stunTimer <= 0) {
                navMeshAgent.SetDestination(targetPosition);
            }
            return;
        }
        switch (ai_State) {
            case AI_State.Idle:
                Idle();
                break;
            case AI_State.Patrolling:
                Patrolling();
                break;
            case AI_State.Wandering:
                Wandering();
                break;
            case AI_State.Investigating:
                Investigating();
                break;
            case AI_State.Seeking:
                Seeking();
                break;
            case AI_State.Attacking:
                Debug.Log("");
                break;
            case AI_State.Fleeing:
                Debug.Log("");
                break;
            case AI_State.Searching:
                Debug.Log("");
                break;
        }
    }

    #region Overidables
    protected void Idle() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            return;
        }
        SetToPatrolling();
    }
    protected void Patrolling() {
        //Checking distance to next point
        CalculateNextPoint();
    }
    protected void Wandering() {

    }
    protected void Investigating() {

    }
    protected void Seeking() {

    }
    protected void Attacking() {

    }
    protected void Fleeing() {

    }
    protected void Searching() {

    }
    protected void SetToIdle() {
        ai_State = AI_State.Idle;
        timer = aiIdleTime + Random.Range(-aiIdleTimeDeviation, aiIdleTimeDeviation);
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
    }
    protected void SetToPatrolling() {
        if (patrolPoints.Length == 0) {
            SetToIdle();
            return;
        }
        ai_State = AI_State.Patrolling;
        navMeshAgent.SetDestination(patrolPoints[currentPoint].position);
        CalculateNextPoint();
        animator.SetBool("Walking", true);
    }
    protected void CalculateNextPoint() {//Checks distance to current control point and sets next if close enough
        if (Helpers.Vector3Distance(transform.position, patrolPoints[currentPoint].position) <= orderComlpletion) {
            currentPoint = (currentPoint == patrolPoints.Length - 1) ? 0 : currentPoint+1;
            navMeshAgent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
    protected void Damaged() {
        if(health.GetHeatlthPercent() != 0) {
            animator.SetTrigger("React");
            stunTimer = stunTime;
            targetPosition = navMeshAgent.destination;
            navMeshAgent.SetDestination(transform.position);
        }
    }
    protected void Die() {
        animator.SetTrigger("Death");
        capsuleCollider.enabled = false;
        navMeshAgent.enabled = false;
        health.enabled = false;
        noiseListener.enabled = false;
    }
    #endregion
}
