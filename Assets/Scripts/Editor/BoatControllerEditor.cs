using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoatController))]
public class BoatControllerEditor : Editor
{
    private BoatController boat;
    private SerializedProperty currentSegment;
    private SerializedProperty queuedSegment;
    private SerializedProperty pendingSegment;
    private SerializedProperty progress;
    private SerializedProperty speed;
    
    private void OnEnable()
    {
        boat = (BoatController)target;
        
        // Cache serialized properties for better performance
        currentSegment = serializedObject.FindProperty("currentSegment");
        queuedSegment = serializedObject.FindProperty("queuedSegment");
        pendingSegment = serializedObject.FindProperty("pendingSegment");
        progress = serializedObject.FindProperty("progress");
        speed = serializedObject.FindProperty("speed");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawPathingSection();
        EditorGUILayout.Space(10);
        
        DrawMovementSection();
        EditorGUILayout.Space(10);
        

        EditorGUILayout.Space(10);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.Space(10);
        
        
        serializedObject.ApplyModifiedProperties();
        
        // Real-time updates (like progress bar)
        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }
    
private void DrawPathingSection()
{
    EditorGUILayout.LabelField("Pathing", EditorStyles.boldLabel);
    
    // Column headers
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label("", GUILayout.Width(15)); // indent
    GUILayout.Label("Current", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
    GUILayout.Label("Queued", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
    GUILayout.Label("Pending", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
    EditorGUILayout.EndHorizontal();
    
    // Spline fields row
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label("Spline:", GUILayout.Width(50));
    
    // Current - green tint if assigned
    Color originalColor = GUI.backgroundColor;
    if (boat.currentSegment != null)
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f); // light green
    EditorGUILayout.PropertyField(currentSegment, GUIContent.none, GUILayout.Width(100));
    GUI.backgroundColor = originalColor;
    
    // Queued - green tint if assigned
    if (boat.queuedSegment != null)
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
    EditorGUILayout.PropertyField(queuedSegment, GUIContent.none, GUILayout.Width(100));
    GUI.backgroundColor = originalColor;
    
    // Pending - green tint if assigned
    if (boat.pendingSegment != null)
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
    EditorGUILayout.PropertyField(pendingSegment, GUIContent.none, GUILayout.Width(100));
    GUI.backgroundColor = originalColor;
    
    EditorGUILayout.EndHorizontal();
    
    // Validation warnings
    if (boat.currentSegment == null)
    {
        EditorGUILayout.HelpBox("No current segment assigned! Boat cannot move.", MessageType.Error);
    }
    
    EditorGUILayout.Space(5);
    
    // Other pathing fields
    EditorGUILayout.PropertyField(serializedObject.FindProperty("lockData"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("BoatData"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("juntionData"));
}
    
    private void DrawMovementSection()
    {
        EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(speed);
        
        // Progress slider with percentage
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Progress");
        float newProgress = EditorGUILayout.Slider(boat.progress, 0f, 1f);
        if (newProgress != boat.progress)
        {
            boat.progress = newProgress;
            EditorUtility.SetDirty(boat);
        }
        EditorGUILayout.LabelField($"{boat.progress * 100:F1}%", GUILayout.Width(45));
        EditorGUILayout.EndHorizontal();
        
        // Visual progress bar
        Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
        EditorGUI.ProgressBar(progressRect, boat.progress, $"Spline Progress");
    }
    

}