using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightMono : MonoBehaviour
{
    public float range;
    public float angle;
    public LayerMask obsMask;
    public bool CheckRange(Transform target)
    {
        return CheckRange(target, range);
    }
    public bool CheckRange(Transform target, float range)
    {
        //A->B
        //B-A
        //A: Self
        //B: Target
        //return Vector3.Distance(self.position, target.position) <= range;

        Vector2 dir = target.position - transform.position;
        float distance = dir.magnitude;
        return distance <= range;
    }
    public bool CheckAngle(Transform target)
    {
        return CheckAngle(target, transform.up);
    }
    public bool CheckAngle(Transform target, Vector2 front)
    {
        Vector2 dir = target.position - transform.position;
        float angleToTarget = Vector2.Angle(front, dir);
        return angleToTarget <= angle / 2;
    }
    public bool CheckView(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        return !Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, obsMask);
    }
    public bool LOS(Transform target)
    {
        return CheckRange(target)
            && CheckAngle(target)
            && CheckView(target);
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
