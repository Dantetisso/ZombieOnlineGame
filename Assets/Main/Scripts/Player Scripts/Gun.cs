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
    private AudioSource audioSource;
    [SerializeField] protected SpriteRenderer muzzleFlash;

    private Vector2 mousePos;
    private Vector2 startPos;

    protected int ammoClip;
    public int CurrentAmmo { get; private set; }
    public int MaxAmmo { get; private set; }
    public event Action<int, int> OnAmmoChange;
    protected float nextFireTime;
    protected bool IsReloading;
    protected float lastFireTime;
    private float nextEmptyClickTime;
    private float emptyClickRate = 0.3f;
    [HideInInspector] public GunEnum gunEnum;


    protected PhotonView playerPhotonView; // protegido para subclases
    private Coroutine flashCoroutine;
    
    #region Unity
    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ammoClip = gunData._clipSize;
        CurrentAmmo = ammoClip;
        MaxAmmo = gunData._maxAmmo;
        gunEnum = gunData._gunType;

        NotifyAmmoChange();

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;

        playerPhotonView = transform.root.GetComponent<PhotonView>();
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine) return;

        HandleShooting();
        HandleReloading();
    }
    #endregion

    #region Métodos
    public abstract void HandleShooting();
    public abstract void HandleReloading();
    protected virtual void HandleInput() { }

    public virtual void Shoot()
    {
        if (IsReloading) return;

        if (CurrentAmmo <= 0)
        {
            if (Time.time >= nextEmptyClickTime)    // para controlar tiempo de reproduccion del sonido sin balas
            {
                PlayEmptyShootSound();
                nextEmptyClickTime = Time.time + emptyClickRate;
            }
            return;
        }

        if (Time.time < nextFireTime) return;       // Cadencia de fuego

        CurrentAmmo--;
        NotifyAmmoChange();
        nextFireTime = Time.time + gunData._fireFate;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)shootPoint.position).normalized;
        float range = Mathf.Min(Vector2.Distance(shootPoint.position, mousePos), gunData._range);

        RaycastHit2D hit = Physics2D.Raycast(shootPoint.position, dir, range, zombieMask);

        if (hit.collider && hit.distance <= range)
        {
            PhotonView targetPhotonView = hit.collider.GetComponent<PhotonView>();

            if (PhotonNetwork.IsConnected)
            {
                if (playerPhotonView != null && targetPhotonView != null)
                {
                    playerPhotonView.RPC(nameof(PlayerGunSync.RPC_MakeDamage),
                        RpcTarget.MasterClient, targetPhotonView.ViewID, gunData._damage);
                }
            }
            else
            {
                if (hit.collider.TryGetComponent(out HealthScript enemyHealth))
                {
                    enemyHealth.TakeDamage(gunData._damage);
                }
            }
        }

        // -------------------- Feedback Local -------------------- //

        PlayShootSound();
        ShowFlash();

        // -------------------- Feedback Red -------------------- //

        if (PhotonNetwork.IsConnected && playerPhotonView != null)
        {
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_PlayShootSound), RpcTarget.Others);
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_ShowMuzzleFlash), RpcTarget.Others);
        }
    }

    public virtual void Reload()
    {
        if (IsReloading || MaxAmmo <= 0 || CurrentAmmo == ammoClip) return;

        IsReloading = true;

        int ammoNeeded = ammoClip - CurrentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, MaxAmmo);

        CurrentAmmo += ammoToLoad;
        MaxAmmo -= ammoToLoad;

        PlayReloadSound();
        NotifyAmmoChange();
        IsReloading = false;
    }

    public virtual void GetAmmo() => MaxAmmo += ammoClip;

    public virtual void FullReload()
    {
        CurrentAmmo = gunData._clipSize;
        MaxAmmo = gunData._maxAmmo;
        NotifyAmmoChange();
    }

    public void ShowFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(MuzzleFlashRoutine());
    }

    IEnumerator MuzzleFlashRoutine()
    {
        if (muzzleFlash == null) yield break;
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.enabled = false;
    }

    public void PlayShootSound()
    {
        audioSource?.PlayOneShot(gunData._shootSound);
    }

    public void PlayEmptyShootSound()
    {
        audioSource?.PlayOneShot(gunData._shootEmptySound);
    }

    public void PlayReloadSound()
    {
        audioSource?.PlayOneShot(gunData._reloadSound);
    }

    protected void NotifyAmmoChange()
    {
        OnAmmoChange?.Invoke(CurrentAmmo, MaxAmmo);
    }
    #endregion
}
#endregion