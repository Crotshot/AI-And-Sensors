using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour {
    /*
     * 
     * A class dedicated to getting player inputs
     * 
     */
    static float HOLD_TIME = 0.2f, LONG_HOLD_TIME = 0.65f, SCROLL_TIME = 0.1f;
    float cAttackT, cDefendT, cRecallT, cRetreatT, cRetreatL, cRecallL, scrollT, scrollLimter;
    [SerializeField] float scrollSensitivity = 1f;

    public Vector3 GetMovementInput() { return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); }
    public Vector3 GetMousePosition() { return Input.mousePosition; }
    public Vector2 GetMouseMovement() { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }

    public float GetShiftAlt() { return Input.GetAxisRaw("ShiftAlt"); }
    public float GetCtrlTab() { return Input.GetAxisRaw("CtrlTab"); }
    public float GetSpaceEsc() { return Input.GetAxisRaw("SpaceEsc"); }

    public float GetAction_1_2_Input() { return Input.GetAxisRaw("Action_1_2"); }
    public float GetInteractInput() { return Input.GetAxisRaw("Interact"); }
    public float GetReloadInput() { return Input.GetAxisRaw("Reload"); }

    public float GetCommandAttackInput() { return Input.GetAxisRaw("Command_AttackDefend"); }
    public float GetCommandRecallInput() { return Input.GetAxisRaw("Command_RecallRetreat"); }

    public bool GetCommandAttackHeld() { if (cAttackT > HOLD_TIME) { cAttackT = 0; return true; } return false; }
    public bool GetCommandDefendHeld() { if (cDefendT > HOLD_TIME) { cDefendT = 0; return true; } return false; }
    public bool GetCommandRecallHeld() { if (cRecallT > HOLD_TIME) { cRecallT = 0; return true; } return false; }
    public bool GetCommandRetreatHeld() { if (cRetreatT > HOLD_TIME) { cRetreatT = 0; return true; } return false; }

    public bool GetCommandRecallLong() { if (cRecallL > LONG_HOLD_TIME) { cRecallL = 0; return true; } return false; }
    public bool GetCommandRetreatLong() { if (cRetreatL > LONG_HOLD_TIME) { cRetreatL = 0; return true; } return false; }

    public float GetScrollWheelInput() { return Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity; }
    public int GetScrollWheelSpaced() { //MAYBE DELETE
        if (scrollLimter > 0)
            return 0;
        scrollT += Time.deltaTime * Input.GetAxisRaw("Mouse ScrollWheel") * scrollSensitivity;
        if (scrollT < -SCROLL_TIME) {
            scrollT = 0;
            scrollLimter = SCROLL_TIME;
            return -1;
        }
        else if (scrollT > SCROLL_TIME) {
            scrollT = 0;
            scrollLimter = SCROLL_TIME;
            return 1;
        }
        else {
            return 0;
        }
    }

    public float GetAnyInput(string name) { return Input.GetAxisRaw(name); }

    private void FixedUpdate() {
        if (Input.GetAxisRaw("Command_AttackDefend") > 0) cAttackT += Time.deltaTime; else cAttackT = 0;
        if (Input.GetAxisRaw("Command_AttackDefend") < 0) cDefendT += Time.deltaTime; else cDefendT = 0;
        if (Input.GetAxisRaw("Command_RecallRetreat") > 0) cRecallT += Time.deltaTime; else cRecallT = 0;
        if (Input.GetAxisRaw("Command_RecallRetreat") < 0) cRetreatT += Time.deltaTime; else cRetreatT = 0;
        if (Input.GetAxisRaw("Command_RecallRetreat") > 0) cRecallL += Time.deltaTime; else cRecallL = 0;
        if (Input.GetAxisRaw("Command_RecallRetreat") < 0) cRetreatL += Time.deltaTime; else cRetreatL = 0;

        if (scrollLimter > 0)
            scrollLimter -= Time.deltaTime;
    }
}