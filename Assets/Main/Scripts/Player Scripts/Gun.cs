using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public enum GunEnum
{
    Pistol,
    AutomaticRifle,
    Rifle,
    Shotgun
}

public abstract class Gun : MonoBehaviourPunCallbacks, IGun
{
    #region Variables
    [Header("Estadisticas del arma")]
    [SerializeField] protected GunStats gunData;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] public Sprite gunImage;
    [SerializeField] protected LayerMask zombieMask;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected SpriteRenderer muzzleFlash;

    private Vector2 mousePos;
    private Vector2 startPos;
    protected int currentAmmo;
    protected int maxAmmo;
    protected int ammoClip;
    public int _currentAmmo => currentAmmo;
    public int _maxAmmo => maxAmmo;
    private bool IsReloading;
    private float nextFireTime;
   [HideInInspector] public GunEnum gunEnum;

    public event Action<int, int> OnAmmoChange;
    private PhotonView playerPhotonView; // cacheo para no buscar cada disparo
    private Coroutine flashCoroutine;    // para controlar la corutina del flash
    #endregion

    #region Metodos de Unity

    protected virtual void Start()
    {
        // Inicializo munición
        ammoClip = gunData._clipAmmo;
        currentAmmo = ammoClip;
        maxAmmo = gunData._maxAmmo;
        gunEnum = gunData._gunType;

        OnAmmoChange?.Invoke(currentAmmo, maxAmmo);

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
    }
    #endregion

    #region Metodos
    public abstract void HandleShooting();
    public abstract void HandleReloading();

    public void Shoot()
    {
        if (IsReloading) return;

        currentAmmo--;
        OnAmmoChange?.Invoke(currentAmmo, maxAmmo);

        Vector2 dir = ((Vector2)mousePos - (Vector2)startPos).normalized;
        float range = Mathf.Min(Vector2.Distance(startPos, mousePos), gunData._range);

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, range, zombieMask);

        if (hit.collider && hit.distance <= range)
        {
            PhotonView targetPhotonView = hit.collider.GetComponent<PhotonView>();

            if (targetPhotonView && playerPhotonView)
            {
                // Llamo RPC de daño solo al MasterClient
                playerPhotonView.RPC(nameof(PlayerGunSync.RPC_MakeDamage), RpcTarget.MasterClient, targetPhotonView.ViewID, gunData._damage);
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

    public virtual void Reload()
    {
        if (IsReloading || maxAmmo <= 0 || currentAmmo == ammoClip)
            return;

        IsReloading = true;

        int ammoNeeded = ammoClip - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, maxAmmo);

        currentAmmo += ammoToLoad;
        maxAmmo -= ammoToLoad;
        OnAmmoChange?.Invoke(currentAmmo, maxAmmo);

        IsReloading = false;
    }

    public virtual void GetAmmo()
    {
        maxAmmo += ammoClip;
    }

    public virtual void FullReload()
    {
        currentAmmo = gunData._clipAmmo;
        maxAmmo = gunData._maxAmmo;
    }

    protected void NotifyAmmoChange()
    {
        OnAmmoChange?.Invoke(currentAmmo, maxAmmo);
    }

    public virtual void ResetFireState()
    {
        nextFireTime = Time.time; // para que no se dispare instantáneamente
                                  // si tu arma tiene flags de disparo, resetearlos aquí también
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
