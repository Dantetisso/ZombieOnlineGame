using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class HealthScript : MonoBehaviourPun
{
    [Header("Health")]
    public int maxHealth;
    [SerializeField] private int currentHealth;
    public int _currentHealth => currentHealth;
    [SerializeField] private bool IsPlayer;

    public event Action<int, int> OnHealthChanged;
    public static event Action<Player> OnPlayerDied;

    private Renderer _renderer;
    private Color _originalColor;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _renderer.material = new Material(_renderer.material);
            _originalColor = _renderer.material.color;
        }

        if (photonView.IsMine)
            photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine || isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, currentHealth, maxHealth);
        StartCoroutine(FlashRed());

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            HandleDeath();
        }
    }

    private IEnumerator FlashRed()
    {
        if (_renderer == null) yield break;

        _renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        _renderer.material.color = _originalColor;
    }

    public void InitHealth(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        isDead = false;
    }

    public void ResetHealth()
    {
        if (!photonView.IsMine) return;

        currentHealth = maxHealth;
        isDead = false;
        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.AllBuffered, currentHealth, maxHealth);
    }

    public bool IsAlive() => !isDead && currentHealth > 0;

    private void HandleDeath()
    {
        if (IsPlayer)
            photonView.RPC(nameof(RPC_PlayerDied), RpcTarget.AllBuffered);
        else
            photonView.RPC(nameof(RPC_ZombieDied), RpcTarget.MasterClient, photonView.ViewID);
    }

    [PunRPC]
    private void RPC_UpdateHealth(int newCurrent, int newMax)
    {
        currentHealth = newCurrent;
        maxHealth = newMax;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [PunRPC]
    private void RPC_PlayerDied()
    {
        OnPlayerDied?.Invoke(photonView.Owner);
    }

    [PunRPC]
    private void RPC_ZombieDied(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            FindObjectOfType<ZombieSpawner>().OnZombieDied(viewID);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
