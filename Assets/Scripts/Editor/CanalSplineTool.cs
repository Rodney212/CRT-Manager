using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine.Splines;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;

namespace CanalSplineTool
{
    public class CanalJsonToSplines : EditorWindow
    {
        private string path = "Assets/Canals/Lengths/test1/";
        private bool isFolderMode = false;
        private int totalSplinesCreated = 0;

        [MenuItem("Tools/Canals/Canal/JSON To Spline Generator")]
        public static void ShowWindow() => GetWindow<CanalJsonToSplines>("Canal Spline Generator");

        private void OnGUI()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            isFolderMode = EditorGUILayout.Toggle("Process Entire Folder?", isFolderMode);

            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(isFolderMode ? "Folder Path" : "File Path", path);
            
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = isFolderMode 
                    ? EditorUtility.OpenFolderPanel("Select JSON Folder", "Assets", "") 
                    : EditorUtility.OpenFilePanel("Select JSON File", "Assets", "json");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.Contains(Application.dataPath))
                        path = "Assets" + selectedPath.Replace(Application.dataPath, "");
                    else
                        path = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Hierarchy")) RunGeneration();
        }

        private void RunGeneration()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            totalSplinesCreated = 0;

            string fullPath = path.StartsWith("Assets") 
                ? Path.Combine(Application.dataPath, path.Replace("Assets/", "")) 
                : path;

            GameObject canalRoot = GetOrCreateLevel("Canal", null);
            GameObject splinesRoot = GetOrCreateLevel("Splines", canalRoot.transform);

            if (isFolderMode)
            {
                if (!Directory.Exists(fullPath)) { Debug.LogError($"[CanalTool] Directory not found: {fullPath}"); return; }
                string[] files = Directory.GetFiles(fullPath, "*.json");
                foreach (string file in files) ProcessJsonFile(file, splinesRoot.transform);
            }
            else
            {
                if (!File.Exists(fullPath)) { Debug.LogError($"[CanalTool] File not found: {fullPath}"); return; }
                ProcessJsonFile(fullPath, splinesRoot.transform);
            }

            timer.Stop();
            Debug.Log($"<color=green>[CanalTool]</color> Generated {totalSplinesCreated} total splines in {timer.Elapsed.TotalSeconds:F2}s.");
        }

        private void ProcessJsonFile(string filePath, Transform splinesParent)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string jsonContent = File.ReadAllText(filePath).Trim();

            GameObject canalNameGroup = GetOrCreateLevel(fileName, splinesParent);

            try
            {
                JObject root = jsonContent.StartsWith("[") 
                    ? JObject.Parse("{\"features\":" + jsonContent + "}") 
                    : JObject.Parse(jsonContent);

                JArray features = (JArray)root["features"];
                if (features == null) return;

                foreach (JObject feature in features)
                {
                    JObject geometry = (JObject)feature["geometry"];
                    if (geometry == null) continue;

                    JArray coordinates = (JArray)geometry["coordinates"];
                    if (coordinates == null || coordinates.Count < 2) continue;

                    JObject properties = (JObject)feature["properties"];
                    string funcLoc = properties?["functionallocation"]?.ToString() ?? "Unknown_Loc";
                    
                    // 1. Get the start location from JSON
                    Vector3 startPos = Vector3.zero;
                    JArray startLocArray = (JArray)properties?["startlocation"];
                    if (startLocArray != null && startLocArray.Count >= 2)
                    {
                        startPos = new Vector3((float)startLocArray[0], 0f, (float)startLocArray[1]);
                    }

                    // 2. Create the spline object and MOVE IT to the start location
                    GameObject splineGo = new GameObject(funcLoc);
                    splineGo.transform.SetParent(canalNameGroup.transform);
                    splineGo.transform.position = startPos;
                    Undo.RegisterCreatedObjectUndo(splineGo, "Create Spline");

                    SplineContainer container = splineGo.AddComponent<SplineContainer>();
                    Spline spline = container.Spline;

                    // 3. Add the knots
                    foreach (JToken coordToken in coordinates)
                    {
                        JArray coord = (JArray)coordToken;
                        float jsonX = (float)coord[0];
                        float jsonZ = (float)coord[1];

                        // We want the IN-ENGINE LOCAL VALUE to match the JSON VALUE exactly.
                        // So we just pass the JSON values straight in.
                        Vector3 jsonBasedLocalPos = new Vector3(jsonX, 0f, jsonZ);
                        spline.Add(new BezierKnot(jsonBasedLocalPos));
                    }

                    totalSplinesCreated++;
                }
                Debug.Log($"[CanalTool] Finished '{fileName}'.");
            }
            catch (Exception e) { Debug.LogError($"[CanalTool] Error in {fileName}: {e.Message}"); }
        }

        private GameObject GetOrCreateLevel(string name, Transform parent)
        {
            GameObject go = parent != null ? parent.Find(name)?.gameObject : GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                if (parent != null) go.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            }
            return go;
        }
    }
}