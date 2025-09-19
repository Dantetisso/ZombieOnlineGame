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
    protected float nextFireTime;
    protected bool IsReloading;
    protected float lastFireTime;
    [HideInInspector] public GunEnum gunEnum;

    public event Action<int, int> OnAmmoChange;

    protected PhotonView playerPhotonView; // protegido para subclases
    private Coroutine flashCoroutine;
    #endregion

    #region Unity
    protected virtual void Start()
    {
        ammoClip = gunData._clipAmmo;
        currentAmmo = ammoClip;
        maxAmmo = gunData._maxAmmo;
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

    public virtual void Shoot()
    {
        if (IsReloading || currentAmmo <= 0) return;

        currentAmmo--;
        NotifyAmmoChange();

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

        PlayShootSound();
        ShowFlash();

        if (PhotonNetwork.IsConnected && playerPhotonView != null)
        {
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_PlayShootSound), RpcTarget.Others);
            playerPhotonView.RPC(nameof(PlayerGunSync.RPC_ShowMuzzleFlash), RpcTarget.Others);
        }
    }

    public virtual void Reload()
    {
        if (IsReloading || maxAmmo <= 0 || currentAmmo == ammoClip) return;

        IsReloading = true;

        int ammoNeeded = ammoClip - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, maxAmmo);

        currentAmmo += ammoToLoad;
        maxAmmo -= ammoToLoad;

        NotifyAmmoChange();
        IsReloading = false;
    }

    public virtual void GetAmmo() => maxAmmo += ammoClip;

    public virtual void FullReload()
    {
        currentAmmo = gunData._clipAmmo;
        maxAmmo = gunData._maxAmmo;
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

    protected void NotifyAmmoChange()
    {
        OnAmmoChange?.Invoke(currentAmmo, maxAmmo);
    }
    #endregion
}
