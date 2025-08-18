using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = ("Scriptable Objects/Enemies"))]
public class EnemyStats : ScriptableObject
{
    [field: SerializeField] public int _health {get; private set;}
    [field: SerializeField] public int _damage {get; private set;}
    [field: SerializeField] public int _range {get; private set;}
    [field: SerializeField] public float _speed {get; private set;}
    [field: SerializeField] public LayerMask _attackLayer {get; private set;}
    [field: SerializeField] public float _attackDistance { get; private set; }
    [field: SerializeField] public float _attackDuration { get; private set; }
    [field: SerializeField] public float _minAttackRange { get; private set; }

}