using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Base_AI : MonoBehaviour
{
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float aiIdleTime, aiIdleTimeDeviation, stunTime, orderComlpletion, walkingSpeed, runningSpeed;
    #region Non Serialized
    Vector3 targetPosition;
    Transform target;
    Animator animator;
    CapsuleCollider capsuleCollider;
    NavMeshAgent navMeshAgent;
    Health health;

    #region Sensors
    NoiseListener noiseListener;
    AI_Smell ai_Smell;
    AI_Vision ai_Vision;
    #endregion

    #region Ranged
    [SerializeField] Weapon weap;
    [SerializeField] Transform weaponPoint;
    [SerializeField] float rAttackTime, rShotTime, rAttackDist;//attackTime is full length of cycle, shotTime is how long into the cycle until it shoots
    float rAttackTimer;
    bool shot;
    #endregion

    #region Melee
    BoxCollider fistBox;
    //hitBoxTime is how far into the punch until the hit box appears and hitBox Linger is hot long the box stays, ranged attacks shoot immediately
    [SerializeField] float mAttackTime, hitBoxTime, hitBoxLinger, mAttackDamage, mAttackDist;
    float mAttackTimer;
    #endregion


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
    private enum AI_State { Idle, Patrolling, Wandering, Investigating, Seeking, Attacking, Fleeing, Searching, Ambushing}
    private enum Combat_Type {Melee, Ranged}
    AI_State ai_State;
    Combat_Type combat_Type;
    float timer, stunTimer, speedBeforeStun = -1f;
    int currentPoint;
    bool dead, soundInvest, sightInvest, hitInvest;
    #endregion

    private void Start() {
        combat_Type = weap == null ? Combat_Type.Melee : Combat_Type.Ranged;
        ai_State = AI_State.Idle;

        if(combat_Type == Combat_Type.Melee) {
            fistBox = GetComponent<BoxCollider>();
        }
        else {
            weap.PickUp(weaponPoint, false);
        }

        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        health.death.AddListener(Die);
        health.damaged.AddListener(Damaged);


        if (TryGetComponent(out NoiseListener ear)) {
            noiseListener = ear;
            noiseListener.soundObeserved.AddListener(SoundDetected);
        }
        if (TryGetComponent(out AI_Smell nose)) {
            ai_Smell = nose;
            ai_Smell.scentDetected.AddListener(SmellDetected);
        }
        if (TryGetComponent(out AI_Vision eyes)) {
            ai_Vision = eyes;
            ai_Vision.objectsObserved.AddListener(ObjectsDetected);
        }

        SetToIdle();
    }

    void Reset_Unit() {
        capsuleCollider.enabled = true;
        health.enabled = true;

        
        if (noiseListener != null) {
            noiseListener.enabled = true;
        }
        if (ai_Smell != null) {
            ai_Smell.enabled = true;
        }
        if (ai_Vision != null) {
            ai_Vision.enabled = true;
        }


        //Position then enable 
        navMeshAgent.enabled = true;
        dead = false;
        SetToIdle();
    }

    private void Update() {
        if (Time.timeScale == 0 || dead)
            return;
        Debug.DrawRay(transform.position,transform.forward,Color.blue);
        if(stunTimer > 0) {
            stunTimer -= Time.deltaTime;
            if(stunTimer <= 0) {
                navMeshAgent.SetDestination(targetPosition);
                navMeshAgent.speed = speedBeforeStun;
                navMeshAgent.isStopped = false;
                speedBeforeStun = -1;
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
                if(combat_Type == Combat_Type.Melee) {
                    AttackingMelee();
                }
                else {
                    AttackingRanged();
                }
                break;
            case AI_State.Fleeing:
                Fleeing();
                break;
            case AI_State.Searching:
                Searching();
                break;
            case AI_State.Ambushing:
                Ambushing();
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
        if(Helpers.Vector3Distance(transform.position, targetPosition) <= 3f) {
            SetToIdle();
            sightInvest = false;
            soundInvest = false;
            hitInvest = false;
        }
    }
    protected void Seeking() {
        if(target == null) {
            SetToInvestigating();
            return;
        }
        if (Helpers.Vector3Distance(transform.position, target.position) <= (combat_Type == Combat_Type.Melee ? mAttackDist : rAttackDist)) {
            SetToAttacking();
        }
        else {
            navMeshAgent.SetDestination(target.position);
        }
    }
    protected void AttackingMelee() {
        if (target == null) {
            //Debug.Log("Lost target mid melee attack");
            animator.SetBool("Punching", false);
            SetToInvestigating();
            return;
        }
        if (Helpers.Vector3Distance(transform.position, target.position) > mAttackDist * 1.1f) {
           // Debug.Log("Too far, getting closer to melee!");
            animator.SetBool("Punching", false);
            SetToSeeking(target);
            return;
        }

        mAttackTimer = mAttackTimer > 0 ? mAttackTimer -= Time.deltaTime : 0;

        if (mAttackTime - mAttackTimer >= hitBoxTime && mAttackTime - mAttackTimer < hitBoxTime + hitBoxLinger)
            fistBox.enabled = true;
        else
            fistBox.enabled = false;

        if (mAttackTime - mAttackTimer >= 1) {
            SetToAttacking_Melee();
           // Debug.Log("Melee Attacking again");
        }

        transform.LookAt(target);
    }

    protected void AttackingRanged() {
        if (target == null) {
            Debug.Log("Lost target  while shooting");
            animator.SetBool("Shooting", false);
            SetToInvestigating();
            return;
        }
        if (Helpers.Vector3Distance(transform.position, target.position) > rAttackDist * 1.4f) {
            Debug.Log("Too far, getting closer to shoot!");
            animator.SetBool("Shooting", false);
            SetToSeeking(target);
            return;
        }

        rAttackTimer = rAttackTimer > 0 ? rAttackTimer -= Time.deltaTime : 0;

        if (rAttackTime - rAttackTimer >= rShotTime && !shot) {
            weap.Shoot();
            shot = true;
        }
        if (rAttackTime - rAttackTimer >= 1) {
            SetToAttacking_Ranged();
            Debug.Log("Shooting again");
        }

        transform.LookAt(target);
    }

    protected void Fleeing() {

    }
    protected void Searching() {

    }
    protected void Ambushing() {

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
        animator.SetBool("Running", false);
        navMeshAgent.speed = walkingSpeed;
    }
    protected void CalculateNextPoint() {//Checks distance to current control point and sets next if close enough
        if (Helpers.Vector3Distance(transform.position, patrolPoints[currentPoint].position) <= orderComlpletion) {
            currentPoint = (currentPoint == patrolPoints.Length - 1) ? 0 : currentPoint+1;
            navMeshAgent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
    protected void SetToSeeking(Transform targ) {
        ai_State = AI_State.Seeking;
        target = targ;
        navMeshAgent.SetDestination(targ.position);
        animator.SetBool("Running", true);
        navMeshAgent.speed = runningSpeed;
    }
    protected void SetToInvestigating() {
        ai_State = AI_State.Investigating;
        navMeshAgent.SetDestination(targetPosition);
        animator.SetBool("Running", true);
        navMeshAgent.speed = runningSpeed;
    }

    protected void SetToAttacking() {
        if (combat_Type == Combat_Type.Ranged) {
            SetToAttacking_Ranged();
        }
        else {
            SetToAttacking_Melee();
        }
    }
    //Duplicate code between melee and ranged as they are called directly instead of SetToAttacking in some locals
    protected void SetToAttacking_Melee() {
        ai_State = AI_State.Attacking;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("Punching", true);
        animator.SetBool("Walking", false);
        mAttackTimer = mAttackTime;
    }

    protected void SetToAttacking_Ranged() {
        if (weap.GetAmmoPercent() == 0) {
            animator.SetBool("Shooting", false);
            SetToIdle();
            return;
        }
        ai_State = AI_State.Attacking;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("Shooting", true);
        animator.SetBool("Walking", false);
        rAttackTimer = rAttackTime;
        shot = false;//Dont shoot immediately to allow animation to transition
        weaponPoint.transform.LookAt(target.position + Vector3.up);
    }

    protected void Die() {
        animator.SetTrigger("Death");
        capsuleCollider.enabled = false;
        navMeshAgent.enabled = false;
        health.enabled = false;
        if (noiseListener != null) {
            noiseListener.enabled = false;
        }
        if (ai_Smell != null) {
            ai_Smell.enabled = false;
        }
        if (ai_Vision != null) {
            ai_Vision.enabled = false;
        }
        dead = true;
    }
    /// <summary>
    /// Base Object detection which can only see the player, when overriding call base last
    /// </summary>
    /// <param name="visibleObjects"></param>
    protected void ObjectsDetected(List<Transform> visibleObjects) {
        //Debug.Log("Objects detected: " + visibleObjects.Count);
        foreach (Transform obj in visibleObjects) {
            if (obj == null)
                continue;

            if (obj.tag.Equals("Player")) {
                Debug.Log("Seeking Player");
                if(ai_State != AI_State.Attacking)
                    SetToSeeking(obj);
                sightInvest = true;
                return;
            }
        }
        if(target != null) {
            targetPosition = target.position;
            Debug.Log("Lost Player");
        }
        target = null;
    }
    protected void Damaged(Vector3 damageOrigin) {
        if (health.GetHeatlthPercent() != 0) {
            hitInvest = true;
            if (sightInvest) {
                targetPosition = navMeshAgent.destination;
            }
            else {
                Debug.DrawRay(damageOrigin, Vector3.up * 10f, Color.red, 10f);
                targetPosition = damageOrigin;
                SetToInvestigating();
            }

            animator.SetTrigger("React");
            stunTimer = stunTime;
            navMeshAgent.SetDestination(transform.position);
            if (speedBeforeStun == -1)
                speedBeforeStun = navMeshAgent.speed;
            navMeshAgent.speed = 0;
            navMeshAgent.isStopped = true;
        }
    }
    protected void SoundDetected(Vector3 calculatedOrigin) {
        if(target == null && !sightInvest && !hitInvest) {
            targetPosition = calculatedOrigin;
            SetToInvestigating();
        }
    }
    protected void SmellDetected(Vector3 calculatedDirection) {
        if (target == null && !sightInvest && !soundInvest && !hitInvest) {
            Ray ray = new Ray(transform.position, calculatedDirection);
            Debug.DrawRay(transform.position, calculatedDirection * 20f, Color.red, 5f);
            //targetPosition = transform.TransformPoint(ray.GetPoint(20f));
            targetPosition = ray.GetPoint(20f);
            SetToInvestigating();
        }
    }
    #endregion

    //attack Trigger 
    private void OnTriggerEnter(Collider other) {
        if (other.tag.Equals("Player") && other.TryGetComponent(out Health hp)) {
            hp.HealthChange(-mAttackDamage);
        }
    }

    public void AssignConmtrolPoints(Transform[] transforms) {
        patrolPoints = transforms;
    }
}
