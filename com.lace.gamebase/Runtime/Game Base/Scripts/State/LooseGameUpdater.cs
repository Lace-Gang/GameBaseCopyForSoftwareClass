using System.Collections;
using UnityEngine;

namespace GameBase
{
    public class LooseGameUpdater : GameUpdater, IGameStateUpdate
    {
        [SerializeField] bool m_playsMusic = false;
        [Tooltip("Background music to play in the loose screen")]
        [SerializeField] AudioClip m_looseScreenMusic;


        //completion checks for scene transition corou
        private bool m_UIAdjusted = false;
        private bool m_SceneDefaultsCompleted = false;
        private bool m_SceneLoaded = false;
        private bool m_FadeOutCompleted = false;
        private bool m_FadeInCompleted = false;

        public override void ExecuteStateUpdate()
        {
            //load level and UI
            StartCoroutine(LoadLooseScreen());

            //transition to "lose screen" GameState
            GameInstance.Instance.m_gameState = GameState.LOSESCREEN;
        }


        /// <summary>
        /// Transitions to Loose Screen
        /// </summary>
        /// <returns>Yield return for a Coroutine</returns>
        private IEnumerator LoadLooseScreen()
        {
            //Check fade out completed so that code does not execute more than once
            if (!m_FadeOutCompleted)
            {
                //Fade Out
                yield return StartCoroutine(UserInterface.Instance.FadeOut());

                m_FadeOutCompleted = true;  //indicate fade out completed
            }

            //Check UI adjustment completed so that code does not execute more than once
            if (!m_UIAdjusted)
            {
                //turns off other UI screens and HUD
                UserInterface.Instance.m_titleScreen.SetActive(false);
                UserInterface.Instance.m_mainMenuScreen.SetActive(false);
                UserInterface.Instance.m_HUD.SetActive(false);
                UserInterface.Instance.m_winScreen.SetActive(false);
                UserInterface.Instance.m_pauseScreen.SetActive(false);

                //Turns on win screen
                UserInterface.Instance.m_loseScreen.SetActive(true);

                //disable Player Audio listener to avoid issues
                GameInstance.Instance.GetPlayerScript().GetComponent<PlayerController>().DisableAudioListener();

                m_UIAdjusted = true;    //indicate UI adjustment completed
            }

            //Check scene loading completed so that code does not execute more than once
            if (!m_SceneLoaded)
            {
                //loads UIDisplayScene
                yield return StartCoroutine(LoadScene("UIDisplayScene"));

                //unloads Game screen
                StartCoroutine(UnloadScene(GameInstance.Instance.GetGameSceneName()));

                m_SceneLoaded = true;   //indicate scene loading completed
            }

            //Check scene defaults completed so that code does not execute more than once
            if (!m_SceneDefaultsCompleted)
            {
                //update audio
                GameInstance.Instance.GetMusicPlayer()?.Stop();

                if (m_playsMusic && m_looseScreenMusic != null)
                {
                    GameInstance.Instance.GetMusicPlayer()?.PlayOneShot(m_looseScreenMusic);
                }

                //Unlock Cursor and make cursor visible
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                m_SceneDefaultsCompleted = true;    //indicate scene defaults completed
            }

            //Check fade in completed so that code does not execute more than once
            if (!m_FadeInCompleted)
            {
                //Fade In
                yield return StartCoroutine(UserInterface.Instance.FadeIn());

                m_FadeInCompleted = true;   //indicate fade in completed
            }

            //set completion checks back to false
            m_FadeOutCompleted = false;
            m_UIAdjusted = false;
            m_SceneLoaded = false;
            m_SceneDefaultsCompleted = false;
            m_FadeInCompleted = false;
        }
    }
}
