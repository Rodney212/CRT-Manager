using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Splines;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;
using UnityEngine.Splines;

namespace CanalSplineTool
{
    public class CanalJsonToSplines : EditorWindow
    {
        private string path = "Assets/Canals/Lengths/test1/";
        private bool isFolderMode = false;
        private bool generateCanalData = true;
        private bool generateSplineData = true;
        private int totalSplinesCreated = 0;
    

        private const string HandoverPrefabPath = "Assets/Prefabs/Handover Trigger.prefab";

        [MenuItem("Tools/Canals/Canal/JSON To Spline Generator")]
        public static void ShowWindow() => GetWindow<CanalJsonToSplines>("Canal Spline Generator");

        private void OnGUI()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            isFolderMode = EditorGUILayout.Toggle("Process Entire Folder?", isFolderMode);
            DrawPathField();

            GUILayout.Space(5);
            GUILayout.Label("Components", EditorStyles.boldLabel);
            generateCanalData = EditorGUILayout.Toggle("Generate Canal Data?", generateCanalData);
            generateSplineData = EditorGUILayout.Toggle("Generate Spline Data?", generateSplineData);

            GUILayout.Space(5);
            if (GUILayout.Button("Generate Hierarchy"))
                RunGeneration();

            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Triggers", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Triggers"))
                RunTriggerGeneration();
        }

        // ----------- UI -----------

        private void DrawPathField()
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(isFolderMode ? "Folder Path" : "File Path", path);

            if (GUILayout.Button("Browse", GUILayout.Width(60)))
                BrowseForPath();

            EditorGUILayout.EndHorizontal();
        }

        private void BrowseForPath()
        {
            string selected = isFolderMode
                ? EditorUtility.OpenFolderPanel("Select JSON Folder", "Assets", "")
                : EditorUtility.OpenFilePanel("Select JSON File", "Assets", "json");

            if (string.IsNullOrEmpty(selected)) return;

            path = selected.Contains(Application.dataPath)
                ? "Assets" + selected.Replace(Application.dataPath, "")
                : selected;
        }

        // ----------- Hierarchy Generation -----------

        private void RunGeneration()
        {
            var timer = Stopwatch.StartNew();
            totalSplinesCreated = 0;

            string fullPath = ResolvePath(path);
            var splinesRoot = GetOrCreateLevel("Splines", GetOrCreateLevel("Canal", null).transform);

            if (isFolderMode)
                ProcessFolder(fullPath, splinesRoot.transform);
            else
                ProcessSingleFile(fullPath, splinesRoot.transform);

            timer.Stop();
            Debug.Log($"<color=green>[CanalTool]</color> Done! {totalSplinesCreated} splines created in {timer.Elapsed.TotalSeconds:F2}s.");
        }

        private void ProcessFolder(string folderPath, Transform parent)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"[CanalTool] Folder not found: {folderPath}");
                return;
            }

            foreach (string file in Directory.GetFiles(folderPath, "*.json"))
                ProcessSingleFile(file, parent);
        }

        private void ProcessSingleFile(string filePath, Transform parent)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[CanalTool] File not found: {filePath}");
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string json = File.ReadAllText(filePath).Trim();
            var group = GetOrCreateLevel(fileName, parent);

            try
            {
                var features = ParseFeatures(json);
                if (features == null) return;

                var segments = BuildAllSegments(features, group.transform);
                LinkSegments(segments);

                if (generateCanalData)
                    AttachCanalData(group, fileName, segments.Count);

                totalSplinesCreated += segments.Count;
                Debug.Log($"[CanalTool] Finished '{fileName}' — {segments.Count} segments.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CanalTool] Error in '{fileName}': {e.Message}");
            }
        }

        // ----------- Trigger Generation -----------

        private void RunTriggerGeneration()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandoverPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[CanalTool] Could not find prefab at: {HandoverPrefabPath}");
                return;
            }

            var allSegments = FindAllSegments();
            if (allSegments.Count == 0)
            {
                Debug.LogWarning("[CanalTool] No SplineData components found in scene. Run Generate Hierarchy first.");
                return;
            }

            var timer = Stopwatch.StartNew();
            int count = 0;

            foreach (var segment in allSegments)
            {
                SpawnTrigger(prefab, segment);
                count++;
            }

            timer.Stop();
            Debug.Log($"<color=green>[CanalTool]</color> Spawned {count} triggers in {timer.Elapsed.TotalSeconds:F2}s.");
        }

        private List<SplineData> FindAllSegments()
        {
            return new List<SplineData>(GameObject.FindObjectsByType<SplineData>(FindObjectsSortMode.None));
        }

        private void SpawnTrigger(GameObject prefab, SplineData segment)
        {
            // find and destroy any existing trigger on this segment
            var existing = segment.transform.Find("Handover Trigger");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Handover Trigger";
            instance.transform.SetParent(segment.transform);
            instance.transform.position = segment.transform.position;

            var trigger = instance.GetComponent<HandoverTrigger>();
            if (trigger != null)
            {
trigger.spline = segment.GetComponent<SplineContainer>();
                trigger.SplineData = segment;
            }
            else
            {
                Debug.LogWarning($"[CanalTool] Prefab is missing HandoverTrigger component on segment: {segment.name}");
            }

            Undo.RegisterCreatedObjectUndo(instance, "Spawn Handover Trigger");
        }

        // ----------- Feature Parsing -----------

        private JArray ParseFeatures(string json)
        {
            var root = json.StartsWith("[")
                ? JObject.Parse("{\"features\":" + json + "}")
                : JObject.Parse(json);

            return root["features"] as JArray;
        }

        private List<SplineData> BuildAllSegments(JArray features, Transform parent)
        {
            var segments = new List<SplineData>();

            foreach (JObject feature in features)
            {
                var segment = TryBuildSplineFromFeature(feature, parent);
                if (segment != null)
                    segments.Add(segment);
            }

            return segments;
        }

        private SplineData TryBuildSplineFromFeature(JObject feature, Transform parent)
        {
            var geometry = feature["geometry"] as JObject;
            var coordinates = geometry?["coordinates"] as JArray;
            if (coordinates == null || coordinates.Count < 2) return null;

            var properties = feature["properties"] as JObject;
            string name = properties?["functionallocation"]?.ToString() ?? "Unknown_Loc";
            Vector3 startPos = ParseStartLocation(properties);

            var go = CreateSplineObject(name, parent, startPos);
            PopulateSpline(go.GetComponent<SplineContainer>().Spline, coordinates);

            if (generateSplineData)
                return AttachSplineData(go, properties);

            return null;
        }

        private Vector3 ParseStartLocation(JObject properties)
        {
            var startArray = properties?["startlocation"] as JArray;
            if (startArray == null || startArray.Count < 2) return Vector3.zero;

            return new Vector3((float)startArray[0], 0f, (float)startArray[1]);
        }

        private void PopulateSpline(Spline spline, JArray coordinates)
        {
            foreach (JArray coord in coordinates)
            {
                var localPos = new Vector3((float)coord[0], 0f, (float)coord[1]);
                spline.Add(new BezierKnot(localPos));
            }
        }

        // ----------- Component Attachment -----------

