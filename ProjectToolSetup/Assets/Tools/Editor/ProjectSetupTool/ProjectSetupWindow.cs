using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

namespace GursaanjTools
{
    public class ProjectSetupWindow : EditorWindow
    {
        #region Variables

        private static ProjectSetupWindow m_window;
        private static string m_windowName = "Project Setup";
        private static Vector2 m_minSize = new Vector2(150,100);
        private static Vector2 m_maxSize = new Vector2(450,100);

        private const string m_gameNameLabel = "Game Name: ";
        private const string m_buttonLabel = "Create Project Structure";
        private const string m_genericGameName = "Game";
        private const string m_guiTextFieldName = "GameNameTextField";
        private const string m_sceneNameExtension = ".unity";

        private const string m_directoryCreationString = "{0}/{1}";
        private const string m_sceneCreationString = "{0}_{1}";
        
        //Display Dialogue Strings
        private const string m_confirmationString = "Sounds good!";
        private const string m_cancelString = "Actually, no!";
        private const string m_errorTitle = "Error";
        private const string m_genricNameDisputeTitle = "Are you sure?";
        private const string m_noNameForDirectoryMessage = "The Game needs a name for a directory to be created.";
        private readonly string m_genericDirectoryMessage = String.Format("Are you sure you would like to create a directory for {0}", m_genericGameName);

        private const int m_buttonHeight = 35;

        private bool m_shouldFocusOnFirstTextElement = true;
        private string m_gameName = m_genericGameName;
        
        #endregion


        #region Unity Methods

        public static void InitWindow()
        {
            m_window = GetWindow<ProjectSetupWindow>(m_windowName);
            m_window.minSize = m_minSize;
            m_window.maxSize = m_maxSize;
            m_window.Focus();
            m_window.Show();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.SetNextControlName(m_guiTextFieldName);
                m_gameName = EditorGUILayout.TextField(m_gameNameLabel, m_gameName);
            }
            
            GUILayout.Space(m_buttonHeight);

            Event currentEvent = Event.current;
            bool wasReturnPressed = currentEvent.isKey && currentEvent.keyCode == KeyCode.Return;

            if (GUILayout.Button(m_buttonLabel, GUILayout.Height(m_buttonHeight), GUILayout.ExpandWidth(true)) || wasReturnPressed)
            {
                CreateProjectFolders();
            }
            
            FocusOnTextField();

            if (m_window != null)
            {
                m_window.Repaint();
            }
        }

        #endregion
        
        #region Custom Methods

        private void CreateProjectFolders()
        {
            if (string.IsNullOrEmpty(m_gameName))
            {
                EditorUtility.DisplayDialog(m_errorTitle, m_noNameForDirectoryMessage, m_confirmationString);
                m_shouldFocusOnFirstTextElement = true;
                return;
            }

            if (string.Equals(m_gameName, m_genericGameName))
            {
                if (!EditorUtility.DisplayDialog(m_genricNameDisputeTitle, m_genericDirectoryMessage, m_confirmationString, m_cancelString))
                {
                    m_shouldFocusOnFirstTextElement = true;
                    return;
                }
            }
            
            //Create Main Directory
            string rootPath = string.Format(m_directoryCreationString, Application.dataPath, m_gameName);
            DirectoryInfo rootInfo = Directory.CreateDirectory(rootPath);
            if (!rootInfo.Exists)
            {
                return;
            }
            
            CreateSubFolders(rootPath);
            
            AssetDatabase.Refresh();
            CloseWindow();
        }
        
        //Todo : Create static class for string
        private void CreateSubFolders(string rootPath)
        {
            DirectoryInfo rootInfo = null;
            string newRootPath = string.Empty;
            List<String> folderNames = new List<string>();
            
            //For Art
            newRootPath = string.Format(m_directoryCreationString, rootPath, "Art");

            rootInfo = Directory.CreateDirectory(newRootPath);

            if (rootInfo.Exists)
            {
                folderNames.Clear();
                folderNames.Add("Animation");
                folderNames.Add("Objects");
                folderNames.Add("Materials");
                folderNames.Add("Shaders");
                folderNames.Add("Fonts");
                
                CreateFolders(newRootPath, folderNames);
            }
            
            //For Scripts
            newRootPath = string.Format(m_directoryCreationString, rootPath, "Scripts");

            rootInfo = Directory.CreateDirectory(newRootPath);

            if (rootInfo.Exists)
            {
                folderNames.Clear();
                folderNames.Add("Editor");
                folderNames.Add("Managers");
                folderNames.Add("Shaders");
                folderNames.Add("UI");
                folderNames.Add("Misc");
                
                CreateFolders(newRootPath, folderNames);
            }
            
            //For Audio
            newRootPath = string.Format(m_directoryCreationString, rootPath, "Audio");

            rootInfo = Directory.CreateDirectory(newRootPath);

            if (rootInfo.Exists)
            {
                folderNames.Clear();
                folderNames.Add("Sound Effects");
                folderNames.Add("Background Music");
                folderNames.Add("UI");

                CreateFolders(newRootPath, folderNames);
            }
            
            //For Prefabs
            newRootPath = string.Format(m_directoryCreationString, rootPath, "Prefabs");

            rootInfo = Directory.CreateDirectory(newRootPath);

            if (rootInfo.Exists)
            {
                folderNames.Clear();
                folderNames.Add("Characters");
                folderNames.Add("Environment");
                folderNames.Add("Props");
                folderNames.Add("UI");
                folderNames.Add("Misc");
                
                CreateFolders(newRootPath, folderNames);
            }
            
            //Create Scenes
            newRootPath = string.Format(m_directoryCreationString, rootPath, "Scenes");
            rootInfo = Directory.CreateDirectory(newRootPath);

            if (rootInfo.Exists)
            {
                CreateScene(newRootPath, string.Format(m_sceneCreationString, m_gameName, "StartScreen"));
                CreateScene(newRootPath,string.Format(m_sceneCreationString, m_gameName, "SandBox"));
                CreateScene(newRootPath,string.Format(m_sceneCreationString, m_gameName, "Loading"));
            }
        }

        private void CreateFolders(string path, List<string> folders)
        {
            if (string.IsNullOrEmpty(path) || folders == null || folders.Count == 0)
            {
                return;
            }

            foreach (string folder in folders)
            {
                Directory.CreateDirectory(string.Format(m_directoryCreationString, path, folder));
            }
        }

        private void CreateScene(string path, string sceneName)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            Scene currentScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            string fullSceneName = string.Format("{0}{1}", sceneName, m_sceneNameExtension);
            EditorSceneManager.SaveScene(currentScene, String.Format(m_directoryCreationString, path, fullSceneName), true);
        }
        
        private void FocusOnTextField()
        {
            if (m_shouldFocusOnFirstTextElement && m_window != null)
            {
                m_window.Focus(); // Refocus onto window if focus is lost from using the Display Dialogue
                EditorGUI.FocusTextInControl(m_guiTextFieldName);
                m_shouldFocusOnFirstTextElement = false;
            }
        }

        private void CloseWindow()
        {
            if (m_window != null)
            {
                m_window.Close();
            } 
        }

        #endregion
    }
}

