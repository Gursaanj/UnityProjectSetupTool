using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        
        [MenuItem("GursaanjTools/Projects/Project Setup Tool")]
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

            m_gameName = IndexGameNameIfNeeded(m_gameName);
            
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

        private void CreateSubFolders(string rootPath)
        {
            DirectoryInfo rootInfo = null;
            string newRootPath = string.Empty;
            List<String> folderNames = new List<string>();

            SortedList<string, List<string>> directories = ProjectSetupData.GetProjectStructure();

            if (string.IsNullOrEmpty(rootPath) || directories == null)
            {
                return;
            }

            foreach (string directory in directories.Keys)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    continue;
                }

                newRootPath = string.Format(m_directoryCreationString, rootPath, directory);
                rootInfo = Directory.CreateDirectory(newRootPath);

                List<string> subDirectories = directories[directory];

                if (rootInfo.Exists && subDirectories != null)
                {
                    folderNames.Clear();
                    
                    foreach (string subDirectory in subDirectories)
                    {
                        if (string.IsNullOrEmpty(subDirectory))
                        {
                            continue;
                        }
                        
                        folderNames.Add(subDirectory);
                    }
                    
                    CreateFolders(newRootPath, folderNames);
                    
                    //Special case for Scenes Folder : Add Scenes
                    if (string.Equals(directory, ProjectSetupData.GetSceneDirectory()))
                    {
                        List<string> sceneNames = ProjectSetupData.GetSceneNames();

                        if (sceneNames != null)
                        {
                            foreach (string sceneName in sceneNames)
                            {
                                CreateScene(newRootPath, string.Format(m_sceneCreationString, m_gameName, sceneName));
                            }
                        }
                    }
                }
            }
        }
        
        // Adds a +1 to the gamename if multiple folders exist
        private string IndexGameNameIfNeeded(string gameName)
        {
            string newGameName = gameName;
            int sucessionIndex = 1;

            string[] mainDirectories = Directory.GetDirectories(Application.dataPath).GetDirectoryNames();

            if (mainDirectories != null)
            {
                while (mainDirectories.Contains(newGameName))
                {
                    newGameName = string.Format("{0}{1}", gameName, sucessionIndex);
                    sucessionIndex++;
                }
            }

            return newGameName;
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

