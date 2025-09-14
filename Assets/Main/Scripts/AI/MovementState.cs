using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
//using static UnityEngine.RuleTile.TilingRuleOutput;

public class MovementState<T> : State<T>
{
    Transform _self;
    Transform _target;
    NavMeshAgent _agent;
    ZombieView _view;
    List<GameObject> _allTargets;
    
    public MovementState(Transform self, List<GameObject> target, NavMeshAgent agent, ZombieView view)
    {
        _self = self;
        _allTargets = target;
        _agent = agent;
        _view = view;
    }

    public override void Enter()
    {
        base.Enter();
        ChangeTarget();
        _view.Walk(1);
    }
    
    public override void Execute()
    {
        base.Execute();

        if (_agent != null)
        {
            if (_target.position != _self.position)
            {
                _agent.SetDestination(_target.position);
            }
        }

        if (ConnectionManager.Instance.GetPlayersInRoom().Count != _allTargets.Count)
        {
            Debug.Log("desync");
            RedoList();
        }
        ChangeTarget();
    }

    void ChangeTarget()
    {
        float mindist = Mathf.Infinity;
        
        foreach (GameObject item in _allTargets)
        {
            if (item == null) continue;
            float dist = Vector2.Distance(_self.transform.position, item.transform.position);
            if (dist < mindist)
            {
                mindist = dist;
                _target = item.transform;
            }
        }
        Debug.Log("target: " + _target);
    }

    void RedoList()
    {
        var palyer = GameObject.FindGameObjectsWithTag("Player");
        _allTargets.Clear();
        for (int i = 0; i < palyer.Length; i++)
        {
            _allTargets.Add(palyer[i]);
        }
    }
}
