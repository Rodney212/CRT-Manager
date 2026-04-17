using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions; // Required for stripping letters
using UnityEngine.Splines;

namespace CanalSplineTool 
{
    [Serializable]
    public class FeatureCollection { public List<FeatureData> features; }

    [Serializable]
    public class FeatureData { 
        public GeometryData geometry; 
        public PropertyData properties; 
    }

    [Serializable]
    public class GeometryData { public float[] coordinates; }

    [Serializable]
    public class PropertyData { 
        public string sap_func_loc; // Using this for sorting now
        public string sap_description; 
    }

    public class LockJsonToSpline : EditorWindow
    {
        private string filePath = "Assets/data.json";

        [MenuItem("Tools/Locks/JSON Spline Generator")]
        public static void ShowWindow() => GetWindow<LockJsonToSpline>("Spline Generator");

        private void OnGUI() {
            filePath = EditorGUILayout.TextField("JSON Path", filePath);
            if (GUILayout.Button("Generate Spline from Func Loc")) GenerateSpline();
        }

        private void GenerateSpline() {
            string fullPath = Path.Combine(Application.dataPath, filePath.Replace("Assets/", ""));
            if (!File.Exists(fullPath)) { Debug.LogError("File not found!"); return; }

            string jsonContent = File.ReadAllText(fullPath).Trim();
            if (jsonContent.StartsWith("[")) jsonContent = "{\"features\":" + jsonContent + "}";

            FeatureCollection data = JsonUtility.FromJson<FeatureCollection>(jsonContent);

            if (data == null || data.features == null) {
                Debug.LogError("Data failed to parse.");
                return;
            }

            // --- THE NEW SORTING LOGIC ---
            var sortedFeatures = data.features.OrderBy(f => {
                if (string.IsNullOrEmpty(f.properties.sap_func_loc)) return 0;
                
                // 1. Remove anything that isn't a number (removes WN and -)
                string numericOnly = Regex.Replace(f.properties.sap_func_loc, "[^0-9]", "");
                
                // 2. Convert to a number for sorting (e.g., "014002" becomes 14002)
                if (long.TryParse(numericOnly, out long result)) {
                    return result;
                }
                return 0;
            }).ToList();

            // Create Spline
            GameObject splineGo = new GameObject("Waterway_Spline_Sequential");
            SplineContainer container = splineGo.AddComponent<SplineContainer>();
            Spline spline = container.Spline;
            Undo.RegisterCreatedObjectUndo(splineGo, "Create Sequential Spline");

            foreach (var feature in sortedFeatures) {
                if (feature.geometry?.coordinates == null || feature.geometry.coordinates.Length < 2) continue;
                
                Vector3 pos = new Vector3(feature.geometry.coordinates[0], 0, feature.geometry.coordinates[1]);
                spline.Add(new BezierKnot(pos));
            }

            Debug.Log($"Spline created with {spline.Count} points sorted by Functional Location.");
            Selection.activeGameObject = splineGo;
        }
    }
}