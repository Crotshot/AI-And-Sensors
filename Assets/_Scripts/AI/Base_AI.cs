using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Base_AI : MonoBehaviour
{
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float aiIdleTime, aiIdleTimeDeviation, stunTime, orderComlpletion, walkingSpeed, runningSpeed;
    #region Non Serialized
    protected Vector3 baseTargetPosition;
    protected Transform baseTarget;
    protected Animator animator;
    protected CapsuleCollider capsuleCollider;
    protected NavMeshAgent navMeshAgent;
    protected Health health;

    #region Sensors
    protected NoiseListener noiseListener;
    protected AI_Smell ai_Smell;
    protected AI_Vision ai_Vision;
    #endregion

    #region Ranged
    [SerializeField] protected Weapon weap;
    [SerializeField] protected Transform weaponPoint;
    [SerializeField] protected float rAttackTime, rShotTime, rAttackDist;//attackTime is full length of cycle, shotTime is how long into the cycle until it shoots
    protected float rAttackTimer;
    protected bool shot;
    #endregion

    #region Melee
    protected BoxCollider fistBox;
    //hitBoxTime is how far into the punch until the hit box appears and hitBox Linger is hot long the box stays, ranged attacks shoot immediately
    [SerializeField] protected float mAttackTime, hitBoxTime, hitBoxLinger, mAttackDamage, mAttackDist;
    protected float mAttackTimer;
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
    public enum AI_State { Idle, Patrolling, Wandering, Investigating, Seeking, Attacking, Fleeing, Searching, Ambushing}
    public enum Combat_Type {Melee, Ranged}
    protected AI_State ai_State;
    protected Combat_Type combat_Type;
    protected float timer, stunTimer, speedBeforeStun = -1f;
    protected int currentPoint;
    protected bool dead, soundInvest, sightInvest, hitInvest;
    #endregion
    #region Overidables
    virtual protected void Start() {
        combat_Type = weap == null ? Combat_Type.Melee : Combat_Type.Ranged;
        ai_State = AI_State.Idle;

        if(combat_Type == Combat_Type.Melee) {
            fistBox = GetComponent<BoxCollider>();
        }
        else {
            weap.transform.parent = null;
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

    virtual protected void Reset_Unit() {
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

    virtual protected void Update() {
        if (Time.timeScale == 0 || dead)
            return;
        Debug.DrawRay(transform.position,transform.forward,Color.blue);
        if(stunTimer > 0) {
            stunTimer -= Time.deltaTime;
            if(stunTimer <= 0) {
                navMeshAgent.SetDestination(baseTargetPosition);
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
                Attacking();
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

    
    virtual protected void Idle() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            return;
        }
        SetToPatrolling();
    }
    virtual protected void Patrolling() {
        //Checking distance to next point
        CalculateNextPoint();
    }
    virtual protected void Wandering() {

    }
    virtual protected void Investigating() {
        if(Helpers.Vector3Distance(transform.position, baseTargetPosition) <= 3f) {
            SetToIdle();
            sightInvest = false;
            soundInvest = false;
            hitInvest = false;
        }
    }
    virtual protected void Seeking() {
        if(baseTarget == null) {
            SetToInvestigating();
            return;
        }
        if (Helpers.Vector3Distance(transform.position, baseTarget.position) <= (combat_Type == Combat_Type.Melee ? mAttackDist : rAttackDist)) {
            SetToAttacking();
        }
        else {
            navMeshAgent.SetDestination(baseTarget.position);
        }
    }

    virtual protected void Attacking() {
        if (combat_Type == Combat_Type.Melee) {
            AttackingMelee();
        }
        else {
            AttackingRanged();
        }
    }
    virtual protected void AttackingMelee() {
        if (baseTarget == null) {
            //Debug.Log("Lost baseTarget mid melee attack");
            animator.SetBool("Punching", false);
            SetToInvestigating();
            return;
        }
        if (Helpers.Vector3Distance(transform.position, baseTarget.position) > mAttackDist * 1.1f) {
           // Debug.Log("Too far, getting closer to melee!");
            animator.SetBool("Punching", false);
            SetToSeeking(baseTarget);
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

        transform.LookAt(baseTarget);
    }

    virtual protected void AttackingRanged() {
        if (baseTarget == null) {
            Debug.Log("Lost baseTarget  while shooting");
            animator.SetBool("Shooting", false);
            SetToInvestigating();
            return;
        }
        if (Helpers.Vector3Distance(transform.position, baseTarget.position) > rAttackDist * 1.4f) {
            Debug.Log("Too far, getting closer to shoot!");
            animator.SetBool("Shooting", false);
            SetToSeeking(baseTarget);
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

        transform.LookAt(baseTarget);
    }

    virtual protected void Fleeing() {

    }
    virtual protected void Searching() {

    }
    virtual protected void Ambushing() {

    }

    virtual protected void SetToIdle() {
        ai_State = AI_State.Idle;
        timer = aiIdleTime + Random.Range(-aiIdleTimeDeviation, aiIdleTimeDeviation);
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
    }
    virtual protected void SetToPatrolling() {
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
    virtual protected void CalculateNextPoint() {//Checks distance to current control point and sets next if close enough
        if (Helpers.Vector3Distance(transform.position, patrolPoints[currentPoint].position) <= orderComlpletion) {
            currentPoint = (currentPoint == patrolPoints.Length - 1) ? 0 : currentPoint+1;
            navMeshAgent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
    virtual protected void SetToSeeking(Transform targ) {
        ai_State = AI_State.Seeking;
        baseTarget = targ;
        navMeshAgent.SetDestination(targ.position);
        animator.SetBool("Running", true);
        navMeshAgent.speed = runningSpeed;
    }
    virtual protected void SetToInvestigating() {
        ai_State = AI_State.Investigating;
        navMeshAgent.SetDestination(baseTargetPosition);
        animator.SetBool("Running", true);
        navMeshAgent.speed = runningSpeed;
    }

    virtual protected void SetToAttacking() {
        if (combat_Type == Combat_Type.Ranged) {
            SetToAttacking_Ranged();
        }
        else {
            SetToAttacking_Melee();
        }
    }
    //Duplicate code between melee and ranged as they are called directly instead of SetToAttacking in some locals
    virtual protected void SetToAttacking_Melee() {
        ai_State = AI_State.Attacking;
        navMeshAgent.SetDestination(transform.position);
        animator.SetBool("Punching", true);
        animator.SetBool("Walking", false);
        mAttackTimer = mAttackTime;
    }

    virtual protected void SetToAttacking_Ranged() {
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
        weaponPoint.transform.LookAt(baseTarget.position + Vector3.up);
    }

    virtual protected void Die() {
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
    virtual protected void ObjectsDetected(List<Transform> visibleObjects) {
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
        if(baseTarget != null) {
            baseTargetPosition = baseTarget.position;
            Debug.Log("Lost Player");
        }
        baseTarget = null;
    }
    virtual protected void Damaged(Vector3 damageOrigin) {
        if (health.GetHeatlthPercent() != 0) {
            hitInvest = true;
            if (sightInvest) {
                baseTargetPosition = navMeshAgent.destination;
            }
            else {
                Debug.DrawRay(damageOrigin, Vector3.up * 10f, Color.red, 10f);
                baseTargetPosition = damageOrigin;
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
    virtual protected void SoundDetected(Vector3 calculatedOrigin) {
        if(baseTarget == null && !sightInvest && !hitInvest) {
            baseTargetPosition = calculatedOrigin;
            SetToInvestigating();
        }
    }
    virtual protected void SmellDetected(Vector3 calculatedDirection) {
        if (baseTarget == null && !sightInvest && !soundInvest && !hitInvest) {
            Ray ray = new Ray(transform.position, calculatedDirection);
            Debug.DrawRay(transform.position, calculatedDirection * 20f, Color.red, 5f);
            //baseTargetPosition = transform.TransformPoint(ray.GetPoint(20f));
            baseTargetPosition = ray.GetPoint(ai_Smell.GetTravelDistance());
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

    public void AssignControlPoints(Transform[] transforms) {
        patrolPoints = transforms;
    }

    public Weapon GetWeapon() {
        return weap;
    }
}
