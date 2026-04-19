using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

// --- 1. DATA STRUCTURES ---
[Serializable]
public class FeatureCollection {
    public List<Feature> features;
}

[Serializable]
public class Feature {
    public Geometry geometry;
    public Properties properties;
}

[Serializable]
public class Geometry {
    public string type;
    public float[] coordinates; 
}

[Serializable]
public class Properties {
    public string sap_description;
    public string waterway_name;
}

// --- 2. THE EDITOR TOOL ---
// Ensure the filename is "LockJsonToMap.cs"
public class LockJsonToMap : EditorWindow
{
    private string filePath = "Assets/Canals/Locks/Llangollen Canal.json";

    // FIX: Changed JsonToWorldTool to LockJsonToMap to match the class name
    [MenuItem("Tools/Locks/Lock JSON Spawner")]
    public static void ShowWindow() => GetWindow<LockJsonToMap>("JSON Spawner");

    private void OnGUI() {
        filePath = EditorGUILayout.TextField("JSON Path", filePath);

        if (GUILayout.Button("Spawn Cubes")) {
            SpawnCubes();
        }
    }

    private void SpawnCubes() {
        string fullPath = Path.Combine(Application.dataPath, filePath.Replace("Assets/", ""));
        
        if (!File.Exists(fullPath)) {
            Debug.LogError("File not found at: " + fullPath);
            return;
        }

        string jsonContent = File.ReadAllText(fullPath).Trim();
        
        // FIX: If your JSON starts with '[', wrap it so Unity's JsonUtility can read it
        if (jsonContent.StartsWith("[")) {
            jsonContent = "{\"features\":" + jsonContent + "}";
        }

        try {
            FeatureCollection data = JsonUtility.FromJson<FeatureCollection>(jsonContent);

            if (data == null || data.features == null) {
                Debug.LogError("JSON parsing failed. Check if your JSON keys match the class variable names.");
                return;
            }


            // The one-liner to get the name
            string canalName = data.features[0].properties.waterway_name ?? data.features[0].properties.waterway_name;
            GameObject parent = new GameObject("Locks For " + canalName);
            Undo.RegisterCreatedObjectUndo(parent, "Spawn Cubes");

            foreach (var feature in data.features) {
                if (feature.geometry == null || feature.geometry.coordinates.Length < 2) continue;

                // Mapping JSON [0] to X and [1] to Z (Unity's floor plane)
                Vector3 position = new Vector3(feature.geometry.coordinates[0], 0, feature.geometry.coordinates[1]);
                
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = position;
                cube.transform.parent = parent.transform;
                
                // Use description if available, otherwise use waterway name or generic title
                string cubeName = feature.properties.sap_description;
                if (string.IsNullOrEmpty(cubeName)) cubeName = feature.properties.waterway_name;
                if (string.IsNullOrEmpty(cubeName)) cubeName = "MapPoint";
                
                cube.name = cubeName;
                
                Undo.RegisterCreatedObjectUndo(cube, "Spawn Cube");
            }
            
            Debug.Log($"Successfully spawned {data.features.Count} cubes.");
        }
        catch (Exception e) {
            Debug.LogError("Error parsing JSON: " + e.Message);
        }
    }
}