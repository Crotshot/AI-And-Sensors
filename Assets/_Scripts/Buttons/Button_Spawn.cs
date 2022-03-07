using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Button_Spawn : Button_Base
{
    [SerializeField] GameObject prefabToSpawn;
    [SerializeField] Transform spawnPoint;

    private void Start() {
        transform.parent.GetChild(2).GetComponentInChildren<TMP_Text>().text = prefabToSpawn.name;
    }

    override protected void ButtonEffect() {
        Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
    }
}