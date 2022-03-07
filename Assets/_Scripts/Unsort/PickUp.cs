using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum PickUpType { Health, Ammo, Key }
    [SerializeField] public PickUpType pickUpType;
    [SerializeField] float resourceRestored;

    public void PickedUp() {
        Destroy(gameObject);
    }

    public PickUpType getType() {
        return pickUpType;
    }

    private void OnTriggerEnter(Collider other) {
        if(pickUpType == PickUpType.Health && other.TryGetComponent(out Health health)) {
            if(health.GetHeatlthPercent() < 1) {
                health.HealthChange(resourceRestored);
                if(transform.parent != null) {
                    if(transform.parent.TryGetComponent(out PickUpRefill fill)) {
                        fill.PickUpConsumed();
                    }
                }
                Destroy(gameObject);
            }
        }

        if (pickUpType == PickUpType.Ammo && other.TryGetComponent(out Intertact inter)) {
            Weapon weap = inter.GetWeapon();
            if (weap == null)
                return;
            if (weap.GetAmmoPercent() < 1) {
                weap.AddAmmo((int)resourceRestored);
                if (transform.parent != null) {
                    if (transform.parent.TryGetComponent(out PickUpRefill fill)) {
                        fill.PickUpConsumed();
                    }
                }
                Destroy(gameObject);
            }
        }

        if (pickUpType == PickUpType.Ammo && other.TryGetComponent(out Base_AI ai)) {
            Weapon weap = ai.GetWeapon();
            if (weap == null)
                return;
            if (weap.GetAmmoPercent() < 1) {
                weap.AddAmmo((int)resourceRestored);
                if (transform.parent != null) {
                    if (transform.parent.TryGetComponent(out PickUpRefill fill)) {
                        fill.PickUpConsumed();
                    }
                }
                Destroy(gameObject);
            }

        }
        if (pickUpType == PickUpType.Key && other.tag.Equals("Player")) {
            FindObjectOfType<LevelCompletion>().KeyCollected();
            Destroy(gameObject);
        }
    }
}