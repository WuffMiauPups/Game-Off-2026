using UnityEditor;
using UnityEditor.SceneManagement;

namespace Voices.EditorTools
{
    // Pressing Play in the editor always starts in the first scene of the build (the main menu),
    // like the finished game, no matter which scene is open.
    [InitializeOnLoad]
    static class PlayFromMainMenu
    {
        static PlayFromMainMenu()
        {
            var scenes = EditorBuildSettings.scenes;
            EditorSceneManager.playModeStartScene = scenes.Length > 0
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[0].path)
                : null;
        }
    }
}
