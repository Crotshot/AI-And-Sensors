using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intertact : MonoBehaviour
{
    //Shoots a ray from the camera if they have no weapon and 
    [SerializeField] float reach;
    Inputs inputs;
    Transform playerCamera;
    Weapon weapon;
    PlayerLeader pL;
    [SerializeField] LayerMask layerMask;
    UI ui;

    private void Start() {
        inputs = FindObjectOfType<Inputs>();
        playerCamera = transform.GetChild(0);
        ui = FindObjectOfType<UI>();
        pL = GetComponent<PlayerLeader>();
    }
    
    Ray cameraRay;
    private void Update() {
        if (Time.timeScale == 0)
            return;

        cameraRay.direction = playerCamera.forward;
        cameraRay.origin = playerCamera.position + playerCamera.forward * 0.5f;
        Debug.DrawRay(playerCamera.position + playerCamera.forward * 0.5f, playerCamera.forward * reach, Color.green);
        if (Physics.Raycast(cameraRay, out RaycastHit hit, reach, layerMask, QueryTriggerInteraction.Ignore)) { //Pick up weapon
           if(weapon == null) {
                if (hit.transform.TryGetComponent(out Weapon weap)) {
                    if (!weap.GetPickUp()) {
                        if (inputs.GetInteractInput() > 0 && weapon == null) {
                            weapon = weap;
                            weapon.PickUp(playerCamera.GetChild(0), true);
                            ui.CentreText("", false);
                        }
                        else {
                            ui.CentreText("pick Up " + weap.name, true);
                        }
                    }
                }
                else {
                    ui.CentreText("", false);
                }
           }
           if(hit.transform.TryGetComponent(out Button_Base button)) {
                ui.CentreText(button.GetTextString(), true);
                if (inputs.GetInteractInput() > 0) {
                    button.Press();
                }
           }
            else if (hit.transform.TryGetComponent(out Base_AI ai)) {
                AI_Hit(ai);
            }
        }
        else if (Physics.Raycast(cameraRay, out RaycastHit hit1, 999f, layerMask, QueryTriggerInteraction.Ignore)) { //Pick up weapon
            if(hit1.transform.TryGetComponent(out Base_AI ai)) {
                AI_Hit(ai);
            }
            else {
                ui.CentreText("", false);
            }
        }
        else {
            ui.CentreText("", false);
        }

        if (weapon != null){
            if (inputs.GetAction_1_Input() > 0) {
                GetComponentInChildren<NoiseMaker>().MakeNoise(weapon.Shoot());
            }
            if (inputs.GetReloadInput() > 0) {
                weapon.Reload();
            }
            if (inputs.GetInteractInput() < 0) { //Drop weapon
                weapon.Drop();
                weapon = null;
                Debug.Log(weapon);
            }
        }
    }

    private void AI_Hit(Base_AI ai) {
        if (ai.TryGetComponent(out Orange_AI aio)) {
            if (!aio.isPlayerFollower()) {
                ui.CentreText("Enemy Follower", true, false);
                pL.SetTarget(ai.transform);
            }
        }
        else {
            ui.CentreText("Enemy", true, false);
            pL.SetTarget(ai.transform);
        }
    }

    public Weapon GetWeapon() {
        return weapon;
    }
}