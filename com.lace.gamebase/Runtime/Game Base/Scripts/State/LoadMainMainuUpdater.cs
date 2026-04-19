using System.Collections;
using UnityEngine;

namespace GameBase
{
    public class LoadMainMainuUpdater : GameUpdater, IGameStateUpdate
    {
        [SerializeField] bool m_playsMusic = false;
        [Tooltip("Background music to play in the main menu")]
        [SerializeField] AudioClip m_mainMenuScreenMusic;
        [Tooltip("Displays a 'Load Game' option in the main menu")]
        [SerializeField] bool m_loadFromMainMenu = true;

        //completion checks for scene transition corou
        private bool m_UIAdjusted = false;
        private bool m_SceneDefaultsCompleted = false;
        private bool m_SceneLoaded = false;
        private bool m_FadeOutCompleted = false;
        private bool m_FadeInCompleted = false;


        public override void ExecuteStateUpdate()
        {
            //load level and UI
            StartCoroutine(LoadMainMenu());

            //transitions to "main menu screen" game state
            GameInstance.Instance.m_gameState = GameState.MAINMENUSCREEN;
        }



        /// <summary>
        /// Transitions to Main Menu
        /// </summary>
        /// <returns>Yield return for a Coroutine</returns>
        private IEnumerator LoadMainMenu()
        {
            //change game state to Main Menu Screen to prevent LoadMainMenu from being called repeatedly
            GameInstance.Instance.m_gameState = GameState.MAINMENUSCREEN;

            //Check for fade out completed so that code does not execute more than once
            if (!m_FadeOutCompleted)
            {
                //Fade Screen Out
                yield return StartCoroutine(UserInterface.Instance.FadeOut());

                m_FadeOutCompleted = true;  //indicate fade out has been completed
            }

            //Check for UI Adjustment completion so that code does not execute more than once
            if (!m_UIAdjusted)
            {
                //turn on main menu screen
                UserInterface.Instance.m_mainMenuScreen.SetActive(true);

                //Only displays "Load" button if it is indicated that that button should be present
                if (m_loadFromMainMenu)
                {
                    UserInterface.Instance.m_loadButtonObject.SetActive(true);

                    //If there is no valid save to load, makes button non-interactable
                    UserInterface.Instance.m_loadButton.interactable = GameInstance.Instance.GetIsValidSaveFile();
                }

                //turn off other UI screens and HUD
                UserInterface.Instance.m_titleScreen.SetActive(false);
                UserInterface.Instance.m_HUD.SetActive(false);
                UserInterface.Instance.m_winScreen.SetActive(false);
                UserInterface.Instance.m_loseScreen.SetActive(false);
                UserInterface.Instance.m_pauseScreen.SetActive(false);

                if (GameInstance.Instance.GetPlayerScript() != null)
                {
                    //disable Player Audio listener to avoid issues
                    GameInstance.Instance.GetPlayerScript().GetComponent<PlayerController>().DisableAudioListener();
                }

                m_UIAdjusted = true;    //indicate UI has beed adjusted
            }

            //Check for Scene Loading completion so that code does not execute more than once
            if (!m_SceneLoaded)
            {
                //loads UIDisplayScene
                yield return StartCoroutine(LoadScene("UIDisplayScene"));

                //unloads game screen
                StartCoroutine(UnloadScene(GameInstance.Instance.GetGameSceneName()));

                m_SceneLoaded = true;   //indicated scenes have been loaded
            }

            //Check for scene default completion so that code does not execute more than once
            if (!m_SceneDefaultsCompleted)
            {
                //update audio
                GameInstance.Instance.GetMusicPlayer()?.Stop();

                if (m_playsMusic && GameInstance.Instance.GetMusicPlayer() != null)
                {
                    GameInstance.Instance.GetMusicPlayer().volume = 0.5f;    //Raise Music Volume
                }

                if (m_playsMusic && m_mainMenuScreenMusic != null)
                {
                    GameInstance.Instance.GetMusicPlayer()?.PlayOneShot(m_mainMenuScreenMusic);
                }

                m_SceneDefaultsCompleted = true;    //indicate that scene defaults have been completed
            }

            //Check for fade in completion so that code does not execute more than once
            if (!m_FadeInCompleted)
            {
                //m_userInterface.FadeScreen();
                yield return StartCoroutine(UserInterface.Instance.FadeIn());

                m_FadeInCompleted = true;   //indicate that scene fade in has completed
            }


            //Unlock Cursor and make cursor visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //Set completion checks back to false
            m_FadeOutCompleted = false;
            m_UIAdjusted = false;
            m_SceneLoaded = false;
            m_SceneDefaultsCompleted = false;
            m_FadeInCompleted = false;
        }
    }
}
