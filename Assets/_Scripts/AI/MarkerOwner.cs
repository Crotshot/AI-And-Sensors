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
    }
}
