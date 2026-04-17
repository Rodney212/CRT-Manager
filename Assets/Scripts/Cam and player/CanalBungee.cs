using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics; // Splines use Mathematics library

public class CanalBungee : MonoBehaviour
{
    public SplineContainer canalSpline;
    public float snapStrength = 10f; 


void LateUpdate()
    {
        if (canalSpline == null) return;

        // 1. Get our current world position
        float3 worldPos = transform.position;

        // 2. Convert to Spline Local Space
        float3 localPos = canalSpline.transform.InverseTransformPoint(worldPos);

        // 3. Find the nearest point (X, Y, and Z)
        SplineUtility.GetNearestPoint(canalSpline.Spline, localPos, out float3 nearestLocal, out float t);

        // 4. Convert back to World Space
        Vector3 nearestWorld = canalSpline.transform.TransformPoint(nearestLocal);

        // 5. The Magic Logic: 
        // We want to snap to the spline's X and Z, 
        // AND we want to snap to the spline's Y (the hill/lock height)
        // PLUS whatever height the user has zoomed to.
        
        // Let's assume your 'Ball' should be at the water level (nearestWorld.y)
        // If your zoom script moves the BALL's Y, we use that as an offset.
        
        float targetX = Mathf.Lerp(transform.position.x, nearestWorld.x, Time.deltaTime * snapStrength);
        float targetY = Mathf.Lerp(transform.position.y, nearestWorld.y, Time.deltaTime * snapStrength);
        float targetZ = Mathf.Lerp(transform.position.z, nearestWorld.z, Time.deltaTime * snapStrength);

        transform.position = new Vector3(targetX, targetY, targetZ);
    }
}