using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;

public class BackToMainMenuSceneController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button backToMenu; // script para pantalla de victoria y derrota

    void Start()
    {
        backToMenu.onClick.AddListener(ReturnToMainMenu);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ReturnToMainMenu()
    {
        if (PhotonNetwork.OfflineMode) { PhotonNetwork.OfflineMode = false;} // preguntar profesor x esto para manejar "modo offline"

        if (PhotonNetwork.InRoom)
        {
            MainMenuStarter.hasRequestedJoinRoom = false; //  Reset antes de salir del Room
            RoomLeaver.Instance.LeaveRoom();
        }
        else
        {
            SceneLoader.LoadScene(ScenesEnum.MainMenu);
        }
    }
}
