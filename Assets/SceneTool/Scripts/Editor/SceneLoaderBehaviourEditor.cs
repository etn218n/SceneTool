using UnityEngine;
using UnityEditor;
using System;

namespace SceneTool
{
    [CustomEditor(typeof(SceneLoaderBehaviour))]
    public class SceneLoaderBehaviourEditor : Editor
    {
        #region Fields & Properties
        private SerializedProperty loadTypeProperty;
        private SerializedProperty sceneToSetActive;
        private SerializedProperty scenesToLoadProperty;
        private SerializedProperty scenesToUnloadProperty;
        private SerializedProperty transitionSceneProperty;
        private SerializedProperty unloadUnusedAssetsProperty;
        private SerializedProperty hasTransitionSceneProperty;
        private SerializedProperty allowSceneActivationProperty;
        private SerializedProperty unloadScenesAfterLoadProperty;
        private SerializedProperty automaticallyUnloadTransitionSceneProperty;

        private SceneLoaderBehaviour behaviour;

        #endregion

        #region Unity Callbacks
        private void OnEnable()
        {
            loadTypeProperty = serializedObject.FindProperty("LoadType");
            sceneToSetActive = serializedObject.FindProperty("sceneToSetActive");
            scenesToLoadProperty = serializedObject.FindProperty("scenesToLoad");
            scenesToUnloadProperty = serializedObject.FindProperty("scenesToUnload");
            transitionSceneProperty = serializedObject.FindProperty("transitionScene");
            unloadUnusedAssetsProperty = serializedObject.FindProperty("UnloadUnusedAssets");
            hasTransitionSceneProperty = serializedObject.FindProperty("HasTransitionScene");
            allowSceneActivationProperty = serializedObject.FindProperty("AllowSceneActivation");
            unloadScenesAfterLoadProperty = serializedObject.FindProperty("UnloadScenesAfterLoad");
            automaticallyUnloadTransitionSceneProperty = serializedObject.FindProperty("AutomaticallyUnloadTransitionScene");

            behaviour = (SceneLoaderBehaviour)target;

            if (scenesToLoadProperty.arraySize == 0)
                scenesToLoadProperty.InsertArrayElementAtIndex(0);

            if (scenesToUnloadProperty.arraySize == 0)
                scenesToUnloadProperty.InsertArrayElementAtIndex(0);

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLoadTypePopup();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLoadTypePopup()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Action"), GUILayout.Width(50));
            EditorGUILayout.PropertyField(loadTypeProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            switch (behaviour.LoadType)
            {
                case SceneActionType.LoadScene: DrawLoadScenePanel(); break;
                case SceneActionType.LoadSceneAsync: DrawLoadSceneAsyncPanel(); break;

                case SceneActionType.LoadAdditiveScene: DrawLoadAdditiveScenePanel(); break;
                case SceneActionType.LoadAdditiveSceneAsync: DrawLoadAdditiveSceneAsyncPanel(); break;

                case SceneActionType.UnloadSceneAsync: DrawUnloadSceneAsyncPanel(); break;
                case SceneActionType.UnloadSelfAsync:  DrawUnloadSelfAsyncPanel();  break;
            }
        }

        private void DrawUnloadSelfAsyncPanel()
        {
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unload Unused Assets"), GUILayout.Width(135f));
            EditorGUILayout.PropertyField(unloadUnusedAssetsProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Draw Asynchronous Operation Panels
        private void DrawLoadSceneAsyncPanel()
        {
            GUILayout.Space(5);

            // Draw header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Load"), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Scene(s) to Load"), GUILayout.Width(115));
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                scenesToLoadProperty.InsertArrayElementAtIndex(scenesToLoadProperty.arraySize);

                scenesToLoadProperty.GetArrayElementAtIndex(scenesToLoadProperty.arraySize - 1).FindPropertyRelative("sceneAsset").objectReferenceValue = null;
            }
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(scenesToLoadProperty.GetArrayElementAtIndex(0), GUIContent.none, GUILayout.MinWidth(100));
            EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            for (int i = 1; i < scenesToLoadProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(scenesToLoadProperty.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.MinWidth(100));

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    // Delete twice due to weird bug by Unity involving array deletion. 
                    // Element of array gets cleared to null in the first call instead of actual delete.
                    int oldsize = scenesToLoadProperty.arraySize;
                    scenesToLoadProperty.DeleteArrayElementAtIndex(i);

                    if (oldsize == scenesToLoadProperty.arraySize)
                        scenesToLoadProperty.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Allow Scene Activation"), GUILayout.Width(135f));
            EditorGUILayout.PropertyField(allowSceneActivationProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Draw Scene to be set active
            if (behaviour.LoadType == SceneActionType.LoadAdditiveSceneAsync || scenesToLoadProperty.arraySize > 1)
                EditorGUILayout.PropertyField(sceneToSetActive, new GUIContent("Active Scene"));

            GUILayout.Space(10);

            DrawTransitionScenePanel();
        }

        private void DrawLoadAdditiveSceneAsyncPanel()
        {
            DrawLoadSceneAsyncPanel();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("After Load"), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unload Scenes After Load"), GUILayout.Width(160));
            EditorGUILayout.PropertyField(unloadScenesAfterLoadProperty, GUIContent.none, GUILayout.MaxWidth(20));
            EditorGUILayout.EndHorizontal();

            if (unloadScenesAfterLoadProperty.boolValue == true)
            {
                GUILayout.Space(5);

                DrawUnloadSceneAsyncPanel();
            }

            GUILayout.Space(5);
        }

        private void DrawTransitionScenePanel()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Transition"), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Has Transition Scene"), GUILayout.Width(135f));
            EditorGUILayout.PropertyField(hasTransitionSceneProperty, GUIContent.none, GUILayout.MaxWidth(20f));
            if (hasTransitionSceneProperty.boolValue == true)
            {
                EditorGUILayout.PropertyField(transitionSceneProperty, GUIContent.none);

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Automatically Unload"), GUILayout.Width(135f));
                EditorGUILayout.PropertyField(automaticallyUnloadTransitionSceneProperty, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.EndHorizontal();   
            }

            GUILayout.Space(10);
        }

        private void DrawUnloadSceneAsyncPanel()
        {
            // Draw add scene button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Scene(s) to Unload"), GUILayout.Width(115));
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                scenesToUnloadProperty.InsertArrayElementAtIndex(scenesToUnloadProperty.arraySize);

                scenesToUnloadProperty.GetArrayElementAtIndex(scenesToUnloadProperty.arraySize - 1).FindPropertyRelative("sceneAsset").objectReferenceValue = null;
            }

            // Draw the first SceneObject's sceneAsset on the same line with AddSceneButton
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(scenesToUnloadProperty.GetArrayElementAtIndex(0), GUIContent.none, GUILayout.MinWidth(100));
            EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            // Draw other SceneObject's sceneAssets on the next line
            for (int i = 1; i < scenesToUnloadProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(scenesToUnloadProperty.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.MinWidth(100));

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    // Delete twice due to weird bug by Unity involving array deletion. 
                    // Element of array gets cleared to null in the first call instead of actual delete.
                    int oldsize = scenesToUnloadProperty.arraySize;
                    scenesToUnloadProperty.DeleteArrayElementAtIndex(i);

                    if (oldsize == scenesToUnloadProperty.arraySize)
                        scenesToUnloadProperty.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unload Unused Assets"), GUILayout.Width(135f));
            EditorGUILayout.PropertyField(unloadUnusedAssetsProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Draw Synchronous Operation Panels
        private void DrawLoadScenePanel()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Scene to Load"), GUILayout.Width(100));
            EditorGUILayout.PropertyField(scenesToLoadProperty.GetArrayElementAtIndex(0), GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLoadAdditiveScenePanel() => DrawLoadScenePanel();
        #endregion
    }
}
