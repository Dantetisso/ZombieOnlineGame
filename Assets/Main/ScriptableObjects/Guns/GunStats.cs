using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = ("Scriptable Objects/Guns"))]
public class GunStats : ScriptableObject
{
    [field: SerializeField] public Sprite _gunImage { get; private set; }
    [field: SerializeField] public GunEnum _gunType { get; private set; }
    [field: SerializeField] public int _damage { get; private set; }
    [field: SerializeField] public int _range { get; private set; }
    [field: SerializeField] public bool _IsAutomatic { get; private set; }
    [field: SerializeField] public float _fireFate { get; private set; }
    [field: SerializeField] public int _clipSize { get; private set; }
    [field: SerializeField] public int _maxAmmo { get; private set; }
    [field: SerializeField] public AudioClip _drawSound { get; private set; }
    [field: SerializeField] public AudioClip _shootSound { get; private set; }
    [field: SerializeField] public AudioClip _shootEmptySound { get; private set; }
    [field: SerializeField] public AudioClip _reloadSound { get; private set; }

    // campos de la escopeta:
    [field: SerializeField] public int _pellets { get; private set; }           // nº de perdigones
    [field: SerializeField] public float _spreadAngle { get; private set; }     // angulo perdigones
}
