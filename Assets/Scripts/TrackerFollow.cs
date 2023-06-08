using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class TrackerFollow : MonoBehaviour
{
    public ViveRoleProperty m_ViveRole;


    private void Update()
    {
        gameObject.transform.position = VivePose.GetPose(m_ViveRole).pos;
        gameObject.transform.rotation = VivePose.GetPose(m_ViveRole).rot;

        bool menuPressed = ViveInput.GetPress(m_ViveRole, ControllerButton.Menu);
        if (menuPressed)
        {
            Debug.Log("MenuPressed");
        }

        bool triggerPressed = ViveInput.GetPress(m_ViveRole, ControllerButton.Trigger);

        float scale = 0.2f;
        if (triggerPressed || menuPressed)
        {
            Debug.Log("Scale!" + name);
            scale = 0.4f;
        }
        gameObject.transform.localScale = Vector3.one * scale;
    }
}
