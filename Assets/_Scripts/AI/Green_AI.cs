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
                        GameObject marker = new GameObject("Ammo_Marker");
                        marker.transform.position = pickUp.transform.position;
                        marker.AddComponent<MarkerOwner>().markerOwner = gameObject.GetInstanceID();
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
                        GameObject marker = new GameObject("Health_Marker");
                        marker.transform.position = pickUp.transform.position;
                        marker.AddComponent<MarkerOwner>().markerOwner = gameObject.GetInstanceID();
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
}