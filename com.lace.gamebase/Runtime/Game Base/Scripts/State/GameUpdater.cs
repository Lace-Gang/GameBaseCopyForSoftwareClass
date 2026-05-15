using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameBase
{
    [System.Serializable]
    public abstract class GameUpdater : MonoBehaviour, IGameStateUpdate
    {
        public abstract void ExecuteStateUpdate();

        /// <summary>
        /// Loads a scene asynchronously
        /// </summary>
        /// <param name="sceneName">The name of the scene being loaded (MUST be the exact name of the scene (case insensitve) or the file path name if two scenes of the same name exist)</param>
        /// <returns>Yield return for Coroutine</returns>
        protected IEnumerator LoadScene(string sceneName)
        {
            //Only loads scene if scene is not already loaded
            if (!SceneManager.GetSceneByName(sceneName).IsValid())
            {
                //Loads specified scene Async (for better performance) and additive so that the base scene remains present
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }

        /// <summary>
        /// Unloads a scene asynchronously
        /// </summary>
        /// <param name="sceneName">The name of the scene being loaded (MUST be the exact name of the scene (case insensitve) or the file path name if two scenes of the same name exist)</param>
        /// <returns>Yield return for Coroutine</returns>
        protected IEnumerator UnloadScene(string sceneName)
        {
            //Only unloads scene if scene is already loaded
            if (SceneManager.GetSceneByName(sceneName).IsValid())
            {
                //Unloads specified scene Async (for better performance)
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }
}
