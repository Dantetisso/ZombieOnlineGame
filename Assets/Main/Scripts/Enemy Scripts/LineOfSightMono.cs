using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightMono : MonoBehaviour
{
    public float range;
    public float angle;
    public LayerMask obsMask; // Usar la layer del jugador

    public bool CheckRange(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        return dir.magnitude <= range;
    }

    public bool CheckAngle(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        float angleToTarget = Vector2.Angle(transform.up, dir);
        return angleToTarget <= angle / 2;
    }

    public bool CheckView(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        return !Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, obsMask);
    }

    public bool LOS(Transform target)
    {
        return CheckRange(target) && CheckAngle(target) && CheckView(target);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, angle / 2) * transform.up * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, -angle / 2) * transform.up * range);
    }
}
