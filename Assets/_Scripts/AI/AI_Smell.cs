using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AI_Smell : MonoBehaviour
{
    /// <summary>
    /// Anosmia is the impaiment of one smell, higher values means less smell
    /// </summary>
    [SerializeField][Range(0f, 1f)]float anosmiaStrength;
    /// <summary>
    /// The minimum smell that a character can detect, any strength beneath this is disraguarded
    /// The maximum smell is the strength a character needs to be able to get the maximum directional accuracy
    /// </summary>
    [SerializeField] [Min(0f)] float minSmellStrength, maxSmellStrength;
    /// <summary>
    /// Inaccuracy => the directional reading are for a characters smell, high values means more inaccuracy
    /// Accuracy => The maximum accuracy the character can have from smell
    /// Inaccuracy will increase from 0 to its value based on strength of smell bewteen max and min
    /// </summary>
    [SerializeField][Range(0f, 45f)]float inAccuracy, accuracy;
    /// <summary>
    /// investigationTimeMinimum => Minimum time a character will remember after smelling a bread crumb
    /// invesigationTimeMaximum => Maximum time a character will rememeber after smelling a bread crumb
    /// Will interpolate depending on the strength of the smell between max and min
    /// </summary>
    [SerializeField] [Range(0f, 60f)] float investigationTimeMinimum, invesigationTimeMaximum;
    // The current direction the character will investigate after Calculating sniff direction
    Vector3 sniffDirection = Vector3.zero;
    //Remember timer;
    float rememberenceTimer;
    public UnityEvent<Vector3> scentDetected;//Using unity event so I can add listemers to the main AI and if there is no Smell scipt I can prevent errors easily.

    private void Start() {
        if (scentDetected == null)
            scentDetected = new UnityEvent<Vector3>();
    }

    public void CalculateSniffDirection(Transform breadCrumb, float strength) {
        strength *= (1 - anosmiaStrength);
        if (strength  < minSmellStrength)
            return;//Smell too weak or character is smelling impaired

        float percentStrength = Mathf.Clamp((strength - minSmellStrength) / (maxSmellStrength - minSmellStrength), 0f, 1f);//How far the smell is between min and max smell strength, if above max set to 0
        StopAllCoroutines();
        rememberenceTimer = investigationTimeMinimum + Mathf.Clamp(((invesigationTimeMaximum - investigationTimeMinimum) * percentStrength), 0, 60f);
        StartCoroutine("MemoryLoss");

        float deltaAngle = accuracy + (inAccuracy * (1 - percentStrength));//Weaker the smell the more inaccurate
        breadCrumb.eulerAngles = new Vector3(0, breadCrumb.eulerAngles.y + Random.Range(-deltaAngle, deltaAngle), 0);//Smelling breadcrumbs alters them

        sniffDirection = breadCrumb.forward;
        
        scentDetected?.Invoke(sniffDirection);

#if UNITY_EDITOR
        //Debug.Log("New Smell Direction: " + sniffDirection);
        Debug.DrawRay(transform.position, sniffDirection * 4f, Color.magenta, 10f);
#endif
    }


    IEnumerator MemoryLoss() {
        while (true) {
            yield return new WaitForSeconds(1f);
            rememberenceTimer -= 1f;
            if (rememberenceTimer <= 0)
                break;
        }
        sniffDirection = Vector3.zero;
        scentDetected?.Invoke(sniffDirection);//When direction is 0,0,0 the smell is lost
    }
}