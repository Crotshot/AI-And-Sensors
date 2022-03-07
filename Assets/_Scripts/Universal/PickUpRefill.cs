using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpRefill : MonoBehaviour{

    /// <summary>
    /// -1 charges for infinite reosurceRespawn
    /// </summary>
    [SerializeField] int charges;
    [SerializeField] float refillTime;
    [SerializeField] GameObject resourcePrefab;

    private void Start() {
        Instantiate(resourcePrefab, transform.position, transform.rotation);
    }

    public void PickUpConsumed() {
        if (charges > 0)
            charges--;
        else if (charges == 0) {
            Destroy(gameObject);
            return;
        }
        StartCoroutine("RespawnResource");
    }

    IEnumerator RespawnResource() {
        yield return new WaitForSeconds(refillTime);
        Instantiate(resourcePrefab, transform);
    }
}