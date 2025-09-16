using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.Mathematics;

public class ZombieController : MonoBehaviourPunCallbacks, IDamageable
{
    NavMeshAgent _agent;
    LineOfSightMono _los;
    HealthScript health;
    [SerializeField] EnemyStats _stats;
    [SerializeField] GameObject _target;
    [SerializeField] List<GameObject> _allTargets = new List<GameObject>();
    ZombieView _view;
    
    void Awake()
    {
        _view = GetComponent<ZombieView>();
        _agent = GetComponent<NavMeshAgent>();
        _los = GetComponent<LineOfSightMono>();
        health = GetComponent<HealthScript>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    void Start()
    {
        if (!photonView.IsMine) return;     // solo el dueño corre la lógica

        _agent.speed = _stats._speed;
        health.InitHealth(_stats._health);
    }

    void Update()
    {
        if (!photonView.IsMine) return;     // solo el dueño corre la lógica

    }

    public void ChangeTarget()
    {
        float mindist = Mathf.Infinity;
        foreach (GameObject item in _allTargets)
        {
            float dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist < mindist)
            {
                mindist = dist;
                _target = item;
            }
        }
        Debug.Log("target: " + _target);
    }

    public void GetDamage(int damage)
    {
        health.TakeDamage(damage);
        CheckDeath();
    }

    void CheckDeath()
    {
        if (health._currentHealth > 0) return; // si no murio no hagas nada

        if (PhotonNetwork.IsMasterClient) // si es el master client destruyo al zombi y aviso al metodo del zombiespawner
        {
            PhotonNetwork.Destroy(gameObject);
            FindObjectOfType<ZombieSpawner>().OnZombieDied(photonView.ViewID);
        }
        else // si el que lo mata no es el master llamo al rpc para que este se encarge de matarlo
        {
            photonView.RPC(nameof(RPC_RequestDeath), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RPC_RequestDeath() // solo el master puede "matar" a los zombies
    {
        if (!PhotonNetwork.IsMasterClient) return;
        CheckDeath();
    }

}
