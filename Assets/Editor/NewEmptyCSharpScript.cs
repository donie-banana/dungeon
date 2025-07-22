// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEditor.SceneManagement;
// using UnityEngine;
// using UnityEditor.Build;

// [InitializeOnLoad]
// public static class AutoLayoutSwitcher
// {
//     // relative paths inside your project
//     const string EditLayout  = "Assets/Editor/Layouts/edit.wlt";
//     const string PlayLayout  = "Assets/Editor/Layouts/play.wlt";

//     static AutoLayoutSwitcher()
//     {
//         EditorApplication.playModeStateChanged += OnPlayModeChanged;
//     }

//     static void OnPlayModeChanged(PlayModeStateChange state)
//     {
//         switch (state)
//         {
//             case PlayModeStateChange.EnteredPlayMode:
//                 LoadLayout(PlayLayout);
//                 break;
//             case PlayModeStateChange.ExitingPlayMode:
//                 LoadLayout(EditLayout);
//                 break;
//         }
//     }

//     static void LoadLayout(string layoutPath)
//     {
//         if (System.IO.File.Exists(layoutPath))
//         {
//             EditorUtility.LoadWindowLayout(layoutPath);
//         }
//         else
//         {
//             Debug.LogWarning($"Layout file not found: {layoutPath}");
//         }
//     }
// }
