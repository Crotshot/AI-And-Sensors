using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Leader : Leader {
    public void AttackCommand(Transform target) {
        Attack(target);
    }

    public void RecallCommand() {
        Recall();
    }

    public override void FollowerDeath(Orange_AI ai) {
        if(TryGetComponent(out Purple_AI p)){ 
           p.Flee();
        }
        base.FollowerDeath(ai);
    }
}