using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] Transform destination;

    private void OnTriggerEnter(Collider other) {
        if (other.tag.Equals("Player")) {
            other.transform.position = destination.transform.position;
            other.transform.rotation = destination.transform.rotation;
        }
    }
}
