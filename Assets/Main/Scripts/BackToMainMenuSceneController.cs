using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BackToMainMenuSceneController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button backToMenu; // botón para volver al menú

    void Start()
    {
        // Activar cursor siempre al iniciar la escena
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (backToMenu != null)
            backToMenu.onClick.AddListener(ReturnToMainMenu);
    }

    private void ReturnToMainMenu()
    {
        if (PhotonNetwork.OfflineMode) 
            PhotonNetwork.OfflineMode = false; // desactivar modo offline al volver al menú

        if (PhotonNetwork.InRoom)
        {
            MainMenuStarter.hasRequestedJoinRoom = false; // reset antes de salir del Room
            RoomLeaver.Instance.LeaveRoom();
        }
        else
        {
            SceneLoader.LoadScene(ScenesEnum.MainMenu);
        }
    }
}