private SplineData AttachSplineData(GameObject go, JObject properties)
{
    var data = go.AddComponent<SplineData>();


            data.objectID           = properties?["OBJECTID"]?.ToObject<int>() ?? 0;
            data.functionalLocation = properties?["functionallocation"]?.ToString() ?? "";
            data.sapCanalCode       = properties?["sapcanalcode"]?.ToString() ?? "";
            data.canalName          = properties?["name"]?.ToString() ?? "";
            data.region             = properties?["region"]?.ToString() ?? "";
            data.sapWidth           = properties?["sapwidth"]?.ToString() ?? "";
            data.navStatus          = properties?["sapnavstatus"]?.ToString() ?? "";
            data.globalID           = properties?["globalid"]?.ToString() ?? "";
            data.lockQuantity       = properties?["lock_quantity"]?.ToObject<int>() ?? 0;
            data.thisSegment = go.GetComponent<SplineContainer>();

            return data;
        }



    private void LinkSegments(List<SplineData> segments)
    {
        if (!generateSplineData) return;

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].previousSegment = i > 0 ? segments[i - 1].GetComponent<SplineContainer>() : null;
            segments[i].nextSegment     = i < segments.Count - 1 ? segments[i + 1].GetComponent<SplineContainer>() : null;
        }
    }


        private void AttachCanalData(GameObject group, string fileName, int segmentCount)
        {
            var data = group.AddComponent<CanalData>();
            data.canalName     = fileName;
            data.totalSegments = segmentCount;
        }

        // ----------- Helpers -----------

        private GameObject CreateSplineObject(string name, Transform parent, Vector3 worldPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = worldPos;
            go.AddComponent<SplineContainer>();
            AttachSplineExtrude(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Spline");
            return go;
        }

        private GameObject GetOrCreateLevel(string name, Transform parent)
        {
            var existing = parent != null
                ? parent.Find(name)?.gameObject
                : GameObject.Find(name);

            if (existing != null) return existing;

            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }

        private string ResolvePath(string inputPath)
        {
            if (inputPath.StartsWith("Assets"))
                return Path.Combine(Application.dataPath, inputPath.Replace("Assets/", ""));

            return inputPath;
        }

        private void AttachSplineExtrude(GameObject go)
        {
            var extrude = go.AddComponent<SplineExtrude>();
            var splineContainer = go.GetComponent<SplineContainer>();

            var framingSplineGO = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Framing Spline.prefab"
            );
            var framingSpline = framingSplineGO != null 
                ? framingSplineGO.GetComponent<SplineContainer>() 
                : null;

            if (framingSpline == null)
                Debug.LogWarning("[CanalTool] Could not find Framing Spline prefab!");

            extrude.Container = splineContainer;
            //extrude.Profile = framingSpline;
            extrude.Sides = 26;
            extrude.SegmentsPerUnit = 0.6f;
            extrude.Radius = 1f;
            extrude.Range = new Vector2(0f, 1f);
            extrude.RebuildOnSplineChange = true;
        }
    }
}