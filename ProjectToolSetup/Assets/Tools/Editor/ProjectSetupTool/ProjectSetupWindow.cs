using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class ProjectSetupWindow : EditorWindow
    {
        #region Variables

        private static ProjectSetupWindow m_window;
        private static string m_windowName = "Project Setup";
        private static Vector2 m_minSize = new Vector2(150,100);
        private static Vector2 m_maxSize = new Vector2(250,100);
        #endregion


        #region Main Methods

        public static void InitWindow()
        {
            m_window = GetWindow<ProjectSetupWindow>(m_windowName);
            m_window.minSize = m_minSize;
            m_window.maxSize = m_maxSize;
            m_window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(m_windowName);
        }

        #endregion
    }
}

