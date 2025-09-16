using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Config", menuName = ("Scriptable Objects/GameConfig"))]
public class GameConfig : ScriptableObject
{
    [field: SerializeField] public int _maxWaves { get; private set; }
    [field: SerializeField] public int _baseZombies { get; private set; }
    [field: SerializeField] public int _zombiesPerRound { get; private set; }
    [field: SerializeField] public int _bossRound { get; private set; }
}