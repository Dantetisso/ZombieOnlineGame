using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieView : MonoBehaviour
{
    [SerializeField] Animator _animator;

    public Action<int> Walk;
    public Action Attack;
    // Start is called before the first frame update
    void Start()
    {
        Walk += IsWalking;
        Attack += IsAttacking;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IsWalking(int value)
    {
        _animator.SetInteger("walking", value);
    }

    void IsAttacking()
    {
        _animator.SetTrigger("attack");
    }
}
