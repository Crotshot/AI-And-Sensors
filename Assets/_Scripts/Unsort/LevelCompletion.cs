using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletion : MonoBehaviour
{
    [SerializeField] float levelTime = 180;
    [SerializeField] bool testing;
    int keysCollected, keysNeeded = 0;
    UI ui;
    bool over;

    void Start()
    {
        ui = FindObjectOfType<UI>();
        foreach (PickUp pack in FindObjectsOfType<PickUp>()) {
            if(pack.pickUpType == PickUp.PickUpType.Key) {
                keysNeeded++;
            }
        }
    }

    void Update()
    {
        levelTime -= Time.deltaTime;
        ui.TimeText(TimeConverter(levelTime));
        if(levelTime <= 0 && !testing) {
            FindObjectOfType<PlayerMovement>().GetComponent<Health>().HealthChange(-999999);
        }

        if(keysNeeded == keysCollected && !over) {
            ui.Victory(TimeConverter(levelTime), keysCollected);
            over = true;
        }
    }

    public void KeyCollected() {
        keysCollected++;
    }


    private string TimeConverter(float time) {
        string ret = "";
        int m = (int)time / 60;
        if (m == 0) {
            ret = "00 : ";
        }
        else if (m < 10) {
            ret = "0" + m.ToString() + " : ";
        }
        else {
            ret = m.ToString() + " : ";
        }
        m = (int)time % 60;
        if (m == 0) {
            ret += "00";
        }
        else if (m < 10) {
            ret += "0" + m.ToString();
        }
        else {
            ret += m.ToString();
        }
        return ret;
    }

}
