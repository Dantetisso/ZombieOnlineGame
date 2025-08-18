using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuStarter : MonoBehaviourPunCallbacks
{
    public static bool hasRequestedJoinRoom = false; // bool para chequear si no estoy queriendo meterme a una escena

    [SerializeField] private InputField playerNameInput;
    [SerializeField] private Button connectButton;

    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true; // lo vuelvo true para evitar problemas
        hasRequestedJoinRoom = false;

        PhotonNetwork.NickName = "";
        playerNameInput.text = "";

        connectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    void OnConnectButtonClicked()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName)) // se supone que chequea si tenes nickname, si no tenes no te deberia dejar haacer nada
        {
            Debug.LogWarning("sin nickname no entras.");
            return;
        }


        if (!PhotonNetwork.IsConnected) // si no estoy conectado al server pongo el nickname, me conecto y me mete a la room
        {
            ConnectionManager.Instance.SetNickName(playerNameInput.text);
            ConnectionManager.Instance.ConnectToServer(() =>
            {
                hasRequestedJoinRoom = true; 
                PhotonNetwork.JoinRandomOrCreateRoom();
            });
        }
        else
        {
            ConnectionManager.Instance.SetNickName(playerNameInput.text);
            hasRequestedJoinRoom = true;
            PhotonNetwork.JoinRandomOrCreateRoom();
        }
    }

    public override void OnJoinedRoom() // se me a la room si antes no hubo otro intento de "acceso"
    {
        if (hasRequestedJoinRoom)
        {
            hasRequestedJoinRoom = false; // lo pongo en falso y cargo la escena
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            Debug.LogWarning("Se entró a una Room sin querer.");
        }
    }
}
