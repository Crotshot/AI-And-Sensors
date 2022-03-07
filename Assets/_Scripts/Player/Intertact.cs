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
    int layer;
    UI ui;

    private void Start() {
        inputs = FindObjectOfType<Inputs>();
        playerCamera = transform.GetChild(0);
        layer = 1 << LayerMask.NameToLayer("Unit");
        ui = FindObjectOfType<UI>();
    }
    
    Ray cameraRay;
    private void Update() {
        if (Time.timeScale == 0)
            return;

        cameraRay.direction = playerCamera.forward;
        cameraRay.origin = playerCamera.position;

        if (Physics.Raycast(cameraRay, out RaycastHit hit, reach, ~layer, QueryTriggerInteraction.Ignore)) { //Pick up weapon
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
        }
        else {
            ui.CentreText("", false);
        }

        if (weapon != null){
            if (inputs.GetAction_1_2_Input() > 0) {
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

    public Weapon GetWeapon() {
        return weapon;
    }
}