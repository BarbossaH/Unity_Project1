using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{
    // public
    // private void Start()
    // {
    //     SwitchConfinerShape();
    // }
    private void OnEnable()
    {
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnSwitchBounds;
    }
    private void OnDisable()
    {
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnSwitchBounds;

    }

    private void OnSwitchBounds(SceneEvent sceneEvent, bool arg2, bool arg3)
    {
        if (sceneEvent == SceneEvent.AfterLoadScene)
        {
            SwitchConfinerShape();
        }
    }

    private void SwitchConfinerShape()
    {
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
        if (confinerShape != null)
        {
            confiner.m_BoundingShape2D = confinerShape;
            //Call this if the bounding shape's points change at runtime
            confiner.InvalidatePathCache();
        }
    }
}
