using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerView : MonoBehaviour
{
    [SerializeField] Animator animator;
    public Action WalkOn;
    public Action WalkOff;
    private void Awake()
    {
        WalkOn += walkon;
        WalkOff += walkoff;
    }
    void walkon()
    {
        animator.SetBool("walk", true);
    }
    void walkoff()
    {
        animator.SetBool("walk", false);
    }
}
