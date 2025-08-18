using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

public class Gun : MonoBehaviourPunCallbacks, IGun
{
    #region Variables
    [SerializeField] GunStats gunData;
    [SerializeField] Transform shootPoint;
    [SerializeField] private Text ammoCount;
    [SerializeField] private Text maxAmmoCount;
    [SerializeField] private LayerMask zombieMask;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject muzzleFlash;

    private Vector2 mousePos;
    private Vector2 startPos;
    private MarketScript market;
    private int currentAmmo;
    private int maxAmmo;
    private int ammoClip;
    private int shotsFired;
    private bool IsReloading;
    private bool isShooting;
    private float nextFireTime;
    #endregion

    #region Metodos de Unity
    void Start()
    {
        // market = FindObjectOfType<MarketScript>();
        ammoClip = gunData._clipAmmo;
        currentAmmo = ammoClip;
        maxAmmo = gunData._maxAmmo;
        ammoCount.text = currentAmmo.ToString();
        maxAmmoCount.text = maxAmmo.ToString();
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPos = shootPoint.position;

            if (gunData._IsAutomatic)
            {
                if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime && currentAmmo > 0)
                {
                    nextFireTime = Time.time + 1f / gunData._fireFate; // limito la velocidad de disparo
                    Shoot();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && currentAmmo > 0)
                {
                    Shoot();
                }
            }

            if (maxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }

            ammoCount.text = currentAmmo.ToString();
            maxAmmoCount.text = maxAmmo.ToString();
        }
    }
    #endregion

    #region Metodos
    public void Shoot()
    {
        if (IsReloading) return;
        currentAmmo--;

        Vector2 dir = ((Vector2)mousePos - (Vector2)startPos).normalized;
        float range = Mathf.Min(Vector2.Distance(startPos, mousePos), gunData._range);

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, range, zombieMask);

        if (hit.collider && hit.distance <= range) // si le pego a un collider y esta dentro del rango.....
        {
            PhotonView targetPhotonView = hit.collider.GetComponent<PhotonView>(); // si le pegue a un objecto que tiene el photonview
            PhotonView playerPhotonView = transform.root.GetComponent<PhotonView>(); // busco el photon view del padre (en este caso el jugador xq las armas son hijos)

            if (targetPhotonView && playerPhotonView) // si encontre el view del objetivo, en este caso zombie y el del padre (el jugador)
            {
                playerPhotonView.RPC(nameof(PlayerGunSync.RPC_MakeDamage), RpcTarget.MasterClient, targetPhotonView.ViewID, gunData._damage); // llamo al rpc del gunsync para dañar a los zombies
            }
        }

        //------------------------------- Feedback Local ------------------------------- //
        // lo hago local para evitar el delay del server y que desde mi camara se vea y escuche que estoy disparando
        PlayShootSound();
        StartCoroutine(MuzzleFlashRoutine());

        //------------------------------- Feedback Servidor ------------------------------- //
        PhotonView pv = transform.root.GetComponent<PhotonView>(); // vuelvo a buscar el view del jugador para llamar a los rpcs de feedback (flash y gun shoot)
        pv.RPC(nameof(PlayerGunSync.RPC_PlayShootSound), RpcTarget.Others); // y  solo se los aplico a los otros jugadores 
        pv.RPC(nameof(PlayerGunSync.RPC_ShowMuzzleFlash), RpcTarget.Others);    // porque ellos tienen que saber que estoy disparando
    }

    public void Reload()
    {
        if (IsReloading || maxAmmo <= 0 || currentAmmo == ammoClip)
        {
            return; // No recarga si ya está recargando, no hay balas, o el cargador está lleno
        }

        IsReloading = true;

        int ammoNeeded = ammoClip - currentAmmo; // Cuántas balas faltan para llenar el cargador
        int ammoToLoad = Mathf.Min(ammoNeeded, maxAmmo); // Carga solo lo que haya disponible

        currentAmmo += ammoToLoad; // añade las balas al cargador y resta de la reserva
        maxAmmo -= ammoToLoad;

        IsReloading = false;
    }

    public void GetAmmo()
    {
        maxAmmo += ammoClip;
    }

    public void FullReload()
    {
        currentAmmo = gunData._clipAmmo;
        maxAmmo = gunData._maxAmmo;
    }

    IEnumerator MuzzleFlashRoutine() // corutina para prender y apagar el flash al disparar
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);   // prendo el flash pasa el tiempo y lo apago 
        muzzleFlash.SetActive(false);
    }

    public void ShowFlash() // llamo a la corutina 
    {
        StartCoroutine(MuzzleFlashRoutine());
    }

    public void PlayShootSound() // metodo que reproduce el sonido de disparo
    {
        audioSource.PlayOneShot(gunData._shootSound);
    }
    #endregion
    
}
