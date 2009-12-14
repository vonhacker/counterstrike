using UnityEngine;

using UnityEditor;
using System.Collections;

public class AddChild : ScriptableObject
{
    [MenuItem("GameObject/Rename")]
    static void MenuAddChild()
    {
        
        Transform[] transforms = Selection.GetTransforms(SelectionMode.Unfiltered);
        int i = 0;
        foreach (Transform transform in transforms)
        {
            i++;
            if (transform.GetComponent<Box>() != null)
                transform.name = i.ToString();
        }
    }
}