using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.Splines;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CanalSplineTool
{
    public class CanalJsonToSplines : EditorWindow
    {
        private string filePath = "Assets/Canals/Lengths/test1/Llangollen Canal.json";

        [MenuItem("Tools/Canals/JSON Canal Spline Generator")]
        public static void ShowWindow() => GetWindow<CanalJsonToSplines>("Canal Spline Generator");

        private void OnGUI()
        {
            filePath = EditorGUILayout.TextField("JSON Path", filePath);
            if (GUILayout.Button("Generate Canal Splines")) GenerateSplines();
        }

 private void GenerateSplines()
        {
            string fullPath = Path.Combine(Application.dataPath, filePath.Replace("Assets/", ""));
            if (!File.Exists(fullPath)) { Debug.LogError("File not found: " + fullPath); return; }

            string jsonContent = File.ReadAllText(fullPath).Trim();
            JObject root;

            if (jsonContent.StartsWith("["))
                root = JObject.Parse("{\"features\":" + jsonContent + "}");
            else
                root = JObject.Parse(jsonContent);

            JArray features = (JArray)root["features"];
            if (features == null || features.Count == 0) return;

            GameObject parent = new GameObject("Canal_Splines");
            Undo.RegisterCreatedObjectUndo(parent, "Create Canal Splines");

            foreach (JObject feature in features)
            {
                JObject geometry = (JObject)feature["geometry"];
                JObject properties = (JObject)feature["properties"];
                if (geometry == null) continue;

                string type = geometry["type"]?.ToString();
                JArray coordinates = (JArray)geometry["coordinates"];
                if (coordinates == null) continue;

                string canalCode = properties?["sapcanalcode"]?.ToString() ?? "??";
                string funcLoc = properties?["functionallocation"]?.ToString() ?? "??";
                string baseName = $"{canalCode}-{funcLoc}";

                // Determine if we are dealing with one line or multiple lines
                if (type == "LineString")
                {
                    CreateSplineObject(parent.transform, baseName, coordinates);
                }
                else if (type == "MultiLineString")
                {
                    for (int i = 0; i < coordinates.Count; i++)
                    {
                        JArray subLine = (JArray)coordinates[i];
                        CreateSplineObject(parent.transform, $"{baseName}_{i}", subLine);
                    }
                }
            }

            Selection.activeGameObject = parent;
        }

        // Helper to keep the main loop clean
        private void CreateSplineObject(Transform parent, string name, JArray coordinateList)
        {
            if (coordinateList.Count < 2) return;

            GameObject splineGo = new GameObject(name);
            splineGo.transform.SetParent(parent);
            Undo.RegisterCreatedObjectUndo(splineGo, "Create Canal Spline");

            SplineContainer container = splineGo.AddComponent<SplineContainer>();
            Spline spline = container.Spline;

            foreach (JToken pt in coordinateList)
            {
                // Ensure we are looking at numbers, not another nested array
                if (pt is JArray coord && coord.Count >= 2)
                {
                    float x = (float)coord[0];
                    float z = (float)coord[1];
                    spline.Add(new BezierKnot(new Vector3(x, 0f, z)));
                }
            }
        }
    }
}