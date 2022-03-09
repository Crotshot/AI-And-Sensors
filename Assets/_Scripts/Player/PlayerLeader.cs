using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLeader : Leader {

    Transform target;
    Inputs inputs;

    private void Start() {
        inputs = FindObjectOfType<Inputs>();
    }

    protected override void Update() {
        base.Update();

        if (inputs.GetCommandRecallHeld()) {
            Debug.Log("Attempting to Recall");
            Recall();
        }
        if (target != null) {
            if (inputs.GetCommandAttackHeld()) {
                Debug.Log("Attempting to Attack");
                Attack(target);
            }
        }

        target = null;
    }


    public void SetTarget(Transform targ) {
        target = targ;
    }
}