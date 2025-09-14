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

    FSM<StateEnum> _fsm;
    ITreeNode _root;
    Coroutine _searchTarget;
    
    void Awake()
    {
        _view = GetComponent<ZombieView>();
        _agent = GetComponent<NavMeshAgent>();
        _los = GetComponent<LineOfSightMono>();
        health = GetComponent<HealthScript>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        health.maxHealth = _stats._health;
    }

    void Start()
    {
        if (!photonView.IsMine) return;     // solo el dueño corre la lógica

        _agent.speed = _stats._speed;
        InitializeFSM();
        InitializeTree();
    }

    void Update()
    {
        if (!photonView.IsMine) return;     // solo el dueño corre la lógica

        _fsm.OnExecute();
        _root.Execute();

    }

    void InitializeFSM()
    {
        _fsm = new FSM<StateEnum>();
        var idle = new IdleState<StateEnum>(_agent, LostTarget);
        var move = new MovementState<StateEnum>(transform, _allTargets, _agent, _view);
        var dead = new deathState<StateEnum>(gameObject);
        var attack = new AttackState<StateEnum>(_view, _agent, ChangeTarget);

        idle.AddTransition(StateEnum.MOVE, move);
        idle.AddTransition(StateEnum.DEATH, dead);
        idle.AddTransition(StateEnum.ATTACK, attack);

        move.AddTransition(StateEnum.IDLE, idle);
        move.AddTransition(StateEnum.DEATH, dead);
        move.AddTransition(StateEnum.ATTACK, attack);

        dead.AddTransition(StateEnum.IDLE, idle);
        dead.AddTransition(StateEnum.MOVE, move);
        dead.AddTransition(StateEnum.ATTACK, attack);

        attack.AddTransition(StateEnum.IDLE, idle);
        attack.AddTransition(StateEnum.MOVE, move);
        attack.AddTransition(StateEnum.DEATH, dead);

        _fsm.SetInit(idle);
    }

    void InitializeTree()
    {

        ITreeNode idle = new ActionNode(() => _fsm.Transition(StateEnum.IDLE));
        ITreeNode move = new ActionNode(() => _fsm.Transition(StateEnum.MOVE));
        ITreeNode dead = new ActionNode(() => _fsm.Transition(StateEnum.DEATH));
        ITreeNode attack = new ActionNode(() => _fsm.Transition(StateEnum.ATTACK));

        ITreeNode qIsOnAttackRange = new QuestionNode(questionIsOnAttackRange, attack, move);
        ITreeNode qHasSeenFoe = new QuestionNode(questionHasSeenFoe, qIsOnAttackRange, idle);
        ITreeNode qIsAlive = new QuestionNode(health.IsAlive, qHasSeenFoe, dead);

        _root = qIsAlive;
    }

    bool questionHasSeenFoe()
    {
        //Debug.Log(_los.LOS(_target.transform));
        return _los.LOS(_target.transform);
    }

    bool questionIsOnAttackRange()
    {
        var los = _los;
        float range = _stats._minAttackRange;
        if (los.CheckRange(_target.transform, range))
            return true;
        else return false;
    }

    void LostTarget()
    {
        Debug.Log("perdi target, tengo 1,5 segundos");
        _searchTarget = StartCoroutine(TargetSearchTimer(1.5f));
    }

    IEnumerator TargetSearchTimer(float time)
    {
        yield return new WaitForSeconds(time);
        _agent.SetDestination(transform.position);
        _view.Walk(0);
    }

    void DashAttack()
    {
        StartCoroutine(DashAttackCoroutine());

    }

    IEnumerator DashAttackCoroutine()
    {
        int steps = 10;
        float stepTime = _stats._attackDuration / steps;
        float stepDistance = _stats._attackDistance / steps;

        Vector2 direction = _target.transform.position - transform.position;

        for (int i = 0; i < steps; i++)
        {

            transform.Translate(direction.normalized * stepDistance);
            yield return new WaitForSeconds(stepTime);
        }

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
        if (health.CurrentHealth > 0) return; // si no murio no hagas nada

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
