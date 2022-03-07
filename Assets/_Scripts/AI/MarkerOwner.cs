using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerOwner : MonoBehaviour
{
    //Marker type is stored in the name of the object when the object is created
    public int markerOwner;

    private void Start() {
        SphereCollider sC = gameObject.AddComponent<SphereCollider>();
        sC.radius = 0.25f;
        sC.isTrigger = true;
        gameObject.layer = 7;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
#endif
}