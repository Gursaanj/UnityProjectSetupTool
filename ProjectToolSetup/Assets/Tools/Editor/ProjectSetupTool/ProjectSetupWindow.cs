using System;
using UnityEditor;
using UnityEngine;
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
        
        //Display Dialogue Strings
        private const string m_confirmationString = "Sounds good!";
        private const string m_cancelString = "Actually, no!";
        private const string m_errorTitle = "Error";
        private const string m_genricNameDisputeTitle = "Are you sure?";
        private const string m_noNameForDirectoryMessage = "The Game needs a name for a directory to be created.";
        private readonly string m_genericDirectoryMessage = String.Format("Are you sure you would like to create a directory for {0}", m_genericGameName);

        private const int m_buttonHeight = 35;
        
        private string m_gameName = m_genericGameName;
        
        #endregion


        #region Unity Methods

        public static void InitWindow()
        {
            m_window = GetWindow<ProjectSetupWindow>(m_windowName);
            m_window.minSize = m_minSize;
            m_window.maxSize = m_maxSize;
            m_window.Show();
        }

        private void OnFocus()
        {
            if (GUI.GetNameOfFocusedControl() == "GameNameTextField")
            {
                EditorGUI.FocusTextInControl("GameNameTextField");
            }
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.SetNextControlName("GameNameTextField");
                m_gameName = EditorGUILayout.TextField(m_gameNameLabel, m_gameName);
            }
            
            GUILayout.Space(m_buttonHeight);

            Event currentEvent = Event.current;
            bool wasReturnPressed = currentEvent.isKey && currentEvent.keyCode == KeyCode.Return;

            if (GUILayout.Button(m_buttonLabel, GUILayout.Height(m_buttonHeight), GUILayout.ExpandWidth(true)) || wasReturnPressed)
            {
                CreateProjectFolders();
            }

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
                return;
            }

            if (string.Equals(m_gameName, m_genericGameName))
            {
                if (!EditorUtility.DisplayDialog(m_genricNameDisputeTitle, m_genericDirectoryMessage, m_confirmationString, m_cancelString))
                {
                    return;
                }
            }
            
            string rootPath = string.Format("{0}/{1}", Application.dataPath, m_gameName);
            
            Directory.CreateDirectory(rootPath);
            AssetDatabase.Refresh();

            CloseWindow();
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

