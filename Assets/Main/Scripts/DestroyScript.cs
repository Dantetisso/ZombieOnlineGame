using System.Collections;
using Photon.Pun;
using UnityEngine;

public class DestroyScript : MonoBehaviour
{
    [SerializeField] private float destructionDelay;

    private void Start()
    {
        Invoke(nameof(Deloy), destructionDelay);
    }

    void Deloy()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
