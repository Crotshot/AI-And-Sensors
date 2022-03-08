using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class NoiseMakerDisableVolume : MonoBehaviour
{
    [SerializeField] Transform c1, c2;
    NoiseMaker nM;
    Transform player;

    private void Start() {
        player = FindObjectOfType<PlayerMovement>().transform;
        nM = player.GetComponentInChildren<NoiseMaker>();
    }

    private void Update() {
        Vector3 pp = player.position;
        //I had to do this terribleness as Unity triggers trigger each other and whenever the player and their noiseMaker went near the safe room it would be disabled
        if (Helpers.FloatIsBetween(c1.position.x, c2.position.x, pp.x) && Helpers.FloatIsBetween(c1.position.y, c2.position.y, pp.y) && Helpers.FloatIsBetween(c1.position.z, c2.position.z, pp.z)) {
            nM.enabled = false;
            nM.off();
        }
        else {
            nM.enabled = true;
        }
    }
}