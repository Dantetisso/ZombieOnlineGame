using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deathState<T> : State<T>
{
    GameObject _obj;

    public deathState(GameObject obj)
    {
        _obj = obj;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("it's dead");
        _obj.SetActive(false);
        
    }
}
