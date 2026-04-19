using System.Collections;
using UnityEngine;

namespace GameBase
{
    public class StartGameUpdater : GameUpdater, IGameStateUpdate
    {
        [SerializeField] bool m_playsMusic = false;
        [Tooltip("Background music to play during gameplay")]
        [SerializeField] AudioClip m_gameBackgroundMusic;

        //completion checks for scene transition corou
        private bool m_playerIsSpawned = false;
        private bool m_UIAdjusted = false;
        private bool m_SceneDefaultsCompleted = false;
        private bool m_SceneLoaded = false;
        private bool m_FadeOutCompleted = false;
        private bool m_FadeInCompleted = false;


        public override void ExecuteStateUpdate()
        {
            //load level and HUD, spawns player (if applicable), and optionally loads from save file
            StartCoroutine(LoadGame());
        }


        /// <summary>
        /// Transitions to Gameplay
        /// </summary>
        /// <returns>Yield return for a Coroutine</returns>
        private IEnumerator LoadGame()
        {
            //transition to LoadGame game state to prevent LoadGame from being called repeatedly
            GameInstance.Instance.m_gameState = GameState.LOADGAME;

            //Check for fade out completion so that code does not execute more than once
            if (!m_FadeOutCompleted)
            {
                //Fade Out
                yield return StartCoroutine(UserInterface.Instance.FadeOut());

                m_FadeOutCompleted = true;  //Indicate fade out has completed
            }

            //Check for UI Adjustment completed so that code does not execute more than once
            if (!m_UIAdjusted)
            {
                //turns off User Interface screens
                UserInterface.Instance.m_titleScreen.SetActive(false);
                UserInterface.Instance.m_mainMenuScreen.SetActive(false);
                UserInterface.Instance.m_winScreen.SetActive(false);
                UserInterface.Instance.m_loseScreen.SetActive(false);
                UserInterface.Instance.m_pauseScreen.SetActive(false);

                //Turns on HUD
                UserInterface.Instance.m_HUD.SetActive(true);

                m_UIAdjusted = true;    //Indicate UI Adjustment has completed
            }

            //Check for scene loading completed so that code does not execute more than once
            if (!m_SceneLoaded)
            {
                //Restarts Game if applicable
                if (GameInstance.Instance.GetRestartingGame())
                {
                    //yield return StartCoroutine(UnloadScene(m_gameSceneName));
                    yield return StartCoroutine(UnloadScene(GameInstance.Instance.GetGameSceneName()));
                    //m_restartingGame = false;
                }

                //Loads Game scene
                yield return StartCoroutine(LoadScene(GameInstance.Instance.GetGameSceneName()));

                //Unloads UIDisplayScene
                StartCoroutine(UnloadScene("UIDisplayScene"));

                m_SceneLoaded = true;   //indicate that scene loading has been completed
            }

            //check for player spawning completed so that code does not execute more than once
            if (!m_playerIsSpawned)
            {
                //Spawns player character
                PlayerSpawnPoint spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();
                if (spawnPoint != null)
                {
                    //Destroy previous Player if such a Player exists
                    if (GameInstance.Instance.GetPlayerCharacter() != null)
                    {
                        GameObject.Destroy(GameInstance.Instance.GetPlayerCharacter());
                        GameInstance.Instance.SetPlayerScript(null);
                    }

                    GameInstance.Instance.SetPlayerCharacter(GameObject.Instantiate(GameInstance.Instance.GetPlayerPrefab(), spawnPoint.transform.position, spawnPoint.transform.rotation));   //Create player
                    GameInstance.Instance.SetPlayerScript(GameInstance.Instance.GetPlayerCharacter().GetComponentInChildren<PlayerCharacter>());   //Get reference to PlayerCharacter component
                    GameInstance.Instance.GetPlayerScript().SetRespawnHealthType(GameInstance.Instance.GetRespawnHealthPercentage());   //Set health percentage on respawn
                    GameInstance.Instance.GetPlayerCharacter().SetActive(true);      //Activate Player
                }
                else
                {
                    //Notify user if there is no spawn point. Without a spawn point, a player cannot be spawned.
                    Debug.LogError("No PlayerSpawnPoint was located in the scene! Player will not be spawned.");
                }


                m_playerIsSpawned = true;   //indicated player spawning completed
            }

            //Check for scene defaults completed so that code does not execute more than once
            if (!m_SceneDefaultsCompleted)
            {
                //Clear inventory to prepare for loading the save file
                Inventory.Instance.ClearInventory();

                //Reset ammunition to default values
                foreach (AmmunitionTracker tracker in GameInstance.Instance.GetAmmunitionList())
                {
                    tracker.ResetAmmunition();
                }

                //Resets Score
                GameInstance.Instance.SetScore(0);
                UserInterface.Instance.UpdateScore(GameInstance.Instance.GetScore());

                //update audio
                GameInstance.Instance.GetMusicPlayer()?.Stop();

                if (m_playsMusic && m_gameBackgroundMusic != null)
                {
                    GameInstance.Instance.GetMusicPlayer()?.PlayOneShot(m_gameBackgroundMusic);
                }

                //Lock Cursor and make cursor invisible
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //unpause game (in the event the game was paused)
                GameInstance.Instance.SetPauseMenuOpen(false);
                GameInstance.Instance.UnpauseGame();

                m_SceneDefaultsCompleted = true;    //indicate scene defaults completed
            }


            //Load save file if applicable. If not, transition to "Play Game" game state
            if (GameInstance.Instance.GetLoadOnPlay())
            {
                GameInstance.Instance.m_gameState = GameState.LOADSAVE;
                StartCoroutine(GameInstance.Instance.LoadSaveTransition());
            }
            else if (!m_FadeInCompleted)    //check for fade in completed so that code does not execute more than once
            {
                if (GameInstance.Instance.GetRestartingGame())
                {
                    yield return new WaitForSeconds(0.5f);  //Wait breifly before fade in to prevent camera glitch


                    //Fade In
                    yield return StartCoroutine(UserInterface.Instance.FadeIn());


                    //transition to "play game" GameState
                    GameInstance.Instance.m_gameState = GameState.PLAYGAME;

                    GameInstance.Instance.SetRestartingGame(false);   //indicate restarting game completed and game is no longer set for a restart
                    m_FadeInCompleted = true;   //indicate fade in completed
                }
                else
                {

                    //Fade In
                    yield return StartCoroutine(UserInterface.Instance.FadeIn());

                    //transition to "play game" GameState
                    GameInstance.Instance.m_gameState = GameState.PLAYGAME;

                    m_FadeInCompleted = true;   //indicate that fade in has completed
                }

                GameInstance.Instance.SetPlayerAlive(true);

            }



            //Set completion checks back to false
            m_FadeOutCompleted = false;
            m_SceneLoaded = false;
            m_playerIsSpawned = false;
            m_UIAdjusted = false;
            m_SceneDefaultsCompleted = false;
            m_FadeInCompleted = false;
        }
    }
}
