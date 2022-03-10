using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Button_Change_Scene : Button_Base {
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] bool playerFollower;//For Orange AI followers
    [SerializeField] string sceneName;

    private void Start() {
        transform.parent.GetChild(2).GetComponentInChildren<TMP_Text>().text = sceneName;
    }

    override protected void ButtonEffect() {
        SceneManager.LoadScene(sceneName);
    }
}
