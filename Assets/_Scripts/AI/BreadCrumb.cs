using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadCrumb : MonoBehaviour
{
    //decayRate is how much strength is lost per second
    float strength, decayRate, minRadius;
    SphereCollider crumbTrigger;

#if UNITY_EDITOR
    Gradient gradient;
    float maxStrength;
#endif

    private void Update() {
        strength -= decayRate * Time.deltaTime;
        crumbTrigger.radius += decayRate * Time.deltaTime;
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, transform.forward * strength * 2f, Color.blue);
#endif
        if (strength <= 0)
            Destroy(gameObject);
    }

    public void SetupCrumb(float strength, float deviation, float minRadius, float decayTime) {
        float modifier = Random.value * (Random.value >= 0.5f ? 1 : -1);
        this.strength = strength + modifier * deviation;
        this.minRadius = minRadius;

        crumbTrigger = gameObject.AddComponent<SphereCollider>();
        crumbTrigger.isTrigger = true;
        crumbTrigger.radius = this.strength + minRadius;

        transform.position += Vector3.up * ((this.strength + minRadius) / 2);

        decayRate = (strength + deviation) / decayTime;//Decays at constant rate regardless of strength

#if UNITY_EDITOR
        maxStrength = strength + deviation;
#endif
    }

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out AI_Smell ai_Smell)) {
            ai_Smell.CalculateSniffDirection(transform, strength);
        }    
    }

#if UNITY_EDITOR //Editor only as we do not see this in game
    public void SetGradient(Gradient gradient) {
        this.gradient = gradient;
    }

    private void OnDrawGizmos() {//Draw the bread crumb so it is easier to see
        Gizmos.color = gradient.Evaluate(strength/maxStrength);
        Gizmos.DrawSphere(transform.position, crumbTrigger.radius);
    }
#endif
}