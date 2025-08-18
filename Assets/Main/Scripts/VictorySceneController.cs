using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictorySceneController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button backToMenu;

    void Start()
    {
        backToMenu.onClick.AddListener(ReturnToMainMenu);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ReturnToMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            MainMenuStarter.hasRequestedJoinRoom = false; // 🔹 Reset antes de salir del Room
            RoomLeaver.Instance.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("menu");
        }
    }

}