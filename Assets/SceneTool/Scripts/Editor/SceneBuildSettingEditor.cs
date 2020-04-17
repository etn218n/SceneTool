using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace SceneTool
{
    [CustomEditor(typeof(SceneBuildSetting))]
    public class SceneBuildSettingEditor : Editor
    {
        #region Fields & Properties
        private SceneBuildSetting  sceneBuildSetting;
        private SerializedProperty preloadSceneProperty;
        private SerializedProperty scenesProperty;

        private GUIContent scenesGUIContent = new GUIContent(text: "Scene(s)", tooltip: "Scene(s) to add to Build Setting.");
        private GUIContent preloadSceneGUIContent = new GUIContent(text: "Preload Scene", tooltip: "First scene to be loaded when enter play mode.");
        private GUIContent addSceneButtonGUIContent = new GUIContent("+");
        private GUIContent removeSceneButtonGUIContent = new GUIContent("-");
        private GUIContent clearPreloadSceneButtonGUIContent = new GUIContent(text: "Clear", tooltip: "Nullify preload scene.");
        private GUIContent applySceneSettingButtonGUIContent = new GUIContent(text: "Apply", tooltip: "Add current scenes to Build Setting.");
        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            sceneBuildSetting = (SceneBuildSetting)target;

            scenesProperty       = serializedObject.FindProperty("scenes");
            preloadSceneProperty = serializedObject.FindProperty("preloadScene");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPreloadSceneSection();

            GUILayout.Space(10);

            DrawSceneListSection();

            if (GUI.changed)
            {
                // Don't check duplicated scenes when add (adding creates empty scene slot) or remove scene
                if (sceneBuildSetting.Scenes.Count == scenesProperty.arraySize)
                    CheckDuplicatedScenes();
            }

            GUILayout.Space(10);

            if (GUILayout.Button(applySceneSettingButtonGUIContent, GUILayout.Width(100)))
                ApplySceneSetting();

            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Draw
        private void DrawPreloadSceneSection()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(preloadSceneGUIContent, GUILayout.Width(100f));
            EditorGUILayout.PropertyField(preloadSceneProperty, GUIContent.none);

            if (preloadSceneProperty.objectReferenceValue != null)
            {
                if (GUILayout.Button(clearPreloadSceneButtonGUIContent, GUILayout.Width(40f)))
                {
                    preloadSceneProperty.objectReferenceValue = null;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSceneListSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(scenesGUIContent, GUILayout.Width(60f));

            if (GUILayout.Button(addSceneButtonGUIContent, GUILayout.Width(20f)))
            {
                scenesProperty.InsertArrayElementAtIndex(scenesProperty.arraySize);

                scenesProperty.GetArrayElementAtIndex(scenesProperty.arraySize - 1).objectReferenceValue = null;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel += 1;

            int buildIndex = preloadSceneProperty.objectReferenceValue ? 1 : 0;
            int skipIndex = 0;

            for (int i = 0; i < scenesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUIContent label;

                // Draw label for unassigned scene
                if (scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    label = new GUIContent("<empty>");
                    skipIndex++;
                }
                else
                {
                    label = new GUIContent("Build Index " + (i + buildIndex - skipIndex));
                }

                EditorGUILayout.PropertyField(scenesProperty.GetArrayElementAtIndex(i), label);

                // Draw delete button
                if (GUILayout.Button(removeSceneButtonGUIContent, GUILayout.Width(20f)))
                {
                    // Delete twice due to weird bug by Unity involving array deletion. 
                    // Element of array gets cleared to null in the first call instead of actual delete.
                    int oldsize = scenesProperty.arraySize;
                    scenesProperty.DeleteArrayElementAtIndex(i);

                    if (oldsize == scenesProperty.arraySize)
                        scenesProperty.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel -= 1;
        }
        #endregion

        #region Core Functions
        private void CheckDuplicatedScenes()
        {
            if (preloadSceneProperty.objectReferenceValue != null)
            {
                SceneAsset preloadScene = (SceneAsset)preloadSceneProperty.objectReferenceValue;

                for (int i = scenesProperty.arraySize - 1; i >= 0; i--)
                {
                    SceneAsset scene = (SceneAsset)scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue;

                    if (scene == preloadScene)
                    {
                        scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue = sceneBuildSetting.Scenes[i];
                    }
                }
            }

            int foundIndex = -1;

            // Find changed element's index
            for (int i = 0; i < scenesProperty.arraySize; i++)
            {
                if (scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    continue;

                if (sceneBuildSetting.Scenes[i] != scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue)
                {
                    foundIndex = i;
                    break;
                }
            }

            // If changed element is already in the list, reset it back to previous value
            if (foundIndex != -1)
            {
                for (int i = 0; i < scenesProperty.arraySize; i++)
                {
                    if (i == foundIndex)
                        continue;

                    if (sceneBuildSetting.Scenes[i] == scenesProperty.GetArrayElementAtIndex(foundIndex).objectReferenceValue)
                    {
                        scenesProperty.GetArrayElementAtIndex(foundIndex).objectReferenceValue = sceneBuildSetting.Scenes[foundIndex];
                        break;
                    }
                }
            }
        }

        private void ClearNonAssignedScenes()
        {
            for (int i = scenesProperty.arraySize - 1; i >= 0; i--)
            {
                if (scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    // Delete twice due to weird bug by Unity involving array deletion. 
                    // Element of array gets cleared to null in the first call instead of actual delete.
                    int oldsize = scenesProperty.arraySize;
                    scenesProperty.DeleteArrayElementAtIndex(i);

                    if (oldsize == scenesProperty.arraySize)
                        scenesProperty.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void ApplySceneSetting()
        {
            List<EditorBuildSettingsScene> editorScenes = new List<EditorBuildSettingsScene>();

            EditorSceneManager.playModeStartScene = (SceneAsset)preloadSceneProperty.objectReferenceValue;

            // If there is a preload scene, assign it to build index 0
            if (preloadSceneProperty.objectReferenceValue != null)
            {
                SceneAsset preloadScene = (SceneAsset)preloadSceneProperty.objectReferenceValue;
                string preloadScenePath = AssetDatabase.GetAssetPath(preloadScene);
                editorScenes.Add(new EditorBuildSettingsScene(preloadScenePath, true));
            }

            for (int i = 0; i < scenesProperty.arraySize; i++)
            {
                SceneAsset scene = (SceneAsset)scenesProperty.GetArrayElementAtIndex(i).objectReferenceValue;

                if (scene != null)
                {
                    string path = AssetDatabase.GetAssetPath(scene);
                    editorScenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = editorScenes.ToArray();
        }
        #endregion
    }
}
