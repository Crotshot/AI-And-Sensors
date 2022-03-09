using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Yellow_AI : Green_AI
{
    protected bool desiresHealth;
    Transform player;

    protected override void ObjectsDetected(List<Transform> visibleObjects) {

        foreach (Transform obj in visibleObjects) {
            if (desiresHealth) {
                if (obj.TryGetComponent(out PickUp pickUp)) {
                    if (pickUp.pickUpType == PickUp.PickUpType.Health) { //AI will go to a health pack if it sees one
                        navMeshAgent.SetDestination(obj.position);
                    }
                }
            }
            else if(player == null) {
                if (obj.tag.Equals("Player")) {
                    if (ai_State != AI_State.Attacking)
                        SetToSeeking(obj);
                    sightInvest = true;
                    player = obj;
                    baseTarget = player;
                    return;
                }
            }
        }

        base.ObjectsDetected(visibleObjects);
    }

    protected override void Patrolling() {
        if (player == null)
            base.Patrolling();
        else
            SetToSeeking(player);
    }

    override protected bool FleeCheck() { //Checks if ai wants health or ammo, yellow ai will only flee for health if it is also low on ammo but sill still pick up health packs in los
        if (health.GetHeatlthPercent() < lowHealthThreshold) {
            if(weap == null) {
                if (!wantsHealth) {//AI had reosurces that are now gone so it shall fully reset its fleeing ability
                    fleeTimer = maxFleeTime + Random.Range(-maxFleeTimeDeviation, maxFleeTimeDeviation);//Only called once everytime it freshly needs resources
                }
                wantsHealth = true;
                return true;
            }
            else {
                if (weap.GetAmmoPercent() < lowAmmoThreshold) {
                    if (!wantsAmmo) {
                        fleeTimer = maxFleeTime + Random.Range(-maxFleeTimeDeviation, maxFleeTimeDeviation);
                    }
                    wantsAmmo = true;
                    wantsHealth = true;
                    return true;
                }
                else{
                    desiresHealth = true;
                }
            }
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

    override protected void Seeking() {
        if (desiresHealth) {//AI saw and walked to a health pack
            if(Helpers.Vector3Distance(transform.position, navMeshAgent.destination) <= orderComlpletion) {
                desiresHealth = false;
            }
            return;
        }

        if (Helpers.Vector3Distance(transform.position, player.position) <= (combat_Type == Combat_Type.Melee ? mAttackDist : rAttackDist)) {
            SetToAttacking();
        }
        else {
            navMeshAgent.SetDestination(player.position);
        }
    }


    override protected void SetToInvestigating() {
        if(player == null) {
            base.SetToInvestigating();
        }
        else {
            baseTarget = player;
            SetToSeeking(player);
        }
    }


    override protected void SetToAttacking_Ranged() {
        baseTarget = player;
        base.SetToAttacking_Ranged();
    }

    override protected void SetToAttacking_Melee() {
        baseTarget = player;
        base.SetToAttacking_Melee();
    }
}