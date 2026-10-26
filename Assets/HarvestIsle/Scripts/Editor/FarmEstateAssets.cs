using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HarvestIsle.Editor
{
    public static class FarmEstateAssets
    {
        [MenuItem("Harvest Isle/An Nhien/Open my default farm and Play")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Open An Nhien from Edit mode.");
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the current scene before opening An Nhien.");
            if(!File.Exists(FarmEstate.ScenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("Nông trại An Nhiên");SceneManager.MoveGameObjectToScene(root,scene);
                root.AddComponent<FarmEstate>();EditorSceneManager.SaveScene(scene,FarmEstate.ScenePath);
            }
            EditorSceneManager.OpenScene(FarmEstate.ScenePath);
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();
            EditorApplication.isPlaying=true;
        }
    }
}
