using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
  [SerializeField] private int id;
  private GunHolderScript gunHolder;

  void Start()
  {
    gunHolder = GetComponent<GunHolderScript>();
  }

  public void OnButton()
  {
    gunHolder = FindObjectOfType<GunHolderScript>();
    
    gunHolder.ChangeGun(id);
    gunHolder.LoadGun(id);
  }

}
