using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Base : MonoBehaviour
{
    [SerializeField] bool toggle;
    [SerializeField] [Range(0f, 1f)] float moveTime, moveAmount;
    [SerializeField] private string text;
    private enum Button_State { moving, Up, Down}
    Button_State button_State = Button_State.Up;

    public void Press() {
        if (button_State == Button_State.moving)
            return;
        StartCoroutine("ButtonMovement");
    }

    IEnumerator ButtonMovement() {
        float direction = button_State == Button_State.Up ? -moveAmount : moveAmount, time = moveTime;
        button_State = Button_State.moving;
        yield return new WaitForFixedUpdate();

        while (time > 0) {
            transform.position += transform.forward * direction * 0.01f;
            time -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        button_State = direction < 0 ? button_State = Button_State.Down : button_State = Button_State.Up;

        if(button_State == Button_State.Up){ //Button has completed cycle so stop
            yield break;
        }
        else if (toggle) { //Toggle button stays down
            ButtonEffect();
            yield break;
        }
        else {
            ButtonEffect();//If non toggling the button will got back up
            StartCoroutine("ButtonMovement");
        }
    }

    public string GetTextString() {
        return text;
    }

    protected void ButtonEffect() {
        Debug.Log("This button needs to be overridden to do something useful!");
    }
}