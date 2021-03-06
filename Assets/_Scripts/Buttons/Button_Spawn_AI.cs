using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Button_Spawn_AI : Button_Base
{
    [SerializeField] GameObject prefabToSpawn;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] bool playerFollower;//For Orange AI followers

    private void Start() {
        transform.parent.GetChild(2).GetComponentInChildren<TMP_Text>().text = prefabToSpawn.name;
    }

    override protected void ButtonEffect() {
        Transform pickedPos = spawnPoints[0];
        if (spawnPoints.Length > 1) {
            pickedPos = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        GameObject ai = Instantiate(prefabToSpawn, pickedPos.position, pickedPos.rotation);

        if (ai.TryGetComponent(out Orange_AI oAI)) {
            oAI.SetPlayerFollower(playerFollower);
        }
        if (patrolPoints.Length > 0) {
            ai.GetComponent<Base_AI>().AssignControlPoints(patrolPoints);
        }
    }
}
