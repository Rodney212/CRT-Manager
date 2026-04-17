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

            // Handle raw array or FeatureCollection
            if (jsonContent.StartsWith("["))
                root = JObject.Parse("{\"features\":" + jsonContent + "}");
            else
                root = JObject.Parse(jsonContent);

            JArray features = (JArray)root["features"];

            if (features == null || features.Count == 0)
            {
                Debug.LogError("No features found in JSON.");
                return;
            }

            GameObject parent = new GameObject("Canal_Splines");
            Undo.RegisterCreatedObjectUndo(parent, "Create Canal Splines");

            int splineCount = 0;

            foreach (JObject feature in features)
            {
                JObject geometry = (JObject)feature["geometry"];
                JObject properties = (JObject)feature["properties"];

                if (geometry == null) continue;

                JArray coordinates = (JArray)geometry["coordinates"];
                if (coordinates == null || coordinates.Count < 2) continue;

                string canalCode = properties?["sapcanalcode"]?.ToString() ?? "??";
                string funcLoc = properties?["functionallocation"]?.ToString() ?? "??";
                string canalName = $"{canalCode}-{funcLoc}";

                GameObject splineGo = new GameObject(canalName);
                splineGo.transform.SetParent(parent.transform);
                Undo.RegisterCreatedObjectUndo(splineGo, "Create Canal Spline");

                SplineContainer container = splineGo.AddComponent<SplineContainer>();
                Spline spline = container.Spline;

                foreach (JToken coordToken in coordinates)
                {
                    JArray coord = (JArray)coordToken;
                    if (coord.Count < 2) continue;

                    float x = (float)coord[0];
                    float z = (float)coord[1];
                    spline.Add(new BezierKnot(new Vector3(x, 0f, z)));
                }

                Debug.Log($"  '{canalName}': {spline.Count} knots");
                splineCount++;
            }

            Debug.Log($"Generated {splineCount} canal splines under 'Canal_Splines'.");
            Selection.activeGameObject = parent;
        }
    }
}