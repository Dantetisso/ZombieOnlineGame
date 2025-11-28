using LootLocker.Requests;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static int leaderboardID = 32066;

    public static void AddScore(int addedScore, string leaderboardKey, System.Action<bool> onDone = null)
    {
        string playerID = PlayerPrefs.GetString("PlayerID", "");    // uso el playerprefs del unity para obtener al jugador

        if (string.IsNullOrEmpty(playerID))
        {
            Debug.LogError("PlayerID no encontrado, no se puede enviar score.");
            return;
        }

        // Obtengo el score del player de la leaderboard
        LootLockerSDKManager.GetMemberRank(leaderboardKey, playerID, (response) =>
        {
            if (!response.success)
            {
                Debug.LogError("Error al obtener score actual.");
                return;
            }

            int currentScore = response.score;  // score anterior
            int newScore = currentScore + addedScore;   // nuevo score, que es la suma del anterior + el actual

            // Sube el nuevo score
            LootLockerSDKManager.SubmitScore(playerID, newScore, leaderboardKey, (submitResponse) =>
            {
                if (!submitResponse.success)
                {
                    Debug.LogError("No se pudo enviar el score");
                    return;
                }

                Debug.Log("Se envio el score");
            });
        });
    }
}
