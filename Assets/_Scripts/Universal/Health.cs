using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {
    [SerializeField] bool player;
    [SerializeField] float maxHealth;
    float health;
    UI ui;

    public UnityEvent death, damaged;
    private void Start() { 
        health = maxHealth;
        if (death == null)
            death = new UnityEvent();
        if (damaged == null)
            damaged = new UnityEvent();
        if (player) {
            ui = FindObjectOfType<UI>();
            ui.HealthText(health);
        }
    }

    #region Test
    public bool hurt;
    private void Update() {
        if (hurt) {
            hurt = false;
            HealthChange(-25f);
        }
    }
    #endregion


    public void HealthChange(float amount) {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        if(player)
            ui.HealthText(health);
        if (amount < 0)
            damaged?.Invoke();
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