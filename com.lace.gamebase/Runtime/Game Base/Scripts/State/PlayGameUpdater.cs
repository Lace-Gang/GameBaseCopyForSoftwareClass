using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public class PlayGameUpdater : GameUpdater, IGameStateUpdate
    {
        [Header("Game Settings and Flow")]
        [Tooltip("Does the game have a pause menu")]
        [SerializeField] bool m_gameHasPauseMenu = true;
        [Tooltip("Which key on the keyboard will be used to open the pause menu")]
        [SerializeField] KeyCode m_pauseMenuToggleKey = KeyCode.X;
        [Tooltip("Should opening the inventory pause the game")]
        [SerializeField] bool m_inventoryPausesGame = true;
        [Tooltip("Which key on the keyboard will be used to open/close the inventory")]
        [SerializeField] KeyCode m_toggleInventoryKey = KeyCode.B;


        [Header("Save and Load Conditions")]
        [Tooltip("Player can save game by hitting a specific key on their keyboard")]
        [SerializeField] bool m_saveHotKeyEnabled = false;
        [Tooltip("Which key on the keyboard can be used to save the game")]
        [SerializeField] KeyCode m_saveHotKey = KeyCode.J;
        [Tooltip("Player can load game by hitting a specific key on their keyboard")]
        [SerializeField] bool m_loadHotKeyEnabled = false;
        [Tooltip("Which key on the keyboard can be used to load the game")]
        [SerializeField] KeyCode m_loadHotKey = KeyCode.L;





        public override void ExecuteStateUpdate()
        {
            //Opens or closes pause menu if game is supposed to have a pause menu, the player character is alive, and the player hits the pause key
            if (m_gameHasPauseMenu && GameInstance.Instance.getPlayerAlive() && Input.GetKeyDown(m_pauseMenuToggleKey))
            {
                GameInstance.Instance.TogglePauseMenu();
            }

            //Opens or closes inventory if the inventory system is being used, the player character is alive, and the player hits the inventory key
            if (GameInstance.Instance.GetUseInventory() && GameInstance.Instance.getPlayerAlive() && Input.GetKeyDown(m_toggleInventoryKey))
            {
                GameInstance.Instance.ToggleInventory();
            }

            //Update prompt display, evaluate if prompt is being triggered, and execute prompt if so
            PromptUpdate();

            //load game if load hot key is enabled and pressed and there is a valid save file to load
            if (m_loadHotKeyEnabled && Input.GetKeyDown(m_loadHotKey) && GameInstance.Instance.GetIsValidSaveFile())
            {
                StartCoroutine(UserInterface.Instance.FadeOut());  //fade out screen for more visually smooth transition
                GameInstance.Instance.m_gameState = GameState.LOADSAVE;
                StartCoroutine(GameInstance.Instance.LoadSaveTransition());   //Load save file
            }

            //save game if save hot key is enabled and pressed
            if (m_saveHotKeyEnabled && Input.GetKeyDown(m_saveHotKey))
            {
                DataPersistenceManager.Instance.SaveGame();
            }
        }




        /// <summary>
        /// Displays highest priority prompt to the screen and checks if the prompt has been interacted with
        /// </summary>
        private void PromptUpdate()
        {
            List<IPrompter> activePrompters = GameInstance.Instance.GetActivePrompters();
            //Hide the prompt box from the user interface if there are no prompts to display
            if (activePrompters.Count == 0)
            //if (m_activePrompters.Count == 0)
            {
                UserInterface.Instance.HidePromptBox();
                return;
            }

            //find the prompt with the highest priority
            IPrompter highestPriority = activePrompters[0];
            foreach (IPrompter prompter in activePrompters)
            {
                if (prompter.GetPromptPriority() > highestPriority.GetPromptPriority()) highestPriority = prompter;
            }

            //display highest priotiry prompt
            UserInterface.Instance.DisplayPromptBox(highestPriority.GetPrompt());

            //Check if prompt is being interacted with, and notify prompter if so
            if (Input.GetKeyDown(highestPriority.GetPromptInteractionKey())) highestPriority.ExecutePrompt();
        }
    }
}
