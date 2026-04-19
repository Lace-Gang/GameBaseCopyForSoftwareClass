using System.Collections;
using UnityEngine;

namespace GameBase
{
    public class LoadTitleUpdater : GameUpdater, IGameStateUpdate
    {
        [SerializeField] bool m_playsMusic = false;
        [Tooltip("Background music to play on the title screen")]
        [SerializeField] AudioClip m_titleScreenMusic;

        //completion checks for scene transition corou
        private bool m_UIAdjusted = false;
        private bool m_SceneDefaultsCompleted = false;
        private bool m_SceneLoaded = false;
        private bool m_FadeInCompleted = false;


        public override void ExecuteStateUpdate()
        {
            //load level and UI
            StartCoroutine(LoadTitle());

            //transitions to "title" game state
            GameInstance.Instance.m_gameState = GameState.TITLESCREEN;
        }



        /// <summary>
        /// Transitions to Title Screen
        /// </summary>
        /// <returns>Yield return for a Coroutine</returns>
        private IEnumerator LoadTitle()
        {
            //Check for UI adjustment completion so that code does not execute more than once
            if (!m_UIAdjusted)
            {
                //turn on title screen
                UserInterface.Instance.m_titleScreen.SetActive(true);

                //turn off other UI screens and HUD
                UserInterface.Instance.m_mainMenuScreen.SetActive(false);
                UserInterface.Instance.m_HUD.SetActive(false);
                UserInterface.Instance.m_winScreen.SetActive(false);
                UserInterface.Instance.m_loseScreen.SetActive(false);
                UserInterface.Instance.m_pauseScreen.SetActive(false);

                m_UIAdjusted = true;       //indicate UI has bee adjusted
            }

            //Check for scene load completion so that code does not execute more than once
            if (!m_SceneLoaded)
            {
                //loads UIDisplayScene
                yield return StartCoroutine(LoadScene("UIDisplayScene"));

                //unloads game screen
                StartCoroutine(UnloadScene(GameInstance.Instance.GetGameSceneName()));

                m_SceneLoaded = true;   //indicate scenes have been loaded
            }

            //Check for scene defaults completion so that code does not execute more than once
            if (!m_SceneDefaultsCompleted)
            {
                GameInstance.Instance.GetMusicPlayer()?.Stop();

                //plays audio
                if (m_playsMusic && m_titleScreenMusic != null)
                {
                    GameInstance.Instance.GetMusicPlayer()?.PlayOneShot(m_titleScreenMusic);
                }

                m_SceneDefaultsCompleted = true;    //indicate scene defaults have been completed
            }

            //Check for fade in completion so that code does not execute more than once
            if (!m_FadeInCompleted)
            {
                //Fade Screen In
                yield return StartCoroutine(UserInterface.Instance.FadeIn());

                m_FadeInCompleted = true;   //indicate fade in has been completed
            }


            //Unlock Cursor and make cursor visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //Set completion checks back to false
            m_UIAdjusted = false;
            m_SceneLoaded = false;
            m_SceneDefaultsCompleted = false;
            m_FadeInCompleted = false;
        }
    }
}
