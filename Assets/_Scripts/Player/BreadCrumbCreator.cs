using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadCrumbCreator : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// Colour gradient for the generated crumb, a max strength crumb will fully tansition the whole gradient, this is only for testing in editor to heplp visualize the crumbs
    /// </summary>
    [SerializeField] Gradient gradient;
#endif
    /// <summary>
    /// The strength and radius of the generated bread crumbs, the radius of the crumb will be the strength  +-(rand.value * strengthDeviation) + minRadius
    /// Decay time is the maximum time it will take for a full strength bread crumb to fully deplete
    /// </summary>
    [SerializeField] float normalStrength, strengthDeviation, minRadius, decayTime;
    /// <summary>
    /// breadCrumbTime -> Interval between placing bread crumbs 
    /// brushModifier  -> Increases frequency of bread crumb placing when brushing against obsticles
    /// </summary>
    [SerializeField] float breadCrumbTime, brushModifier;
    float breadCrumbTimer;
    Transform lastCrumb;

    void Update()
    {
        breadCrumbTimer -= Time.deltaTime;
        if(breadCrumbTimer <= 0) {
            breadCrumbTimer += breadCrumbTime;
            GenerateCrumb();
        }
    }

    void GenerateCrumb() {
        GameObject crumb = new GameObject("Bread Crumb");
        crumb.transform.position = transform.position;
        crumb.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y,0);

        if (lastCrumb != null) {
            lastCrumb.LookAt(transform);
            crumb.transform.eulerAngles = new Vector3(0, crumb.transform.eulerAngles.y, 0);
        }
        lastCrumb = crumb.transform;

        BreadCrumb bCrumb = crumb.AddComponent<BreadCrumb>();
        bCrumb.SetupCrumb(normalStrength, strengthDeviation, minRadius, decayTime);
#if UNITY_EDITOR
        bCrumb.SetGradient(gradient);
#endif
    }
}