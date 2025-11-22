using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;

public class PlayerMovement : MonoBehaviourPunCallbacks, IDamageable
{
    #region  Variables
    [Header("Movement")]
    [SerializeField, Range(0, 10)] private float movSpeed;
    private float horizontal;
    private float vertical;
    private Vector2 dir;

    
    [Header("Evade")]
    [SerializeField] private int maxEvades;  // Máximo de cargas de esquive
    [SerializeField, Range(5, 20)] private float evadeForce;
    [SerializeField, Min(0.1f)] private float evadeDuration;
    [SerializeField, Min(0.1f)] private float evadeCooldown; // Tiempo de cooldown por carga de esquive
    private bool isEvading = false;
    private int currentEvades;  // Cargas disponibles

    private Rigidbody2D rb;

    [Header("Interaction")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform interactPoint;
    [SerializeField] private CameraWork cameraFollow;
    [SerializeField] private Gun[] guns;

    [Header("MeleeAttack")]
    [SerializeField] private GameObject attackFeedback;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackDuration;
    [SerializeField] private float attackCooldown;
    private float attackTimer;
    private float nextAttackTime;
    private bool IsAttacking;

    [Header("Grenade")]
    [SerializeField] private GameObject grenadeObject;
    [SerializeField] private Transform grenadePos;
    [SerializeField] private int maxGrenadeCount;
    [SerializeField] private float grenadeThrowForce;
    private int grenadeCount;
    private bool IsThrowing;

    private Gun activeGun;
    private HealthScript healthScript;
    private Camera mainCamera;

    [Header("UI")]
    [SerializeField] private GameObject localHUD;
    [SerializeField] private GameObject netWorkHUD;
    [SerializeField] private TMP_Text playerNameText;

    public event Action<Gun> OnChangeGun;
    public static event Action<int> OnPlayerDied;
    public event Action<int> OnChangeGrenade;
    #endregion

    #region Metodos
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthScript = GetComponent<HealthScript>();

        currentEvades = maxEvades;
        mainCamera = Camera.main;
        grenadeCount = maxGrenadeCount;
        OnChangeGrenade?.Invoke(grenadeCount);

        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            SetupLocalPlayer();

            var gun = GetComponentInChildren<Gun>();
            activeGun = gun;          // asigna el arma activa
            OnChangeGun?.Invoke(activeGun); // dispara evento a UI
        }
        else
        {
            SetupRemotePlayer();
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            dir = new Vector2(horizontal, vertical);

            Look();
            ChangeGuns();
            HandleInput();

            if (IsAttacking)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    attackFeedback.SetActive(false);
                    IsAttacking = false;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (isEvading) return;
            Move();
        }
    }

    private void SetupLocalPlayer()
    {
        cameraFollow = mainCamera.GetComponent<CameraWork>();

        localHUD.SetActive(true);
        netWorkHUD.SetActive(false);

        if (cameraFollow != null)
            cameraFollow.SetPlayer(transform);

        photonView.RPC("RPC_SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);

        // Inicializa UI local
        PlayerUIController ui = localHUD.GetComponent<PlayerUIController>();
        Gun gun = GetComponentInChildren<Gun>();

        if (ui != null && gun != null) ui.InitGun(gun);
    }

    private void SetupRemotePlayer()
    {
        Camera camera = GetComponentInChildren<Camera>();
        if (camera != null) camera.gameObject.SetActive(false);

        localHUD.SetActive(false);
        netWorkHUD.SetActive(true);
    }

    void Look()
    {
        if (!photonView.IsMine) return;
        if (mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Move()
    {
        rb.velocity = dir.normalized * movSpeed;
    }

    void Evade()
    {
        if (rb.velocity != Vector2.zero)
        {
            isEvading = true;
            currentEvades--;
            rb.velocity = Vector2.zero;
            rb.AddForce(dir.normalized * evadeForce, ForceMode2D.Impulse);

            StartCoroutine(EndEvade());
            StartCoroutine(ReloadEvade());
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E)) ONInteract();

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1)) && currentEvades > 0 && !isEvading)
            Evade();

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (Input.GetKeyDown(KeyCode.P)) RoomLeaver.Instance.LeaveRoom();

        if (Input.GetKeyDown(KeyCode.V)) MeleeAttack();

        if (Input.GetKeyDown(KeyCode.G)) Grenade();
    }

    void ChangeGuns()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeGunWithSync(GunEnum.AutomaticRifle);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeGunWithSync(GunEnum.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeGunWithSync(GunEnum.Shotgun);
    }

    private void ChangeGunWithSync(GunEnum type)
    {
        photonView.RPC(nameof(RPC_ChangeGun), RpcTarget.AllBuffered, type);
    }

    void ChangeGun(GunEnum type)
    {
        Gun newGun = null;

        foreach (var gun in guns)
        {
            if (gun == null) continue;
            gun.gameObject.SetActive(gun.gunEnum == type);
            if (gun.gunEnum == type) newGun = gun;
        }

        if (newGun != null)
        {
            activeGun = newGun;
            OnChangeGun?.Invoke(activeGun);

            if (photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                var ui = GetComponentInChildren<PlayerUIController>();
                ui?.InitGun(activeGun);
            }
        }
    }

    void Grenade()
    {
        if (grenadeCount > 0)
        {
            // Crear la granada en la posición del jugador
            GameObject grenade = PhotonNetwork.Instantiate(grenadeObject.name, grenadePos.position, Quaternion.identity);

            // Obtener el Rigidbody2D
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();
            if (grenadeRb == null) return;

            // Calcular la dirección hacia el mouse
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 throwDir = (mouseWorldPos - grenadePos.position).normalized;

            // Aplicar fuerza a la granada
            grenadeRb.AddForce(throwDir * grenadeThrowForce, ForceMode2D.Impulse);
            grenadeCount--;
            OnChangeGrenade?.Invoke(grenadeCount);
        }
    }

    IEnumerator EndEvade()
    {
        yield return new WaitForSeconds(evadeDuration);
        isEvading = false;
    }

    IEnumerator ReloadEvade()
    {
        yield return new WaitForSeconds(evadeCooldown);
        if (currentEvades < maxEvades) currentEvades++;
    }

    public float GetEvadeTime()
    {
        return (float)currentEvades / maxEvades;
    }

    private void ONInteract()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(interactPoint.position, new Vector2(1f, 1f), 0f, interactableLayer);

        foreach (Collider2D items in colliders)
        {
            if (items.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
                Debug.Log("Toque: <color=green>" + interactable + "</color>");
            }
        }
    }

    private void MeleeAttack()
    {
        if (Time.time < nextAttackTime) return;

        // Feedback visual
        attackFeedback.SetActive(true);
        IsAttacking = true;
        attackTimer = attackDuration;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRange, attackRange), 0f, enemyLayer);

        // Ataca enemigos
        foreach (var coll in colliders)
        {
            if (coll == null) continue;

            if (PhotonNetwork.IsConnected && photonView.IsMine)
            {
                var enemy = coll.GetComponent<PhotonView>();
                
                if (enemy != null)
                {
                    photonView.RPC(nameof(RPC_MeleeAttack), RpcTarget.MasterClient, enemy.ViewID, attackDamage);
                }
            }
            else
            {
                if (coll.TryGetComponent(out IDamageable dmg)) 
                {
                    dmg.TakeDamage(attackDamage);
                }
            }
        }

        // Feedback visual solo 1 vez
        if (photonView.IsMine && PhotonNetwork.IsConnected)
            photonView.RPC(nameof(RPC_PlayAttackFeedback), RpcTarget.Others, attackDuration);

        nextAttackTime = Time.time + attackCooldown;
    }
    #endregion

    #region RPCs
    [PunRPC]
    void RPC_ChangeGun(GunEnum type)
    {
        ChangeGun(type);
    }

    [PunRPC]
    public void RPC_SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    [PunRPC]
    public void RPC_MeleeAttack(int enemyViewID, int dmg)
    {
        PhotonView enemyPhoton = PhotonView.Find(enemyViewID);
        if (enemyPhoton != null && enemyPhoton.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(dmg);
        }
    }

    [PunRPC]
    void RPC_PlayAttackFeedback(float duration)
    {
        attackFeedback.SetActive(true);
        StartCoroutine(DisableFeedbackAfter(duration));
    }

    IEnumerator DisableFeedbackAfter(float time)
    {
        yield return new WaitForSeconds(time);
        attackFeedback.SetActive(false);
    }
    #endregion
    public void TakeDamage(int damage)
    {
        healthScript.GetDamage(damage);
        if (healthScript._currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
      photonView.RPC(nameof(RPC_PlayerDied), RpcTarget.AllBuffered, photonView.ViewID);
    }

    [PunRPC]
    private void RPC_PlayerDied(int ID)
    {
        OnPlayerDied?.Invoke(ID);
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRange, attackRange, 0));
    }
    
    #endregion
}
