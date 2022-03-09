using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushPlate : MonoBehaviour
{
    [SerializeField] Red_AI ai;


    private void OnCollisionEnter(Collision collision) {
        ai.AmbushTrigger();
        Debug.Log("Ambush plater trigger for : " + ai.name);
        Destroy(this);
    }
}
