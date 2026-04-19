using System.Collections;
using UnityEngine;

namespace GameBase
{
    public class WinGameUpdater : GameUpdater, IGameStateUpdate
    {
        [SerializeField] bool m_playsMusic = false;
        [Tooltip("Background music to play in the win screen")]
        [SerializeField] AudioClip m_winScreenMusic;


        //completion checks for scene transition corou
        private bool m_UIAdjusted = false;
        private bool m_SceneDefaultsCompleted = false;
        private bool m_SceneLoaded = false;
        private bool m_FadeOutCompleted = false;
        private bool m_FadeInCompleted = false;

        public override void ExecuteStateUpdate()
        {
            //load level and UI
            StartCoroutine(LoadWinScreen());

            //transition to "win screen" GameState
            GameInstance.Instance.m_gameState = GameState.WINSCREEN;
        }



        /// <summary>
        /// Transitions to Win Screen
        /// </summary>
        /// <returns>Yield return for a Coroutine</returns>
        private IEnumerator LoadWinScreen()
        {
            //Check for fade out completed so that code does not execute more than once
            if (!m_FadeOutCompleted)
            {
                //Fade Out
                yield return StartCoroutine(UserInterface.Instance.FadeOut());

                m_FadeOutCompleted = true;  //indicate that fade out has completed
            }

            //Check for UI Adjustment completed so that code does not execute more than once
            if (!m_UIAdjusted)
            {
                //turns off other UI screens and HUD
                UserInterface.Instance.m_titleScreen.SetActive(false);
                UserInterface.Instance.m_mainMenuScreen.SetActive(false);
                UserInterface.Instance.m_HUD.SetActive(false);
                UserInterface.Instance.m_loseScreen.SetActive(false);
                UserInterface.Instance.m_pauseScreen.SetActive(false);

                //Turns on win screen
                UserInterface.Instance.m_winScreen.SetActive(true);


                //disable Player Audio listener to avoid issues
                GameInstance.Instance.GetPlayerScript().GetComponent<PlayerController>().DisableAudioListener();

                m_UIAdjusted = true;    //indicate UI Adjustment has completed
            }


            //Check for scene loading completed so that code does not execute more than once
            if (!m_SceneLoaded)
            {
                //loads UIDisplayScene
                yield return StartCoroutine(LoadScene("UIDisplayScene"));

                //unloads Game screen
                StartCoroutine(UnloadScene(GameInstance.Instance.GetGameSceneName()));

                m_SceneLoaded = true;   //indicated scene loading completed
            }

            //Check for scene defaults completed so that code does not execute more than once
            if (!m_SceneDefaultsCompleted)
            {
                //update audio
                GameInstance.Instance.GetMusicPlayer()?.Stop();

                if (m_playsMusic && m_winScreenMusic != null)
                {
                    GameInstance.Instance.GetMusicPlayer()?.PlayOneShot(m_winScreenMusic);
                }

                //Unlock Cursor and make cursor visible
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                m_SceneDefaultsCompleted = true;    //indicate scene defaults completed
            }

            //Check for fade in completed so that code does not execute more than once
            if (!m_FadeInCompleted)
            {
                //Fade In
                yield return StartCoroutine(UserInterface.Instance.FadeIn());

                m_FadeInCompleted = true;   //indicate fade in completed
            }


            //Set completion checks back to false
            m_FadeOutCompleted = false;
            m_UIAdjusted = false;
            m_SceneLoaded = false;
            m_SceneDefaultsCompleted = false;
            m_FadeInCompleted = false;
        }
    }
}
