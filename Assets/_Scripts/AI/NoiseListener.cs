using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers = Crotty.Helpers.StaticHelpers;

public class NoiseListener : MonoBehaviour
{
    /// <summary>
    /// Controls how deaf a character is, the higher the deafness the closer the listener will need to be to the sound to acknowledge it
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float deafness = 0;

    public void SoundHeard(Vector3 soundOrigin, float soundRadius) {
        if(Helpers.Vector3Distance(transform.position, soundOrigin) <= soundRadius * (1f - deafness))
            Debug.Log("I heard a sound at " + soundOrigin);
    }
}
