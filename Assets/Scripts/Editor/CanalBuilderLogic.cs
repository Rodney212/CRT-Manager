using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class CanalBuilderLogic : MonoBehaviour
{
    [Header("Spline Settings")]
    public List<Vector3> controlPoints = new List<Vector3>();
    public int splineResolution = 20;
    
    [Header("Visualization")]
    public bool showSpline = true;
    public bool showPoints = true;
    public bool showNormals = false;
    public Color splineColor = Color.cyan;
    public Color pointColor = Color.red;
    
    [Header("Canal Settings")]
    public float canalWidth = 2f;
    public float canalDepth = 0.5f;
    
    private LineRenderer lineRenderer;
    private List<Vector3> splinePoints = new List<Vector3>();
    private bool needsUpdate = true;
    
    public int PointCount => controlPoints.Count;
    public float CanalLength => CalculateCanalLength();
    
    private void OnEnable()
    {
        InitializeLineRenderer();
        needsUpdate = true;
    }
    
    private void Update()
    {
        if (needsUpdate)
        {
            UpdateSplinePoints();
            UpdateVisualization();
            needsUpdate = false;
        }
    }
    
    private void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 0;
    }
    
    public void UpdateVisualization()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = showSpline;
            if (showSpline && splinePoints.Count > 1)
            {
                lineRenderer.positionCount = splinePoints.Count;
                lineRenderer.SetPositions(splinePoints.ToArray());
                lineRenderer.startColor = splineColor;
                lineRenderer.endColor = splineColor;
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }
        
        SceneView.RepaintAll();
    }
    
    public void StartNewCanal()
    {
        controlPoints.Clear();
        splinePoints.Clear();
        needsUpdate = true;
        
        // Add starting point at current scene view camera position
        if (SceneView.lastActiveSceneView != null)
        {
            Vector3 cameraPos = SceneView.lastActiveSceneView.camera.transform.position;
            Vector3 startPoint = new Vector3(cameraPos.x, 0, cameraPos.z);
            AddPoint(startPoint);
        }
        else
        {
            AddPoint(Vector3.zero);
        }
        
        needsUpdate = true;
        UpdateVisualization();
    }
    
    public void AddPoint(Vector3 worldPosition)
    {
        Vector3 lockedPoint = LockToXZPlane(worldPosition);
        controlPoints.Add(lockedPoint);
        needsUpdate = true;
    }
    
    public void AddPointAtMousePosition()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        AddPoint(mouseWorldPos);
    }
    
    public void InsertPointAtMousePosition()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 lockedPoint = LockToXZPlane(mouseWorldPos);
        
        if (controlPoints.Count < 2)
        {
            controlPoints.Add(lockedPoint);
        }
        else
        {
            // Find closest segment to insert point
            int closestIndex = FindClosestSegmentIndex(lockedPoint);
            if (closestIndex >= 0 && closestIndex < controlPoints.Count - 1)
            {
                controlPoints.Insert(closestIndex + 1, lockedPoint);
            }
            else
            {
                controlPoints.Add(lockedPoint);
            }
        }
        
        needsUpdate = true;
    }
    
    public void RemoveLastPoint()
    {
        if (controlPoints.Count > 0)
        {
            controlPoints.RemoveAt(controlPoints.Count - 1);
            needsUpdate = true;
        }
    }
    
    public void RemovePoint(int index)
    {
        if (index >= 0 && index < controlPoints.Count)
        {
            controlPoints.RemoveAt(index);
            needsUpdate = true;
        }
    }
    
    public void ClearAllPoints()
    {
        controlPoints.Clear();
        splinePoints.Clear();
        needsUpdate = true;
        UpdateVisualization();
    }
    
    public Vector3 GetPoint(int index)
    {
        if (index >= 0 && index < controlPoints.Count)
        {
            return controlPoints[index];
        }
        return Vector3.zero;
    }
    
    public void SelectPoint(int index)
    {
        if (index >= 0 && index < controlPoints.Count)
        {
            Selection.activeGameObject = gameObject;
            // Highlight the point in scene view
            SceneView.lastActiveSceneView.Frame(new Bounds(controlPoints[index], Vector3.one * 2), false);
        }
    }
    
    private Vector3 LockToXZPlane(Vector3 position)
    {
        return new Vector3(position.x, 0, position.z);
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }
        }
        return Vector3.zero;
    }
    
    private int FindClosestSegmentIndex(Vector3 point)
    {
        float minDistance = float.MaxValue;
        int closestIndex = -1;
        
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 closestPoint = ClosestPointOnSegment(controlPoints[i], controlPoints[i + 1], point);
            float distance = Vector3.Distance(point, closestPoint);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }
    
    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }
    
    private void UpdateSplinePoints()
    {
        splinePoints.Clear();
        
        if (controlPoints.Count < 2) return;
        
        // Generate Catmull-Rom spline points
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = controlPoints[Mathf.Max(0, i - 1)];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = controlPoints[Mathf.Min(controlPoints.Count - 1, i + 2)];
            
            for (int j = 0; j <= splineResolution; j++)
            {
                float t = j / (float)splineResolution;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                point = LockToXZPlane(point); // Ensure Y is 0
                splinePoints.Add(point);
            }
        }
    }
    
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        
        Vector3 result = 0.5f * ((2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
        
        return result;
    }
    
    private float CalculateCanalLength()
    {
        float length = 0;
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            length += Vector3.Distance(splinePoints[i], splinePoints[i + 1]);
        }
        return length;
    }
    
    public void SnapPointsToGrid(float gridSize = 1f)
    {
        for (int i = 0; i < controlPoints.Count; i++)
        {
            Vector3 point = controlPoints[i];
            point.x = Mathf.Round(point.x / gridSize) * gridSize;
            point.z = Mathf.Round(point.z / gridSize) * gridSize;
            controlPoints[i] = point;
        }
        needsUpdate = true;
    }
    
    public void GenerateCanalMesh()
    {
        if (splinePoints.Count < 2)
        {
            Debug.LogWarning("Not enough spline points to generate canal mesh");
            return;
        }
        
        GameObject canalMesh = new GameObject("Canal_Mesh");
        canalMesh.transform.SetParent(transform);
        
        MeshFilter meshFilter = canalMesh.AddComponent<MeshRenderer>() != null ? 
            canalMesh.GetComponent<MeshFilter>() : canalMesh.AddComponent<MeshFilter>();
        
        if (meshFilter == null) meshFilter = canalMesh.AddComponent<MeshFilter>();
        
        MeshRenderer meshRenderer = canalMesh.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = canalMesh.AddComponent<MeshRenderer>();
        
        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        
        float halfWidth = canalWidth / 2;
        
        for (int i = 0; i < splinePoints.Count; i++)
        {
            Vector3 forward;
            if (i == 0)
                forward = (splinePoints[i + 1] - splinePoints[i]).normalized;
            else if (i == splinePoints.Count - 1)
                forward = (splinePoints[i] - splinePoints[i - 1]).normalized;
            else
                forward = (splinePoints[i + 1] - splinePoints[i - 1]).normalized;
            
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            
            Vector3 leftPoint = splinePoints[i] - right * halfWidth;
            Vector3 rightPoint = splinePoints[i] + right * halfWidth;
            
            vertices.Add(leftPoint);
            vertices.Add(rightPoint);
            
            float u = i / (float)(splinePoints.Count - 1);
            uv.Add(new Vector2(0, u));
            uv.Add(new Vector2(1, u));
        }
        
        // Create triangles
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            int baseIndex = i * 2;
            
            // Triangle 1
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            
            // Triangle 2
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 2);
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;
        
        Material defaultMaterial = new Material(Shader.Find("Standard"));
        defaultMaterial.color = new Color(0.2f, 0.6f, 0.8f, 0.8f);
        meshRenderer.material = defaultMaterial;
        
        Debug.Log($"Canal mesh generated with {vertices.Count} vertices");
    }
    
    public void ExportPointsToFile()
    {
        string path = EditorUtility.SaveFilePanel("Export Canal Points", "", "canal_points", "json");
        if (string.IsNullOrEmpty(path)) return;
        
        PointsData data = new PointsData();
        data.points = controlPoints.Select(p => new SerializableVector3(p)).ToArray();
        
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(path, json);
        
        Debug.Log($"Points exported to: {path}");
    }
    
    public void CenterCameraOnCanal()
    {
        if (controlPoints.Count == 0) return;
        
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in controlPoints)
        {
            center += point;
        }
        center /= controlPoints.Count;
        
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.LookAt(center);
        }
    }
    
    // Drawing gizmos for visual feedback in scene view
    private void OnDrawGizmos()
    {
        if (!showPoints) return;
        
        Gizmos.color = pointColor;
        foreach (Vector3 point in controlPoints)
        {
            Gizmos.DrawSphere(point, 0.2f);
        }
        
        if (showNormals && splinePoints.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < splinePoints.Count; i++)
            {
                Vector3 forward;
                if (i == 0)
                    forward = (splinePoints[i + 1] - splinePoints[i]).normalized;
                else if (i == splinePoints.Count - 1)
                    forward = (splinePoints[i] - splinePoints[i - 1]).normalized;
                else
                    forward = (splinePoints[i + 1] - splinePoints[i - 1]).normalized;
                
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                Gizmos.DrawLine(splinePoints[i], splinePoints[i] + right);
            }
        }
    }
    
    [System.Serializable]
    public class PointsData
    {
        public SerializableVector3[] points;
    }
    
    [System.Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;
        
        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}