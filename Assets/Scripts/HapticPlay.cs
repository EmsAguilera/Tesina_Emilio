using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.Tact.Unity;

public class HapticPlay : MonoBehaviour
{
    [Range(0.2f, 5f)]
    public float intensity = 1f;
    [Range(0.2f, 5f)]
    public float duration = 1f;

    [Range(0, 360)]
    public float angleX;
    [Range(-0.5f, 0.5f)]
    public float offsetY;

    public VestHapticClip clip;

    public int vest = 1;

    public void Play()
    {
        if(vest == 1)
        {
            clip.Play(intensity, duration, angleX, offsetY);
        }
    }
}
