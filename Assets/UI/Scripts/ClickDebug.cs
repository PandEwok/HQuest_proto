using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// TEMPORARY: put on any GameObject. Logs what is under the mouse on every left click,
/// from the top-most object to the bottom-most. Delete it when the bug is found.
public class ClickDebugger : MonoBehaviour
{
    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || EventSystem.current == null) return;
        if (!mouse.leftButton.wasPressedThisFrame) return;

        var data = new PointerEventData(EventSystem.current) { position = mouse.position.ReadValue() };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0) { Debug.Log("ClickDebugger: nothing under the mouse"); return; }

        string text = "ClickDebugger (top first): ";
        foreach (var r in results) text += GetPath(r.gameObject.transform) + "  |  ";
        Debug.Log(text);
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}