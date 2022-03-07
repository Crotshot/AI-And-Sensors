using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class Blue_AI : Base_AI {
    protected override void SetToInvestigating() {
        transform.LookAt(targetPosition);
        SetToIdle();//Dont check for area as this is the Blue AI and go back to patrolling/idling
        sightInvest = false;
        soundInvest = false;
        hitInvest = false;
    }
}