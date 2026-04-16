using UnityEngine;
using UnityEngine.Splines;

public class CanalPathConstraint : MonoBehaviour
{
    [Header("References")]
    public SplineContainer canalSpline;

    [Header("Settings")]
    public bool isLockEnabled = true;

    void LateUpdate()
    {
        if (!isLockEnabled || canalSpline == null) return;

        // 1. Get current position (after CameraController has moved it in Update)
        Vector3 currentPos = transform.position;

        // 2. Find the closest point on the spline
        // We use the spline's local space and convert it to world space
        SplineUtility.GetNearestPoint(canalSpline.Spline, currentPos, out var nearestPointLocal, out var t);
        Vector3 nearestPointWorld = canalSpline.transform.TransformPoint(nearestPointLocal);

        // 3. Apply the constraint
        // We keep the original Y (so your Zoom/height logic still works)
        // but force X and Z to the canal path
        transform.position = new Vector3(nearestPointWorld.x, currentPos.y, nearestPointWorld.z);
    }
}