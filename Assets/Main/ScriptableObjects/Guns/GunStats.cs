using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = ("Scriptable Objects/Guns"))]
public class GunStats : ScriptableObject
{
    [field: SerializeField] public GunEnum _gunType{ get; private set; }
    [field: SerializeField] public int _damage { get; private set; }
    [field: SerializeField] public int _range { get; private set; }
    [field: SerializeField] public bool _IsAutomatic { get; private set; }
    [field: SerializeField] public float _fireFate { get; private set; }
    [field: SerializeField] public int _clipAmmo { get; private set; }
    [field: SerializeField] public int _maxAmmo { get; private set; }
    [field: SerializeField] public AudioClip _shootSound{ get; private set; }
}
