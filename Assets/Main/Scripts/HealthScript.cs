using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))] // me aseguro que el objeto tenga el photon view
public class HealthScript : MonoBehaviourPun
{
    [Header("Health")]
    public int maxHealth = 100;
    [SerializeField] private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    public event Action<int, int> OnHealthChanged;

    private Renderer _renderer;
    private Color _originalColor;

    void Start()
    {
        _currentHealth = maxHealth;

        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _renderer.material = new Material(_renderer.material); // instancia propia
            _originalColor = _renderer.material.color; // guardar color original
        }

        if (photonView.IsMine) // solo el dueño le manda a todos para actualizar su vida
        {
            photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, _currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine) return;  // si no esta sincronizado corta aca

        _currentHealth -= damage; // x si acaso me aseguro que la vida nunca sea negativa (osea no baje de 0)
        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, _currentHealth, maxHealth);
        Debug.Log("took damage");

        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        _renderer.material.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        _renderer.material.color = _originalColor;
    }

    public void ResetHealth() // metodo x si quiero reiniciar la vida
    {
        if (!photonView.IsMine) return;

        _currentHealth = maxHealth;
        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.AllBuffered, _currentHealth, maxHealth);
    }

    public bool IsAlive()
    {
        return _currentHealth > 0;
    }

    [PunRPC] // rpc para actualizar la vida con evento
    void RPC_UpdateHealth(int newCurrent, int newMax)
    {
        _currentHealth = newCurrent;
        maxHealth = newMax;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    [PunRPC]
    public void RPC_TakeDamage(int dmg)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= dmg;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0) { Destroy(this); }

    }
}
