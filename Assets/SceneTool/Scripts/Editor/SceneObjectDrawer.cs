using UnityEditor;
using UnityEngine;

namespace SceneTool
{
    [CustomPropertyDrawer(typeof(SceneObject))]
    public class SceneObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");

            EditorGUI.PropertyField(position, sceneAssetProperty, label);

            // TODO: warning helpbox for not included scene buildindex
        }
    }
}
