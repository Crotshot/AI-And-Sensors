using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Weapon : MonoBehaviour
{
    [SerializeField] protected int magSize, reserveSize, startingReserve, hitParticleCount = 15;
    [SerializeField] protected float fireRate_RPM, reloadTime, inaccuracy, maxRange = 80f, audbileDistance, damage;
    [SerializeField] Image reloadFillImage;
    [SerializeField] TMP_Text magText, reserveText;
    [SerializeField] ParticleSystem casing, bullet, muzzle;
    [SerializeField] Transform bulletPos;
    [SerializeField] LayerMask layer;
    Animator animator;
    Transform hitPoint, fakeParent;

    int currentMag, currentReserve;
    private bool pickUp, canShoot,  reloading, playerHeld, aiHeld;
    private float reloadTimer, shotTimer;
    Rigidbody rb;
    ParticleSystem hit;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        currentMag = magSize;
        currentReserve = startingReserve;
        animator = GetComponent<Animator>();
        hitPoint = transform.GetChild(2);
        hit = hitPoint.GetComponent<ParticleSystem>();
    }

    private void Update() {
        if (aiHeld && fakeParent != null) {
//            transform.position = fakeParent.TransformPoint(fakeParent.position);
            transform.position = fakeParent.position;
            transform.eulerAngles = fakeParent.eulerAngles;
        }
        if (reloading && pickUp) {
            if(reloadTimer > 0) {
                reloadTimer -= Time.deltaTime;
                reloadFillImage.fillAmount = reloadTimer / reloadTime;
            }
            else {
                reloadTimer = 0;
                reloadFillImage.fillAmount = 0;
                reloading = false;
                canShoot = true;
                //STOP ANIMATION

                int currentReserveAfterBullets = currentReserve - (currentMag > 0 ? magSize - currentMag + 1 : magSize);
                currentMag = (currentMag > 0 ? 1 : 0) + (currentReserveAfterBullets > 0 ? magSize : currentReserve);
                currentReserve = Mathf.Clamp(currentReserveAfterBullets, 0, reserveSize);
                //Debug.Log("Reloaded==>> Mag: " + currentMag + ",  Reserve: " + currentReserve);
                magText.text = currentMag.ToString();
                reserveText.text = currentReserve.ToString();
            }
        }
        shotTimer = shotTimer >= 0 ? shotTimer -= Time.deltaTime : 0;
        if(shotTimer < 0 && ((!aiHeld && currentMag > 0 && !reloading) || (aiHeld && currentReserve > 0))) {
            canShoot = true;
            shotTimer = 0;
        }
    }

    public float Shoot() {
        if (!pickUp)
            return 0f;
        if(canShoot) {
            if (!aiHeld)
                currentMag--;
            else
                currentReserve--;
            
            shotTimer = 1f/(fireRate_RPM / 60f);
            magText.text = currentMag.ToString();
            if(!aiHeld)
                animator.SetTrigger("Shot");

            Ray ray = new Ray(bulletPos.position, bulletPos.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRange, layer, QueryTriggerInteraction.Ignore)) {
                Debug.DrawLine(bulletPos.position, hit.point, Color.green, 5f);
                hitPoint.position = hit.point;
                hitPoint.LookAt(transform.position);
                this.hit.Emit(hitParticleCount);
                //DAMAGE
                if(hit.collider.TryGetComponent(out Health health)) {
                    health.HealthChange(-damage, transform.position);
                }
            }
            else {
                //bullet.Emit(1);
                Debug.DrawRay(bulletPos.position, bulletPos.forward * maxRange, Color.red, 5f);
            }

            casing.Emit(1);
            muzzle.Emit(1);
            if (currentMag >= 0 || (aiHeld && currentReserve >= 0)) {
                canShoot = false;
            }

            return audbileDistance;
        }
        else if (reloading) {
            //Click//Click
            Debug.Log("Click, Click");
            return 0f;
        }
        return 0f;
    }

    public void PickUp(Transform point, bool player) {
        pickUp = true;
        if (player) {
            aiHeld = false;
            bulletPos = FindObjectOfType<Camera>().transform;
            transform.GetChild(0).GetChild(0).gameObject.SetActive(true); //Enable Weapon UI
        }
        else {
            fakeParent = point;
            aiHeld = true;
            bulletPos = transform.GetChild(1);
            currentReserve += currentMag;
            currentMag = 0;
        }
        canShoot = !reloading;
        if(!aiHeld)
            transform.parent = point;
        
        magText.text = currentMag.ToString();
        reserveText.text = currentReserve.ToString();

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        if (reloading) {
            animator.SetTrigger("Reload");
        }
    }

    public void Reload() {
        if (reloading || currentReserve == 0)
            return;
        canShoot = false;
        reloading = true;
        animator.SetTrigger("Reload");
        reloadTimer = reloadTime;
    }

    public void Drop() {
        playerHeld = false;
        pickUp = false;
        transform.GetChild(0).GetChild(0).gameObject.SetActive(false); //Disable Weapon UI
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        fakeParent = null;
        transform.parent = null;

        if (reloading) {
            reloadTimer = reloadTime;
        }
    }

    public void AddAmmo(int amount) {
        currentReserve = Mathf.Clamp(currentReserve + amount, 0, reserveSize);
        reserveText.text = currentReserve.ToString();
    }

    public float GetAmmoPercent() {
        if (currentReserve == 0)
            return 0;
        return ((float)currentReserve) / (float) reserveSize;
    }

    public bool GetPickUp() {
        return pickUp;
    }
}