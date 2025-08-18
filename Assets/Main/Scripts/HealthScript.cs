using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))] // me aseguro que el objeto tenga el photon view
public class HealthScript : MonoBehaviourPun
{
    [Header("Health")]
    public int maxHealth = 100;

    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;

    public event Action<int, int> OnHealthChanged;

    private Renderer _renderer;
    private Color _originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _renderer.material = new Material(_renderer.material); // instancia propia
            _originalColor = _renderer.material.color; // guardar color original
        }

        if (photonView.IsMine) // solo el dueño le manda a todos para actualizar su vida
        {
            photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine) return;  // si no esta sincronizado corta aca

        currentHealth -= damage; // x si acaso me aseguro que la vida nunca sea negativa (osea no baje de 0)
        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, currentHealth, maxHealth);
        Debug.Log("took damage");

        StartCoroutine(FlashRed());
    }

    private void RPC_FlashRed()
    {
        if (_renderer != null)
        {
            StopCoroutine(nameof(FlashRed)); // evitar superposición si recibe daño muy rápido
            StartCoroutine(FlashRed());
        }
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

        currentHealth = maxHealth;
        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.AllBuffered, currentHealth, maxHealth);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    [PunRPC] // rpc para actualizar la vida con evento
    void RPC_UpdateHealth(int newCurrent, int newMax)
    {
        currentHealth = newCurrent;
        maxHealth = newMax;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    [PunRPC]
    public void RPC_TakeDamage(int dmg)
    {
        if (currentHealth <= 0) return;

        currentHealth -= dmg;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) { Destroy(this); }

    }
}
