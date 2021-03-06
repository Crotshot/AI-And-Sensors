using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    SphereCollider audibleNoise;
    [SerializeField] float minHearRadius = 0.1f;
    float radius;
    bool noiseFrame;

    private void Start() {
        audibleNoise = GetComponent<SphereCollider>();    
    }

    private void FixedUpdate() {
        if(radius > minHearRadius) {
            radius *= 0.98f;
        }
        if (noiseFrame) {
            audibleNoise.radius = minHearRadius;
        }
        else {
            if (radius < audibleNoise.radius)
                return;
            audibleNoise.radius = radius;
        }
        noiseFrame ^= true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent( out NoiseListener listener)) {
            listener.SoundHeard(transform.position, audibleNoise.radius);
        }
    }

    public void MakeNoise(float noiseRadius) {
        if (noiseRadius < radius)
            return;
        radius = noiseRadius;
    }

    public void off() {
        radius = 0;
    }
}