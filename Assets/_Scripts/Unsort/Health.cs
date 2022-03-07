using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {
    [SerializeField] bool player;
    [SerializeField] float maxHealth;
    float health;
    UI ui;

    public UnityEvent death;
    public UnityEvent<Vector3> damaged;

    private void Start() { 
        health = maxHealth;
        if (death == null)
            death = new UnityEvent();
        if (damaged == null)
            damaged = new UnityEvent<Vector3>();
        if (player) {
            ui = FindObjectOfType<UI>();
            ui.HealthText(health);
        }
    }

    public void HealthChange(float amount) {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        if(player)
            ui.HealthText(health);
        if (amount < 0)
            damaged?.Invoke(Vector3.zero);
        if (health == 0) {
            death?.Invoke();
        }
        //Debug.Log("HP: " + health);
    }

    public void HealthChange(float amount, Vector3 origin) {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        if (player)
            ui.HealthText(health);
        if (amount < 0)
            damaged?.Invoke(origin);
        if (health == 0) {
            death?.Invoke();
        }
        //Debug.Log("HP: " + health);

    }
    public float GetHeatlthPercent() {
        return health == 0 ? 0 : health / maxHealth;
    }

    private void OnEnable() {
        health = maxHealth;
    }
}