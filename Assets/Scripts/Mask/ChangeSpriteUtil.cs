#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChangeSpriteUtil : MonoBehaviour
{
    public SpriteMaskInteraction smiTo;
    public void Change()
    {
        var children = GetComponentsInChildren<SpriteRenderer>();
        foreach (var i in children)
        {
            i.maskInteraction = smiTo;
        }
    }
    
    [CustomEditor(typeof(ChangeSpriteUtil))]
    public class ChangeSpriteUtilEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            ChangeSpriteUtil c = (ChangeSpriteUtil)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("m"))
            {
                c.Change();
            }
        }
    }
}
#endif
