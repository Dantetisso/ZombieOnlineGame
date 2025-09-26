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
    [SerializeField] protected LayerMask zombieMask;
    
    [Header("Feedback Visual")]
    [SerializeField] protected SpriteRenderer muzzleFlash;
    private AudioSource audioSource;

    private Vector2 mousePos;
    private Vector2 startPos;

    private int ammoClip;
    public int CurrentAmmo { get; private set; }
    public int MaxAmmo { get; private set; }
    public event Action<int, int> OnAmmoChange;
    private float nextFireTime;
    private bool IsReloading;
    private bool hasAmmo;
    private float lastFireTime;
    // Cooldown sonido sin balas
    private float nextEmptyClickTime;
    private float emptyClickRate = 0.4f;
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
        hasAmmo = true;

        NotifyAmmoChange();

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;

        playerPhotonView = transform.root.GetComponent<PhotonView>();
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine) return;

        if (gunData._IsAutomatic)
        {
            if (Input.GetKey(KeyCode.Mouse0)) TryShoot();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) TryShoot();
        }

        if (MaxAmmo > 0 && Input.GetKeyDown(KeyCode.R)) Reload();
    }
    #endregion

    #region Métodos
    protected virtual bool CanShoot() { return !IsReloading && Time.time >= nextFireTime; }

    protected virtual void TryShoot()
    {
        if (!CanShoot()) return;

        if (CurrentAmmo > 0)
        {
            Shoot();
            Debug.Log("<Color=yellow>" + name + "</color> dispara");
        }
        else
        {
            // Cooldown para el sonido de emptyshoot
            if (Time.time >= nextEmptyClickTime)
            {
                PlayEmptyShootSound();
                nextEmptyClickTime = Time.time + emptyClickRate;
            }
        }
        
    }

    public virtual void Shoot()
    {
        if (IsReloading) return;

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

        hasAmmo = CurrentAmmo < 0;
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

        hasAmmo = CurrentAmmo > 0;
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