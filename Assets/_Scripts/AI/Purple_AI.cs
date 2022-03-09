using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purple_AI : Green_AI {

    //Time between ordering minions to attack
    [SerializeField] float orderIntervalTime;
    float orderIntervalTimer;
    AI_Leader leader;

    protected override void Start() {
        leader = GetComponent<AI_Leader>();
        base.Start();
    }

    protected override void ObjectsDetected(List<Transform> visibleObjects) {
        foreach (Transform obj in visibleObjects) {
            if (obj == null)
                continue;

            if (obj.TryGetComponent(out Orange_AI oai)) {
                if (oai.isPlayerFollower()) {
                    Debug.Log("Seeking Enemy Orange AI");
                    if (ai_State != AI_State.Attacking)
                        SetToSeeking(obj);
                    sightInvest = true;
                    return;
                }
            }
        }
        base.ObjectsDetected(visibleObjects);
    }

    protected override void SetToAttacking() {
        orderIntervalTimer = orderIntervalTime;
        base.SetToAttacking();
    }

    protected override void Attacking() {
        if (baseTarget == null)
            SetToInvestigating();
        base.Attacking();
    }

    protected override bool SetToFleeing() {
        orderIntervalTimer = orderIntervalTime;
        return base.SetToFleeing();
    }

    protected override void Fleeing() {
        if (orderIntervalTimer > 0) {
            orderIntervalTimer -= Time.deltaTime;
        }
        else {
            orderIntervalTimer = orderIntervalTime;
            leader.RecallCommand();
        }
        base.Fleeing();
    }

    protected override void Die() {
        animator.SetTrigger("Death");
        animator.SetBool("Walking", false);
        animator.SetBool("Running", false);
        animator.SetBool("Shooting", false);
        animator.SetBool("Punching", false);

        base.Die();
        leader.Death();
    }

    protected override void Update() {
        base.Update();

        if ((ai_State == AI_State.Attacking || ai_State == AI_State.Seeking) && baseTarget != null) {
            if (orderIntervalTimer > 0) {
                orderIntervalTimer -= Time.deltaTime;
            }
            else {
                orderIntervalTimer = orderIntervalTime;
                leader.AttackCommand(baseTarget);
            }
        }
    }
    override protected void OnTriggerEnter(Collider other) {
        bool validTarget = false;
        if (other.TryGetComponent(out Orange_AI oai)) {
            if (oai.isPlayerFollower()) {
                validTarget = true;
            }
        }
        else if (other.tag.Equals("Player")) {
            validTarget = true;
        }

        if (validTarget && other.TryGetComponent(out Health hp)) {
            hp.HealthChange(-mAttackDamage);
        }
    }

    public void Flee() {
        SetToFleeing();
    }
}