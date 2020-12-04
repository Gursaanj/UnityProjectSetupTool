using UnityEditor;

namespace GursaanjTools
{
    public class EditorMenus
    {
        [MenuItem("GursaanjTools/Projects/Project Setup Tool")]
        public static void InitProjectSetupTool()
        {
            ProjectSetupWindow.InitWindow();
        }
    }
}

