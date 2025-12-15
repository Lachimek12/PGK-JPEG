using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmartImageObject))]
public class SmartImageObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SmartImageObject smartImage = (SmartImageObject)target;
        
        EditorGUILayout.LabelField("Smart Image Object", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUI.BeginChangeCheck();
        
        Sprite newSprite = (Sprite)EditorGUILayout.ObjectField("Original Sprite", smartImage.OriginalSprite, typeof(Sprite), false);
        Texture2D newTexture = (Texture2D)EditorGUILayout.ObjectField("Original Texture", smartImage.OriginalTexture, typeof(Texture2D), false);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(smartImage, "Change Smart Image");
            
            if (newSprite != null)
            {
                smartImage.OriginalSprite = newSprite;
            }
            else if (newTexture != null)
            {
                smartImage.OriginalTexture = newTexture;
            }
            
            EditorUtility.SetDirty(smartImage);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Working Copy", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("The working copy is automatically created from the original. Image operations modify the copy, not the original.", MessageType.Info);
        
        if (GUILayout.Button("Reset to Original"))
        {
            Undo.RecordObject(smartImage, "Reset to Original");
            smartImage.ResetToOriginal();
            EditorUtility.SetDirty(smartImage);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Changes to the working copy will automatically update all slides using this Smart Image Object.", MessageType.Info);
    }
}

