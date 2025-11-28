using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootLockerBootstrap : MonoBehaviour
{
    public static bool SessionStarted {  get; private set; }

   // [SerializeField] string playerIdentifier = "";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartLogin();
    }

/*    void StartGuest()
    {
        LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
        {
            if (!response.success)
            {
                Debug.LogError("Fallo al iniciar LOOTLOCKER");
                return;
            }
            SessionStarted = true;
            Debug.Log("Conectado a LOOTLOCKER");
        });
    }*/

    void StartLogin()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.LogError("Fallo al iniciar LOOTLOCKER");
                return;
            }
            SessionStarted = true;
            Debug.Log("Conectado a LOOTLOCKER");
            PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
        });
    }
}
