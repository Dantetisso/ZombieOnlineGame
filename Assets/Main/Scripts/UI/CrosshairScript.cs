using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CrosshairScript : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        Cursor.visible = false;
    }
    
    private void Update()
    {
        if(photonView.IsMine)
        {
            Vector2 MouseCursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = MouseCursorPos;
            transform.rotation = Quaternion.identity;
        }
    }
}
