using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketScript : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject market;
    public bool _IsShooping => IsShooping;
    private bool IsShooping;

    public void Interact()
    {
        IsShooping = true;
        market.SetActive(true);
        Cursor.visible = true;
    }

    public void Exit()
    {
        IsShooping = false;
        market.SetActive(false);
        Cursor.visible = false;
    }
}
