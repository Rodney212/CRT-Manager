using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BoatUiController : MonoBehaviour
{
    public VisualTreeAsset uxmlDocument;
    private VisualElement _root;
    private VisualElement _container;

    void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        // Start hidden
        _root.style.display = DisplayStyle.None;
    }

    public void Display(BoatController boat)
    {
        BoatData data = boat.BoatData;
        _root.Clear();
        _container = uxmlDocument.Instantiate();
        _root.Add(_container);

        // This line connects the ScriptableObject to the UI fields via binding-path
        _root.Bind(new UnityEditor.SerializedObject(data));

        // Setup Close Button
        var closeBtn = _root.Q<Button>("Close-Button");
        closeBtn.clicked += Close;

        _root.style.display = DisplayStyle.Flex;
    }

    public void Close()
    {
        _root.style.display = DisplayStyle.None;
        _root.Unbind(); // Clean up binding
    }
}