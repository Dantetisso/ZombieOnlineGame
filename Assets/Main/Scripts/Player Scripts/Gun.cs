using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Gun : MonoBehaviourPunCallbacks, IGun
{
    #region Variables
    [SerializeField] GunStats gunData;
    [SerializeField] Transform shootPoint;
    [SerializeField] private Text ammoCount;
    [SerializeField] private Text maxAmmoCount;
    [SerializeField] private LayerMask zombieMask;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpriteRenderer muzzleFlash;

    private Vector2 mousePos;
    private Vector2 startPos;
    private int currentAmmo;
    private int maxAmmo;
    private int ammoClip;
    private bool IsReloading;
    private float nextFireTime;

    private PhotonView playerPhotonView; // cacheo para no buscar cada disparo
    private Coroutine flashCoroutine;    // para controlar la corutina del flash
    #endregion

    #region Metodos de Unity
    void Start()
    {
        // Inicializo munición
        ammoClip = gunData._clipAmmo;
        currentAmmo = ammoClip;
        maxAmmo = gunData._maxAmmo;
        ammoCount.text = currentAmmo.ToString();
        maxAmmoCount.text = maxAmmo.ToString();

        // ------------------------ Apago el flash al inicio ------------------------ //
        if (muzzleFlash != null)
            muzzleFlash.enabled = false;

        // obtengo el PhotonView del player (porque el arma es hijo del objeto player donde tengo el photon view)
        playerPhotonView = transform.root.GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = shootPoint.position;

        HandleShooting();
        HandleReloading();

        ammoCount.text = currentAmmo.ToString();
        maxAmmoCount.text = maxAmmo.ToString();
    }
    #endregion

    #region Metodos
    private void HandleShooting()
    {
        if (gunData._IsAutomatic)
        {
            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                nextFireTime = Time.time + 1f / gunData._fireFate;
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
    }

    private void HandleReloading()
    {
        if (maxAmmo > 0 && Input.GetKeyDown(KeyCode.R))
            Reload();
    }

    public void Shoot()
    {
        if (IsReloading) return;
        currentAmmo--;

        Vector2 dir = ((Vector2)mousePos - (Vector2)startPos).normalized;
        float range = Mathf.Min(Vector2.Distance(startPos, mousePos), gunData._range);

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, range, zombieMask);

        if (hit.collider && hit.distance <= range)
        {
            PhotonView targetPhotonView = hit.collider.GetComponent<PhotonView>();

            if (targetPhotonView && playerPhotonView)
            {
                // Llamo RPC de daño solo al MasterClient
                playerPhotonView.RPC(nameof(PlayerGunSync.RPC_MakeDamage),
                                     RpcTarget.MasterClient,
                                     targetPhotonView.ViewID,
                                     gunData._damage);
            }
        }

        //------------------------------- Feedback Local ------------------------------- //
        // Se hace local para evitar delay y se ve en la propia cámara
        PlayShootSound();
        ShowFlash();

        //------------------------------- Feedback Red ------------------------------- //
        // Solo se llama a RPC una vez por disparo para otros jugadores
        if (playerPhotonView != null)
        {
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_PlayShootSound), RpcTarget.Others);
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_ShowMuzzleFlash), RpcTarget.Others);
        }
    }

    public void Reload()
    {
        if (IsReloading || maxAmmo <= 0 || currentAmmo == ammoClip)
            return;

        IsReloading = true;

        int ammoNeeded = ammoClip - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, maxAmmo);

        currentAmmo += ammoToLoad;
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

    IEnumerator MuzzleFlashRoutine()
    {
        if (muzzleFlash == null) yield break;

        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.1f);   // duracion del flash
        muzzleFlash.enabled = false;
    }

    public void ShowFlash()
    {
        // Detengo corutina anterior si estaba corriendo
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(MuzzleFlashRoutine());
    }

    public void PlayShootSound()
    {
        audioSource.PlayOneShot(gunData._shootSound);
    }
    #endregion
}
