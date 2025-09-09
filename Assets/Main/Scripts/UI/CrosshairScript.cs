using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CrosshairScript : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Cursor.visible = false;
    }
    
    void Update()
    {
        if(photonView.IsMine)
        {
            Vector2 MouseCursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = MouseCursorPos;
        }
    }
}
