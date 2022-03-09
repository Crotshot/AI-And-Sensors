using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Grenade : MonoBehaviour
{
    [SerializeField][Min(0)]float radius, damage, armTime;
    [SerializeField] LayerMask unitLayer, unitDefaultLayer;
    float armTimer;

    private void Start() {
        armTimer = armTime;
    }

    private void Update() {
        armTimer -= armTimer <= 0 ? 0 : Time.deltaTime;
    }

    private void GrenadeDamageScan() {
        Collider[] scannedColliders = Physics.OverlapSphere(transform.position, radius, unitLayer, QueryTriggerInteraction.Collide);
        for (int i = 0; i < scannedColliders.Length; ++i) {
            if (scannedColliders[i].TryGetComponent(out Health health)) {
                Vector3 objPos = scannedColliders[i].transform.position;
                Vector3 directionToObject = (objPos + Vector3.up - transform.position).normalized;//All characters orgins are at their feet so +V3.up for close to centre
#if UNITY_EDITOR
                Debug.DrawRay(transform.position, directionToObject * radius, Color.blue, 5f);
#endif
                int countHitsOnTarget = 0;
                Ray ray;
                for (int y = 6; y > -7; y--) {//Draw rays to check if we hit the target
                    for (int x = 6; x > -7; x--) {
#if UNITY_EDITOR
                        Debug.DrawRay(transform.position, Quaternion.Euler(x * 5, y * 5, 0) * directionToObject * radius, Color.yellow, 5f);
#endif
                        ray = new Ray(transform.position, Quaternion.Euler(x * 5, y * 5, 0) * directionToObject);
                        if(Physics.Raycast(ray, out RaycastHit hit, radius, unitDefaultLayer, QueryTriggerInteraction.Ignore)) {
                            if(hit.collider == scannedColliders[i]) {
                                countHitsOnTarget++;
                            }
                        }
                    }
                }
                if(countHitsOnTarget > 0) {
#if UNITY_EDITOR
                    Debug.Log("Hit: " + health.name + " " + countHitsOnTarget + " time(s)!");
#endif
                    health.HealthChange(-damage * countHitsOnTarget);
                }
            }
        }

        ParticleSystem pS = GetComponent<ParticleSystem>();
        pS.Emit(30);
        armTime = 10f;
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        StartCoroutine("DestroyGrenade");
    }

    IEnumerator DestroyGrenade() {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision) {
        if (armTimer > 0)
            return;

        GrenadeDamageScan();
    }

#if UNITY_EDITOR
    public bool test;
    private void OnValidate() {
        if (test) {
            GrenadeDamageScan();
            test = false;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}