using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

[EditorTool("XZ Spline Tool", typeof(SplineContainer))]
public class XZSplineTool : EditorTool
{
    private GUIContent m_IconContent;
    
    void OnEnable()
    {
        // Optional: Set a custom icon for the tool
        m_IconContent = EditorGUIUtility.IconContent("d_MoveTool");
    }
    
    public override GUIContent toolbarIcon
    {
        get
        {
            if (m_IconContent == null)
                m_IconContent = new GUIContent("XZ");
            m_IconContent.tooltip = "Edit splines locked to XZ plane (Y = 0)";
            return m_IconContent;
        }
    }
    
    public override void OnToolGUI(EditorWindow window)
    {
        // Get the selected Spline
        var splineContainer = target as SplineContainer;
        if (splineContainer == null) return;
        
        // Get the native spline
        var spline = splineContainer.Spline;
        if (spline == null) return;
        
        // Lock Y position when moving knots
        for (int i = 0; i < spline.Count; i++)
        {
            var knot = spline[i];
            var position = knot.Position;
            
            // Check if Y is not zero and fix it
            if (position.y != 0f)
            {
                Undo.RecordObject(splineContainer, "Lock Y Position");
                position.y = 0f;
                knot.Position = position;
                spline[i] = knot;
                EditorUtility.SetDirty(splineContainer);
            }
        }
        
        // Also handle tangent locking if needed
        for (int i = 0; i < spline.Count; i++)
        {
            var knot = spline[i];
            var tangentIn = knot.TangentIn;
            var tangentOut = knot.TangentOut;
            
            if (tangentIn.y != 0f || tangentOut.y != 0f)
            {
                Undo.RecordObject(splineContainer, "Lock Tangent Y");
                tangentIn.y = 0f;
                tangentOut.y = 0f;
                knot.TangentIn = tangentIn;
                knot.TangentOut = tangentOut;
                spline[i] = knot;
                EditorUtility.SetDirty(splineContainer);
            }
        }
    }
}