using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletion : MonoBehaviour
{
    [SerializeField] float levelTime = 180, spawnInterval = 25f;
    float spawnTimer;
    [SerializeField] bool testing;
    [SerializeField] GameObject[] ai;
    [SerializeField] Transform[] spawnPoints, patrolPoints;
    int keysCollected, keysNeeded = 0;
    UI ui;
    bool over;

    void Start()
    {
        spawnTimer = spawnInterval;
        ui = FindObjectOfType<UI>();
        foreach (PickUp pack in FindObjectsOfType<PickUp>()) {
            if(pack.pickUpType == PickUp.PickUpType.Key) {
                keysNeeded++;
            }
        }
    }

    void Update()
    {
        if (over)
            return;

        spawnTimer -= Time.deltaTime;
        levelTime -= Time.deltaTime;

        if(spawnTimer <= 0) {
            spawnTimer = spawnInterval;
            GameObject a = Instantiate(ai[Random.Range(0, ai.Length)], transform.position, Quaternion.identity);
            a.GetComponent<Base_AI>().AssignControlPoints(patrolPoints);
        }

        ui.TimeText(TimeConverter(levelTime));
        if(levelTime <= 0 && !testing) {
            FindObjectOfType<PlayerMovement>().GetComponent<Health>().HealthChange(-999999);
        }

        if(keysNeeded == keysCollected) {
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
