using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    SphereCollider audibleNoise;
    [SerializeField] float minHearRadius = 1, decayRate = 6f, hangTime = 0.25f;
    float hangTimer;

    private void Start() {
        audibleNoise = GetComponent<SphereCollider>();    
    }
    //TEST BOOL DEL LATER
    public bool NOISE;
    //
    private void FixedUpdate() {
        //
        if (NOISE) {
            NOISE = false;
            MakeNoise(20 + audibleNoise.radius);
        }
        //
        if (hangTimer <= 0) {
            if (audibleNoise.radius > minHearRadius) {
                audibleNoise.radius -= decayRate * (audibleNoise.radius/5f) * Time.deltaTime;
            }
        }
        else
            hangTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent( out NoiseListener listener)) {
            listener.SoundHeard(transform.position, audibleNoise.radius);
        }
    }

    public void MakeNoise(float noiseRadius) {
        if (noiseRadius < audibleNoise.radius)
            return;

        audibleNoise.radius = noiseRadius;
        hangTimer = hangTime;
    }
}