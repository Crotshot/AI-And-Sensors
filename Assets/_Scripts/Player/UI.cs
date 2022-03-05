using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [SerializeField] TMP_Text centreText, healthText;
    [SerializeField] GameObject EscMenu, gameOver;
    Inputs inputs;

    bool menuToggle;

    private void Start() {
        inputs = FindObjectOfType<Inputs>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        FindObjectOfType<PlayerMovement>().GetComponent<Health>().death.AddListener(GameOver);
    }

    private void Update() {
        if (gameOver.activeInHierarchy)
            return;

        if (menuToggle) {
            if(inputs.GetSpaceEsc() < 0) {
                EscMenu.SetActive(EscMenu.activeInHierarchy ^ true);
                Cursor.visible ^= true;
                menuToggle = false;
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? Cursor.lockState = CursorLockMode.None : Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = Time.timeScale == 0 ? Time.timeScale = 1 : Time.timeScale = 0;
            }
        }
        else if(inputs.GetSpaceEsc() == 0) {
            menuToggle = true;
        }
    }

    public void CentreText(string objectName, bool enable) {
        if (enable) {
            centreText.text = "[Interact] to Pick Up " + objectName;
        }
        else {
            centreText.text = "";
        }
    }

    public void HealthText(float health) {
        healthText.text = health.ToString("F0");
    }

    public void GameOver() {
        gameOver.SetActive(true);
        Cursor.visible ^= true;
        menuToggle = false;
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? Cursor.lockState = CursorLockMode.None : Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = Time.timeScale == 0 ? Time.timeScale = 1 : Time.timeScale = 0;
    }

    #region Buttons
    public void B_Resume() {
        EscMenu.SetActive(EscMenu.activeInHierarchy ^ true);
        Cursor.visible ^= true;
        menuToggle = false;
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? Cursor.lockState = CursorLockMode.None : Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = Time.timeScale == 0 ? Time.timeScale = 1 : Time.timeScale = 0;
    }
    public void B_Quit() {
        Application.Quit();
    }
    public void B_Restart() {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion
}
