using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushPlate : MonoBehaviour
{
    [SerializeField] Red_AI ai;

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.TryGetComponent(out PlayerMovement pM)) {
            ai.AmbushTrigger();
            Destroy(this);
            Debug.Log("Ambush plater trigger for : " + ai.name);
        }
    }
}
