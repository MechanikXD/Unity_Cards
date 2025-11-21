using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public static class ApplicationManager
    {
        public static void ExitApplication() => Application.Quit();
        
        public static void RestartCurrentScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        public static void EnterGameScene() => SceneManager.LoadScene("GameScene");
        
        public static void EnterMainMenuScene() => SceneManager.LoadScene("MainMenu");
    }
}