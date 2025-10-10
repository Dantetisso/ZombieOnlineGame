using System;
using System.Collections;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class HealthScript : MonoBehaviourPun
{
    [Header("Health")]
    public int maxHealth;
    private int currentHealth;
    public int _currentHealth => currentHealth;

    public event Action<int, int> OnHealthChanged;

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

    public void GetDamage(int damage)
    {
        if (!photonView.IsMine || isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, currentHealth, maxHealth);
        photonView.RPC(nameof(RPC_DamageFeedback), RpcTarget.All);

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
        }
    }

    private IEnumerator DamageFeedback()
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

    [PunRPC]
    private void RPC_DamageFeedback()
    {
        StartCoroutine(DamageFeedback());
    }

    [PunRPC]
    private void RPC_UpdateHealth(int newCurrent, int newMax)
    {
        currentHealth = newCurrent;
        maxHealth = newMax;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

}
