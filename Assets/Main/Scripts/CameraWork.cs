using UnityEngine;

public class CameraWork : MonoBehaviour
{ // un script simple en donde la camara sigue al jugador con un lerp entre su posicion, la posicion del jugador sumado el offset y una velocidad
    private Transform target;

    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothSpeed = 5f;

    public void SetPlayer(Transform playerTarget)
    {
        this.target = playerTarget;  // Asigna el jugador local
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}