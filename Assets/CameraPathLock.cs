using UnityEngine;
using UnityEngine.Splines;

public class CameraPathLock : MonoBehaviour
{
    [Header("References")]
    public SplineContainer canalSpline; 
    
    [Header("Settings")]
    public float heightOffset = 30f; // Matches your clamp logic
    public bool useSmoothing = true;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (canalSpline == null) return;

        // 1. Get the parent's current position (where your WASD is moving it)
        Vector3 anchorPos = transform.parent.position;

        // 2. Find the closest point on the canal spline to that anchor
        SplineUtility.GetNearestPoint(canalSpline.Spline, anchorPos, out var nearestPoint, out var t);

        // 3. Create the target position (Locked X/Z from spline, Y from height)
        // Note: nearestPoint is in local space of the SplineContainer
        Vector3 worldNearest = canalSpline.transform.TransformPoint(nearestPoint);
        Vector3 targetPos = new Vector3(worldNearest.x, anchorPos.y, worldNearest.z);

        // 4. Apply to the Camera
        if (useSmoothing)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.position = targetPos;
        }

        // Optional: Keep the camera looking at the anchor point 
        // so the player's cursor/focus stays centered
        transform.LookAt(anchorPos);
    }
}