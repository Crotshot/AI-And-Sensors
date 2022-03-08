using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Green_AI : Base_AI {
    /// <summary>
    /// Green AI remembers where health and ammo packs are, stored as Vector3 so if a pack is picke dup it does not immediately knwo that it is gone
    /// To implement:
    ///     -> Find ammo pack
    ///     ->Remeber its vector3
    ///     ->When needing ammo pack goes to closest one
    ///     ->When AI can see ammo pack position loop though all ammo packs in scene and find if there is one within x units of it
    ///     ->Pick up pack
    /// </summary>
    protected List<Transform> ammoPacks, healthPacks;
    [SerializeField] protected LayerMask packLayer;
    /// <summary>
    /// What percentage of health the ai must be at in order to seek out a health pack
    ///     ->Will flee from player to health pack if beneath threshold
    ///     ->If no health packs are available it will simply just flee instead
    ///     ->AI keeps track of player through sound, sight, smell & damage, if it can see/hear the player it will run away from the general direction of the player
    ///     ->When player is not bothering AI it will enter wandering mode until it finds a health pack to heal itself on
    ///     ->This goes the same for ammo
    ///     
    ///     ->If AI has been fleeing from player for longer than its flee timer when it hears the player it will turn to attack for brief moment and then continue fleeing
    ///     ->If it has not ammmo at all if will not hold its ground ever and keep fleeing/wandering till it finds ammo
    ///     
    ///     ->To Calculate FleePoint the ai will use the last known location to the player and check which flee point it is closer to than the player
    ///     ->After reaching the first flee point if understurbed it will start to wander from fleepoint to fleepoint until it finds enough health/ ammo to resume patrolling
    /// </summary>
    [SerializeField][Range(0,1)] protected float lowHealthThreshold, lowAmmoThreshold;
    [SerializeField] [Min(0)] protected float maxFleeTime, maxFleeTimeDeviation, shortFleeTime, shortFleeTimeDeviation, groundTime, groundTimeDeviation;//Max is total fleetime to count towards becoming mroe aggressive and short fleetime is for each individual time it flees
    [SerializeField] [Min(0)] protected float minFleeDistance;
    protected float fleeTimer, shortFleeTimer, groundTimer;
    protected bool wantsAmmo, wantsHealth;

    protected Transform packTarget;
    protected Vector3 packTargetPosition;

    protected override void Start() {
        base.Start();
        ammoPacks = new List<Transform>();
        healthPacks = new List<Transform>();
    }

    protected override void Update() {
        if (Time.timeScale == 0 || dead)
            return;
        Debug.Log(ai_State);

        if(groundTimer > 0) {
            groundTimer -= Time.deltaTime;
            if(groundTimer <= 0)
                fleeTimer = maxFleeTime + Random.Range(-maxFleeTimeDeviation, maxFleeTimeDeviation);
        }
        base.Update();
    }

    protected override void CalculateNextPoint() {//Checks distance to current control point and sets it to random
        float distance = Helpers.Vector3Distance(transform.position, patrolPoints[currentPoint].position);
        if (distance <= orderComlpletion) {
            currentPoint = Random.Range(0, patrolPoints.Length);
            navMeshAgent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    protected override void ObjectsDetected(List<Transform> visibleObjects) {
        foreach (Transform obj in visibleObjects) {
            if (obj.TryGetComponent(out PickUp pickUp)) {
                if (pickUp.pickUpType == PickUp.PickUpType.Ammo) {
                    bool alreadyScanned = false;
                    foreach (Transform point in ammoPacks) {
                        if(Helpers.Vector3Distance(point.position, pickUp.transform.position) <= 2.5f) {//Dont add ammo pack as it is already in list if it is within 2.5 units
                            alreadyScanned = true;
                        }
                    }
                    if (!alreadyScanned) {//This is done so that we dont add the same health pack/ammo pack multiple times to the ammo/health pack lists
                        GameObject marker;
                        if(obj.transform.localScale.x == 1) 
                           marker = new GameObject("Large_Ammo_Marker");//AI can distinguish between large and small ammo packs
                        else
                            marker = new GameObject("Small_Ammo_Marker");
                        marker.transform.position = new Vector3(pickUp.transform.position.x,0, pickUp.transform.position.z);
                        marker.AddComponent<MarkerOwner>().markerOwner = GetInstanceID();
                        Debug.Log("Remembering Ammo Marker");
                        ammoPacks.Add(marker.transform);
                    }
                }
                else {
                    bool alreadyScanned = false;
                    foreach (Transform point in healthPacks) {
                        if (Helpers.Vector3Distance(point.position, pickUp.transform.position) <= 2.5f) {//Dont add health pack as it is already in list if it is within 2.5 units
                            alreadyScanned = true;
                        }
                    }
                    if (!alreadyScanned) {
                        GameObject marker;
                        if (obj.transform.localScale.x == 1)
                            marker = new GameObject("Large_Health_Marker");//AI can distinguish between large and small health packs
                        else
                            marker = new GameObject("Small_Health_Marker");
                        marker.transform.position = new Vector3(pickUp.transform.position.x, 0, pickUp.transform.position.z);
                        marker.AddComponent<MarkerOwner>().markerOwner = GetInstanceID();
                        Debug.Log("Remembering Health Marker");
                        healthPacks.Add(marker.transform);
                    }
                }
            }
            else if (obj.TryGetComponent(out MarkerOwner marker)) { //Look at one of our markers to see if the corresponding pack is still persistent, if no, destroy it
                if(marker.markerOwner == GetInstanceID()) {
                    bool foundPack = false;
                    foreach (Collider coll in Physics.OverlapSphere(marker.transform.position, 2.5f, packLayer, QueryTriggerInteraction.Collide)) {//Only queries packs so it is very unlikey to detect more than one
                        if (coll.transform == marker.transform) {
                            continue; //Marker scans istelf also
                        }
                        foundPack = true;
                        break;
                    }
                    if (!foundPack) {
                        if (marker.name.Contains("Ammo"))
                            ammoPacks.Remove(marker.transform);
                        else
                            healthPacks.Remove(marker.transform);
                        Debug.Log("Removing Marker");
                        Destroy(marker.gameObject);//If ammo/health pack is gone destroy the marker
                    }
                }
            }
        }

        base.ObjectsDetected(visibleObjects); //Base only detects players, changes baseTarget to player if player is visible
    }

    protected override void Fleeing() {
        sightInvest = false;
        soundInvest = false; //Ai still will listen to sensor information 
        hitInvest = false;
        //Flee timer decreases
        fleeTimer -= Time.deltaTime;
        shortFleeTimer -= Time.deltaTime;

        if (shortFleeTimer <= 0) {
            SetToWandering();
        }
        if (Helpers.Vector3Distance(navMeshAgent.destination, transform.position) <= orderComlpletion) //Ammo pack/health pack is pickup, will immediately go to a desired pack if it sees one
            if (FleeCheck()) {
                Debug.Log("Fleeing");
                SetToFleeing();//AI will pick another random point away from the last player target position and go there.
            }
            else {
                SetToPatrolling();
            }
    }

    protected virtual void SetToWandering() {
        animator.SetBool("Running", false);
        animator.SetBool("Walking", true);
        navMeshAgent.speed = walkingSpeed;
        ai_State = AI_State.Wandering;
        if (Helpers.Vector3Distance(navMeshAgent.destination, transform.position) <= orderComlpletion) {
            if (FleeCheck()) {
                if (wantsHealth)
                    packTarget = Helpers.FindClosestTransform(healthPacks, transform.position);
                else
                    packTarget = Helpers.FindClosestTransform(ammoPacks, transform.position);

                if (packTarget != null) {
                    packTargetPosition = packTarget.position;
                    navMeshAgent.SetDestination(packTargetPosition);
                }
                else {
                    //WanderToRandomPoint(); //While wander the AI does not care where the last known location of the player was
                    SetToRandomPoint();
                    navMeshAgent.SetDestination(baseTarget.position);
                    return;
                }
            }
            else {
                SetToIdle();
            }
        }
    }

    protected override void Wandering() {
        //if within destination piont make a new one
        if(Helpers.Vector3Distance(navMeshAgent.destination, transform.position) <= orderComlpletion) {
            SetToWandering();
        }
        else {//Cannot get here without wanting health or ammo so no need to Flee Check here
            if (wantsHealth)
                packTarget = Helpers.FindClosestTransform(healthPacks, transform.position);
            else
                packTarget = Helpers.FindClosestTransform(ammoPacks, transform.position);

            if (packTarget != null) {
                animator.SetBool("Running", true);
                animator.SetBool("Walking", true);
                navMeshAgent.speed = runningSpeed;
                packTargetPosition = packTarget.position;
                navMeshAgent.SetDestination(packTargetPosition);
            }
        }
    }
    /// <summary>
    /// Returns boolean of success if it goes to flee else it will stay
    /// </summary>
    /// <returns></returns>
    protected virtual bool SetToFleeing() {
        navMeshAgent.speed = runningSpeed;
        shortFleeTimer = shortFleeTime + Random.Range(-shortFleeTimeDeviation, shortFleeTimeDeviation);
        if (fleeTimer <= 0 && weap.GetAmmoPercent() > 0) {
            //AI has been fleeing too much so it will prevent itself from fleeing for a moment so it can take a shot or 2 at the player
            if(groundTimer == 0) {
                groundTimer = groundTime + Random.Range(-groundTimeDeviation, groundTimeDeviation);
                SetToInvestigating();
            }
            return false;
        }

        ai_State = AI_State.Fleeing;
        animator.SetBool("Running", true);
        animator.SetBool("Walking", true);

        if (wantsHealth)
            packTarget = Helpers.FindClosestTransform(healthPacks, transform.position);
        else
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

    protected override void SetToAttacking_Ranged() {
        if ((weap.GetAmmoPercent() <= lowAmmoThreshold && groundTime <= 0) || weap.GetAmmoPercent() == 0) {
            animator.SetBool("Shooting", false);
            if (!wantsAmmo) {
                fleeTimer = maxFleeTime + Random.Range(-maxFleeTimeDeviation, maxFleeTimeDeviation);
            }
            wantsAmmo = true;
            SetToFleeing();
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

    protected override void SetToSeeking(Transform targ) {
        if (FleeCheck()) {
            if (SetToFleeing() && groundTimer <= 0) {
                baseTarget = targ;
                return;
            }
        }
        base.SetToSeeking(targ);
    }

    protected override void SetToInvestigating() {
        if (FleeCheck()) {
            if (SetToFleeing() && groundTimer <= 0) {
                return;
            }
            else {
                transform.LookAt(baseTargetPosition);
                SetToIdle();
                return;
            }
        }
        base.SetToInvestigating();
    }

    protected override void Idle() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            return;
        }
        if(FleeCheck()) {
            SetToFleeing();
        }
        else {
            SetToPatrolling();
        }
    }

    protected virtual bool FleeCheck() { //Checks if ai wants health or ammo, if it does newly want ammo or health set the fleeTimer
        if (health.GetHeatlthPercent() < lowHealthThreshold) {
            if (!wantsHealth) {//AI had reosurces that are now gone so it shall fully reset its fleeing ability
                fleeTimer = maxFleeTime + Random.Range(-maxFleeTimeDeviation, maxFleeTimeDeviation);//Only called once everytime it freshly needs resources
            }
            wantsHealth = true;
            return true;
        }
        else {
            wantsHealth = false;
        }
        if (weap != null) {
            if (weap.GetAmmoPercent() < lowAmmoThreshold) {
                if (!wantsAmmo) {
                    fleeTimer = maxFleeTime + Random.Range(-maxFleeTimeDeviation, maxFleeTimeDeviation);
                }
                wantsAmmo = true;
                return true;
            }
        }
        else {
            wantsAmmo = false;
        }
        return false;
    }
    /// <summary>
    /// Sets ai baseTarget position to a random point somewhere
    /// </summary>
    protected virtual void SetToRandomPoint() {
        Transform p = GameObject.FindGameObjectWithTag("FleePoints").transform;
        baseTarget = p.GetChild(Random.Range(0, p.childCount));
    }

    //protected void WanderToRandomPoint() {
    //    Transform p = GameObject.FindGameObjectWithTag("FleePoints").transform;
    //    List<Transform> ps = new List<Transform>();
    //    foreach (Transform child in p) {
    //        ps.Add(child);
    //    }
    //    packTargetPosition = p.GetChild(Random.Range(0, p.childCount)).position;
    //}

    protected virtual void SetToPointAwayFrombaseTargetPosition() { //Flee to the point that is the furthest from the player
        Transform p = GameObject.FindGameObjectWithTag("FleePoints").transform;
        List<Transform> ps = new List<Transform>();
        foreach(Transform child in p) {
            ps.Add(child);
        }
        ps = Helpers.FindTransformsOutsideRadius(ps, baseTargetPosition, minFleeDistance);
        navMeshAgent.SetDestination(Helpers.RandomTransform(ps, baseTargetPosition).position);
    }

    protected override void Damaged(Vector3 damageOrigin) {
        base.Damaged(damageOrigin);
        if (FleeCheck()) {
            SetToFleeing();
        }
    }
}