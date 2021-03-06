using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Helpers = Crotty.Helpers.StaticHelpers;

public class NoiseListener : MonoBehaviour
{
    /// <summary>
    /// Controls how deaf a character is, the higher the deafness the closer the listener will need to be to the sound to acknowledge it
    /// </summary>
    [SerializeField] [Range(0f, 1f)] private float deafness = 0;
    /// <summary>
    /// Controls how close to the sound origin the position sent to the AI will be
    /// Min -> The minimum additional inaccuracy at range
    /// Max -> The maximum in accuracy of sound at the furthest point the sound could be heard
    /// Accuracy->The maximum accuracy in units achievable for the listener
    /// Range->How far the sound has to be for inaccuracy to take effect
    /// </summary>
    [SerializeField] [Range(0f, 25f)] private float rangeAccuracyMin, rangeAccuracyMax, inaccuracy, range;

    public UnityEvent<Vector3> soundObeserved;

    private void Start() {
        if (soundObeserved == null)
            soundObeserved = new UnityEvent<Vector3>();
    }

    public void SoundHeard(Vector3 soundOrigin, float soundRadius) {
        if(Helpers.Vector3Distance(transform.position, soundOrigin) <= soundRadius * (1f - deafness)){
            float maxAudibleDistance = soundRadius * (1f - deafness),
            percentInaccuracy = 1f - (range / maxAudibleDistance),
            totalInaccuracy =  ((rangeAccuracyMin + (rangeAccuracyMax - rangeAccuracyMin)) * percentInaccuracy) + inaccuracy;
            Vector3 projectedOrigin = soundOrigin + new Vector3(Random.Range(-totalInaccuracy, +totalInaccuracy),0, Random.Range(-totalInaccuracy, +totalInaccuracy));
            soundObeserved?.Invoke(projectedOrigin);
        }
        else {
            Debug.DrawRay(transform.position, Vector3.up, Color.grey, 5f);
        }
    }
}
