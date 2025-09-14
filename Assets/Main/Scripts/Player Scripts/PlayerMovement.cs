using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPlayer
{
    #region  Variables
    [Header("Movment")]
    [Range(0, 10)]
    [SerializeField] private float movSpeed = 6f;
    private float horizontal;
    private float vertical;
    private Vector2 dir;

    [Header("Evade")]
    [Range(5, 20)]
    [SerializeField] private float evadeForce;
    private bool isEvading = false;
    [SerializeField] private int maxEvades = 2;  // Máximo de cargas de esquive
    private int currentEvades;  // Cargas disponibles
    private float evadeDuration = 0.2f;
    [SerializeField] private float evadeCooldown = 2f; // Tiempo de cooldown por carga de esquive

    private Vector2 mousePos;
    private Rigidbody2D rb;

    [Header("Interaction")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform interactPoint;
    [SerializeField] private GunHolderScript gunHolder;
    [SerializeField] private CameraWork cameraFollow;
    private HealthScript health;
    private Camera mainCamera;
    
    [Header("UI")]
    [SerializeField] private GameObject localHUD;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text mesenger;
    #endregion

    #region Metodos
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthScript>();
        currentEvades = maxEvades;
        mainCamera = Camera.main;

        if (photonView.IsMine)
        {
            cameraFollow = mainCamera.GetComponent<CameraWork>();
            localHUD.SetActive(true);

            if (cameraFollow != null)
            {
                cameraFollow.SetPlayer(transform);  // Asigna la cámara para el jugador local
            }
            else
            {
                Debug.LogError("No esta el script CameraWork en la main camera.");
            }

            photonView.RPC("RPC_SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName); // llamo al rpc para setearle el nombre al player
        }
        else
        {
            Camera camera = GetComponentInChildren<Camera>();

            if (camera != null)
            {
                camera.gameObject.SetActive(false); // Desactiva la cámara del jugador remoto
            }

            localHUD.SetActive(false);
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

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1) && currentEvades > 0 && !isEvading)
            {
                Evade();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                ONInteract();
            }

            ChangeGuns();

            if (Input.GetKeyDown(KeyCode.Escape)) // salir del juego
            {
                Application.Quit();
            }
            
            if (Input.GetKeyDown(KeyCode.P)) // salir de la room
            {
                Debug.Log("Sali de la room");
                RoomLeaver.Instance.LeaveRoom();
            }
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (isEvading)
            {
                return;
            }

            Move();
        }
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

    void ChangeGuns()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gunHolder.ChangeGun(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gunHolder.ChangeGun(1);
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

        if (currentEvades < maxEvades) // Solo se recargan si hay menos del maximo de cargas
        {
            currentEvades++;
        }
    }

    public float GetevadeTime()
    {
        return (float)currentEvades / maxEvades;
    }

    void Look()
    {
        if (!photonView.IsMine) return; // chequeo x las dudas

        if (mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void ONInteract()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(interactPoint.position, new Vector2(1f, 1f), 0f, interactableLayer);

        foreach (Collider2D items in colliders)
        {
            if (items.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
                Debug.Log("Toque: " + "<color=green>" + " " + interactable + "</color>");
            }
        }
    }
   
    [PunRPC]
    public void RPC_SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void LobbyMesage(string mesage)
    {
        mesenger.text = mesage;
    }

    public void GetDamage(int damage)
    {
        health.TakeDamage(damage);
    }
    #endregion
}
