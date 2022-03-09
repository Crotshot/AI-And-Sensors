using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour {
    /*
     * 
     * A class dedicated to getting player inputs
     * 
     */
    static float HOLD_TIME = 0.33f;
    float cAttackT, cRecallT;

    public Vector3 GetMovementInput() { return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); }
    public Vector3 GetMousePosition() { return Input.mousePosition; }
    public Vector2 GetMouseMovement() { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }

    public float GetShiftAlt() { return Input.GetAxisRaw("ShiftAlt"); }
    public float GetCtrlTab() { return Input.GetAxisRaw("CtrlTab"); }
    public float GetSpaceEsc() { return Input.GetAxisRaw("SpaceEsc"); }

    public float GetAction_1_Input() { return Input.GetAxisRaw("Action_1"); }
    public float GetInteractInput() { return Input.GetAxisRaw("Interact"); }
    public float GetReloadInput() { return Input.GetAxisRaw("Reload"); }

    public float GetCommandAttackRecallInput() { return Input.GetAxisRaw("Command_AttackRecall"); }

    //Holding down buttons to repeatedly order minions to attack
    public bool GetCommandAttackHeld() {
        if (cAttackT > HOLD_TIME) {
            cAttackT = 0;
            return true;
        }
        return false;
    }
    public bool GetCommandRecallHeld() {
        if (cRecallT > HOLD_TIME) {
            cRecallT = 0;
            return true;
        }
        return false;
    }

    private void FixedUpdate() {
        if (Input.GetAxisRaw("Command_AttackRecall") > 0)
            cAttackT += Time.deltaTime;
        else
            cAttackT = 0;

        if (Input.GetAxisRaw("Command_AttackRecall") < 0)
            cRecallT += Time.deltaTime;
        else
            cRecallT = 0;
    }
}