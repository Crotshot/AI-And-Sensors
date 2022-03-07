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
    [SerializeField][Range(0,1)] float lowHealthThreshold, lowAmmoThreshold;
    [SerializeField] [Min(0)] float maxFleeTime, maxFleeTimeDeviation, shortFleeTime, shortFleeTimeDeviation;//Max is total fleetime to count towards becoming mroe aggressive and short fleetime is for each individual time it flees
    float fleeTimer, shortFleeTimer;
    bool wantsAmmo, wantsHealth;

    override protected void Start() {
        base.Start();
        ammoPacks = new List<Transform>();
        healthPacks = new List<Transform>();
    }

    override protected void CalculateNextPoint() {//Checks distance to current control point and sets it to random
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
                        marker.transform.position = pickUp.transform.position;
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
                        marker.transform.position = pickUp.transform.position;
                        marker.AddComponent<MarkerOwner>().markerOwner = GetInstanceID();
                        Debug.Log("Remembering Health Marker");
                        healthPacks.Add(marker.transform);
                    }
                }
                return;//No need to call base if it is known not to be a player already
            }
            else if (obj.TryGetComponent(out MarkerOwner marker)) { //Look at one of our markers to see if the corresponding pack is still persistent, if no, destroy it
                if(marker.markerOwner == GetInstanceID()) {
                    bool foundPack = false;
                    foreach (Collider coll in Physics.OverlapSphere(marker.transform.position, 2.5f, packLayer, QueryTriggerInteraction.Collide)) {//Only queries packs so it is very unlikey to detect more than one
                        if (coll.transform == marker.transform) {
                            Debug.Log("Marker scanned itself lol");
                            continue;
                        }
                        Debug.Log("NGL kinda sus");
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
                return;//No need to call base if it is known not to be a player already
            }
        }
        base.ObjectsDetected(visibleObjects); //Base only detects players
    }



    protected override void Fleeing() {
        //Flee timer decreases
        fleeTimer -= Time.deltaTime;
        shortFleeTimer -= Time.deltaTime;

        if (shortFleeTimer <= 0) {
            SetToWandering();
        }
        if (fleeTimer <= 0) {
            //aggression
        }
        if (target == null)
            SetToFleeing();
    }

    virtual protected void SetToWandering() {
        //Pick random point nearby and set to AI destination
    }

    protected override void Wandering() {
        //if within destination piont make a new one
        if(Helpers.Vector3Distance(navMeshAgent.destination, transform.position) <= orderComlpletion) {
            SetToWandering();
        }
    }

    virtual protected void SetToFleeing() {
        if (wantsHealth) {
            target = Helpers.FindClosestTransform(ammoPacks, transform.position);
            if(target != null) {
                navMeshAgent.SetDestination(target.position);
            }
        }
        else if (wantsAmmo) {
            target = Helpers.FindClosestTransform(healthPacks, transform.position);
            if (target != null) {
                navMeshAgent.SetDestination(target.position);
            }
        }
        
    }

    virtual protected void Spook() { //Spook is called after hearing/seeing the player whenever the AI is below hp/ammo thresh or is fleeing

    }

    protected override void Investigating() {
        if(health.GetHeatlthPercent() < lowHealthThreshold) {
            if (fleeTimer <= 0)
                transform.LookAt(targetPosition);////////////////////////////////////PICK UP HERE
        }
        else if(weap != null) {
            if (weap.GetAmmoPercent() < lowAmmoThreshold) {
                if(fleeTimer <= 0)
                    transform.LookAt(targetPosition);
            }
        }

        base.Investigating();
    }
}