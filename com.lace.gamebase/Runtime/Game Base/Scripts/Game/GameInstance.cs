using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameBase
{
    public class GameInstance : MonoBehaviour, IDataPersistence, ISubscriber
    {
        #region Hidden variables

        ////Hidden Variables
        
        //Used for tracking various states and other important info
        private bool m_pauseMenuOpen = false;                   //Is the pause menu open
        private bool m_usesInventory = false;                   //Is the inventory system being used in this game
        private bool m_inventoryOpen = false;                   //Is the inventory open
        private bool m_validSaveFile = false;
        private bool m_loadOnPlay = false;                      //Should save file load when game loads
        private bool m_restartingGame = false;                  //Has player indicated from pause menu to restart since the last update

        ////completion checks for scene transition coroutines
        private bool m_saveHasAlreadyLoaded = false;            //Tracks if the save file has been loaded in the current scene transition so that it isn't loaded repeatedly
        private bool m_FadeInCompleted = false;                 //Tracks if the fade in has been completed in the current scene transition so that the screen does not attempt to fade out repeatedly

        //Tracks player
        private GameObject m_playerCharacter;                   //Reference to the Player Character
        private PlayerCharacter m_playerScript;                 //Reference to the Player Character Script

        //Tracks other data
        private float m_score;                                  //Current score

        //Tracking Lists
        private List<IPrompter> m_activePrompters = new List<IPrompter>();  //List of all currently active prompts (messages that display on the screen through the UI)

        //Game State Context
        private GameStateContext m_stateContext;

        #endregion Hidden variables


        #region Exposed In Editor Variables

        ////Exposed Variables
        [Header("Critical Information and References")]
        [Tooltip("What Game State the game will be in when the game is first opened")]
        [SerializeField] public  GameState m_gameState = GameState.LOADTITLE;     //What State is the game in
        [Tooltip("Reference to the 'Player' prefab")]
        [SerializeField] GameObject m_playerPrefab;
        [Tooltip("Name of the Scene where the game will execute")]
        [SerializeField] string m_gameSceneName = "SampleScene";    //Name of the game scene. Used to load game scene

        [Header("Game Settings and Flow")]
        [Tooltip("After loosing the game, where does the player restart from")]
        [SerializeField] RestartMode m_restartMode = RestartMode.RESTARTFROMBEGINNING;
        //[Tooltip("Does the game have a pause menu")]
        //[SerializeField] bool m_gameHasPauseMenu = true;
        //[Tooltip("Which key on the keyboard will be used to open the pause menu")]
        //[SerializeField] KeyCode m_pauseMenuToggleKey = KeyCode.X;
        [Tooltip("Should opening the inventory pause the game")]
        [SerializeField] bool m_inventoryPausesGame = true;
        //[Tooltip("Which key on the keyboard will be used to open/close the inventory")]
        //[SerializeField] KeyCode m_toggleInventoryKey = KeyCode.B;

        [Header("General Player Info")]
        [Tooltip("Time between player death event and transition")]
        [SerializeField] float m_deathTransitionTimer = 4f;

        [Header ("Player Respawn")]
        [Tooltip("How should the player respawn")]
        [SerializeField] RespawnType m_respawnType = RespawnType.RESPAWNINPLACE;
        [Tooltip("What should player health be at respawn")]
        [SerializeField] float m_respawnHealthPercentage = 100;
        [Tooltip("Time (in seconds) after the player respawns when they cannot be hurt")]
        [SerializeField] float m_respawnInvincibilityTimer = 2f;

        [Header("Object Tracking")]
        [Tooltip("List Of Ammunition Types To Track")]
        [SerializeField] List<AmmunitionTracker> m_ammunitionList = new List<AmmunitionTracker>();
        [Tooltip("Default SpawnableSound prefab")]
        [SerializeField] GameObject m_SpawnableSound;



        [Header("Save and Load Conditions")]
        [Tooltip("Displays a 'Save Game' option in the pause menu")]
        [SerializeField] bool m_saveFromPauseMenu = true;
        //[Tooltip("Displays a 'Load Game' option in the main menu")]
        //[SerializeField] bool m_loadFromMainMenu = true;
        //[Tooltip("Player can save game by hitting a specific key on their keyboard")]
        //[SerializeField] bool m_saveHotKeyEnabled = false;
        //[Tooltip("Which key on the keyboard can be used to save the game")]
        //[SerializeField] KeyCode m_saveHotKey = KeyCode.J;
        //[Tooltip("Player can load game by hitting a specific key on their keyboard")]
        //[SerializeField] bool m_loadHotKeyEnabled = false;
        //[Tooltip("Which key on the keyboard can be used to load the game")]
        //[SerializeField] KeyCode m_loadHotKey = KeyCode.L;


        [Header("Save and Load Information")]
        [SerializeField] bool m_saveScore = false;


        [Header("Audio Information")]
        [Tooltip("Does the GameInstance play background music")]
        [SerializeField] bool m_playsMusic = false;
        [Tooltip("Reference to the AudioSource that will be dedicated to playing background music")]
        [SerializeField] AudioSource m_musicPlayer;
        //[Tooltip("Background music to play on the title screen")]
        //[SerializeField] AudioClip m_titleScreenMusic;
        //[Tooltip("Background music to play in the main menu")]
        //[SerializeField] AudioClip m_mainMenuScreenMusic;
        //[Tooltip("Background music to play during gameplay")]
        //[SerializeField] AudioClip m_gameBackgroundMusic;
        //[Tooltip("Background music to play in the win screen")]
        //[SerializeField] AudioClip m_winScreenMusic;
        //[Tooltip("Background music to play in the loose screen")]
        //[SerializeField] AudioClip m_looseScreenMusic;

        [Header("Game Updaters")]
        [SerializeField] GameUpdater m_loadTitleUpdater;
        [SerializeField] GameUpdater m_titleScreenUpdater;
        [SerializeField] GameUpdater m_loadMainMenuUpdater;
        [SerializeField] GameUpdater m_mainMenuScreenUpdater;
        [SerializeField] GameUpdater m_startGameUpdater;
        [SerializeField] GameUpdater m_loadGameUpdater;
        [SerializeField] GameUpdater m_loadSaveUpdater;
        [SerializeField] GameUpdater m_playGameUpdater;
        [SerializeField] GameUpdater m_winGameUpdater;
        [SerializeField] GameUpdater m_loseGameUpdater;
        [SerializeField] GameUpdater m_winScreenUpdater;
        [SerializeField] GameUpdater m_looseScreenUpdater;


        #endregion Exposed In Editor Variables


        public static GameInstance Instance { get; private set; }  //Allows other scripts to get the singleton instance of the GameInstance


        #region Getters and Setters
        public PlayerCharacter GetPlayerScript() { return m_playerScript; }  //Allows other scripts to access the current player character script

        /// <summary>
        /// Sets whether the save file should load next time the game loads
        /// </summary>
        /// <param name="loadSaveFile">Should save file load next time game loads</param>
        public void SetLoadOnPlay(bool loadSaveFile)
        {
            m_loadOnPlay = (loadSaveFile) ? true : false;
        }

        /// <summary>
        /// Set if there is a valid save file (for if/when that changes)
        /// </summary>
        /// <param name="validSaveFile"></param>
        public void SetValidSaveFile(bool validSaveFile)
        {
            m_validSaveFile = validSaveFile;
        }


        public string GetGameSceneName() { return m_gameSceneName; }

        public AudioSource GetMusicPlayer() { return m_musicPlayer; }

        public bool GetIsValidSaveFile() { return m_validSaveFile; }

        public GameObject GetPlayerCharacter() { return m_playerCharacter; }

        public GameObject GetPlayerPrefab() { return m_playerPrefab; }

        public float GetRespawnHealthPercentage() { return m_respawnHealthPercentage; }

        public List<AmmunitionTracker> GetAmmunitionList() { return m_ammunitionList; }

        public float GetScore() { return m_score; }

        public bool GetLoadOnPlay() { return m_loadOnPlay; }

        public bool GetUseInventory() { return m_usesInventory; }


        public List<IPrompter> GetActivePrompters() { return m_activePrompters; }


        public void SetPlayerScript(PlayerCharacter playerScript) { m_playerScript = playerScript; }

        public void SetPlayerCharacter(GameObject playerCharacter) { m_playerCharacter = playerCharacter; }

        public void SetScore(float score) { m_score = score; }

        public void SetPauseMenuOpen(bool pauseMenuOpen) { m_pauseMenuOpen = pauseMenuOpen; }


        #endregion Getters and Setters


        #region Awake, Start, and Update

        /// <summary>
        /// Checks that only this instance of the GameInstance exists at this time and notifies the user if this is not true.
        /// Only one instance of the GameInstance should exist at any one time.
        /// </summary>
        private void Awake()
        {
            //Notifies user if GameInstance Singleton is being used improperly
            if (Instance != null)
            {
                Debug.LogError("Found more than one Game Instance in the scene.");
            }
            Instance = this;

            m_stateContext = new GameStateContext(m_loadTitleUpdater);
            ConcretePublisher.Instance.RegisterSubscriber(this);
        }

        /// <summary>
        /// Tracks if inventory system is being used
        /// </summary>
        private void Start()
        {
            m_usesInventory = Inventory.Instance.GetUseInventory(); //Track if Inventory is being used
        }

        /// <summary>
        /// Executes Game Cycle
        /// </summary>
        void Update()
        {
            //Behavior depended on which state the game is currently in
            switch (m_gameState)
            {
                case GameState.LOADTITLE:
                    m_stateContext.SetContext(m_loadTitleUpdater);
                    break;

                case GameState.TITLESCREEN:
                    m_stateContext.SetContext(m_titleScreenUpdater);
                    break;

                case GameState.LOADMAINMENU:
                    m_stateContext.SetContext(m_loadMainMenuUpdater);
                    break;

                case GameState.MAINMENUSCREEN:
                    m_stateContext.SetContext(m_mainMenuScreenUpdater);
                    break;

                case GameState.STARTGAME:
                    m_stateContext.SetContext(m_startGameUpdater);

                    break;

                case GameState.LOADGAME:
                    //Allows game to load before transitioning to PLAYGAME state
                    m_stateContext.SetContext(m_loadGameUpdater);
                    break;

                case GameState.LOADSAVE:
                    //LoadSaveTransition Coroutine CANNOT be started in this section of the switch, or else the coroutine will be started too many times!
                    m_stateContext.SetContext(m_loadSaveUpdater);
                    break;

                case GameState.PLAYGAME:
                    m_stateContext.SetContext(m_playGameUpdater);
                    break;

                case GameState.WINGAME:
                    m_stateContext.SetContext(m_winGameUpdater);
                    break;

                case GameState.LOSEGAME:
                    m_stateContext.SetContext(m_loseGameUpdater);
                    break;

                case GameState.WINSCREEN:
                    m_stateContext.SetContext(m_winScreenUpdater);
                    break;

                case GameState.LOSESCREEN:
                    m_stateContext.SetContext(m_looseScreenUpdater);
                    break;

                default:
                    m_stateContext.SetContext(null);
                    break;
            }

            m_stateContext.UpdateGame();
        }

        #endregion Awake, Start, and Update


        #region Load and Unload Scenes

        /// <summary>
        /// Loads a scene asynchronously
        /// </summary>
        /// <param name="sceneName">The name of the scene being loaded (MUST be the exact name of the scene (case insensitve) or the file path name if two scenes of the same name exist)</param>
        /// <returns>Yield return for Coroutine</returns>
        public IEnumerator LoadScene(string sceneName)
        {
            //Only loads scene if scene is not already loaded
            if(!SceneManager.GetSceneByName(sceneName).IsValid())
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
        public IEnumerator UnloadScene(string sceneName)
        {
            //Only unloads scene if scene is already loaded
            if (SceneManager.GetSceneByName(sceneName).IsValid())
            {
                //Unloads specified scene Async (for better performance)
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }
        }

        #endregion Load and Unload Scenes


        #region Save and Load Data

        /// <summary>
        /// Saves data to the save file
        /// </summary>
        /// <param name="data">GameData object that information is being saved to</param>
        public void SaveData(ref GameData data)
        {
            //Check for key. If key exists, change value to current value, else add key with current value
            if (data.boolData.ContainsKey("GameInstance.ValidSaveFile"))
            {
                data.boolData["GameInstance.ValidSaveFile"] = true;
            }
            else
            {
                data.boolData.Add("GameInstance.ValidSaveFile", true);
            }

            //Save score (if indicated to do so)
            if (m_saveScore)
            {
                //Check floatData for key. If key exists, change value to current value, else add key with current value
                if (data.floatData.ContainsKey("GameInstance.Score"))
                {
                    data.floatData["GameInstance.Score"] = m_score;
                }
                else
                {
                    data.floatData.Add("GameInstance.Score", m_score);
                }
            }
        }

        /// <summary>
        /// Loads data from the save file
        /// </summary>
        /// <param name="data">GameData object that the loaded save file information</param>
        public void LoadData(GameData data)
        {
            //Validate that a save file already exists. If a key exists in the save file, the file has already been saved to. 

            if (data.boolData.ContainsKey("GameInstance.ValidSaveFile"))
            {
                //Track that a valid save file is present.
                m_validSaveFile = data.boolData["GameInstance.ValidSaveFile"];

                //Load score (if applicable)
                if(m_saveScore)
                {
                    //Check if key exists in floatData, if so load score, if not then do nothing
                    if(data.floatData.ContainsKey("GameInstance.Score"))
                    {
                        m_score = data.floatData["GameInstance.Score"];


                        //update UI
                        UserInterface.Instance.UpdateScore(m_score);
                    }
                }
            }
        }

        #endregion Save and Load Data


        #region Pause, Unpause and Inventory

        /// <summary>
        /// If game is in pause menu, hides menu and unpauses game, else displays menu and pauses game
        /// </summary>
        public void TogglePauseMenu()
        {
            if(!m_pauseMenuOpen && !m_inventoryOpen)    //if neither the pause menu nor the inventory are open, opens pause menu
            {
                UserInterface.Instance.m_pauseScreen.SetActive(true);   //Display pause menu
                if (m_saveFromPauseMenu) UserInterface.Instance.m_saveButton.SetActive(true);   //Show save button if applicable

                PauseGame();    //Pause Game

                m_pauseMenuOpen = true;     //tracks that pause menu is now open
            }
            else    //Otherwise, closes pause menu
            {
                UserInterface.Instance.m_pauseScreen.SetActive(false);  //Hide pause menu

                UnpauseGame();  //Unpause Game

                m_pauseMenuOpen = false;    //tracks that pause menu is no longer open

            }
        }

        /// <summary>
        /// If game is in inventory screen, hides inventory and (optionally) unpauses game, else displays inventory and (optionally) pauses game
        /// </summary>
        public void ToggleInventory()
        {
            if(!m_inventoryOpen && !m_pauseMenuOpen)    //if neither the inventory nor the pause menu are open, opens inventory
            {
                UserInterface.Instance.m_inventoryScreen.SetActive(true);   //Display inventory screen

                if(m_inventoryPausesGame)
                {
                    PauseGame();    //Pause game
                }
                else
                {
                    //Unlock Cursor and make cursor visible
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                m_inventoryOpen = true;     //tracks that inventory is now open
            }
            else    //Otherwise, closes inventory
            {
                UserInterface.Instance.m_inventoryScreen.SetActive(false);  //Hide inventory screen
                UserInterface.Instance.m_inventoryMenuScreen.SetActive(false); //Hides inventory menu

                Inventory.Instance.DeselectSelectedItem();      //Deselects selected item in the inventory screen

                if (m_inventoryPausesGame)
                {
                    UnpauseGame();  //Unpause game
                }
                else
                {
                    //Lock Cursor and make cursor invisible
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                m_inventoryOpen = false;    //tracks that inventory is no longer open
            }
        }

        /// <summary>
        /// Pauses game but does not open pause menu
        /// </summary>
        public void PauseGame()
        {
            //Pause Game
            Time.timeScale = 0;
            ConcretePublisher.Instance.Publish("Game Paused");

            //Lower Music Volume
            if(m_playsMusic && m_musicPlayer != null)
            {
                m_musicPlayer.volume = 0.25f;
            }

            //Unlock Cursor and make cursor visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Unpauses game and exits pause menu
        /// </summary>
        public void UnpauseGame()
        {
            //Unpause game
            Time.timeScale = 1;
            ConcretePublisher.Instance.Publish("Game Unpaused");


            //Raise Music Volume
            if (m_playsMusic && m_musicPlayer != null)
            {
                m_musicPlayer.volume = 0.5f;
            }

            //Lock Cursor and make cursor invisible
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion Pause, Unpause and Inventory


        #region UI Updates

        /// <summary>
        /// Updates player health in the UI
        /// </summary>
        /// <param name="currentHealth">Current player character health</param>
        /// <param name="maxHealth">Player character max health</param>
        public void UpdatePlayerHealth(float currentHealth, float maxHealth)
        {
            UserInterface.Instance.UpdateHealthBar(currentHealth, maxHealth);
        }

        /// <summary>
        /// Updates player lives in the UI
        /// </summary>
        /// <param name="lives">Current number of player lives</param>
        public void UpdatePlayerLives(int lives)
        {
            UserInterface.Instance.UpdatePlayerLives(lives);
        }

        /// <summary>
        /// Adds an prompter to the list of active prompters
        /// </summary>
        /// <param name="prompter">Prompter being added</param>
        public void AddToActivePrompts(IPrompter prompter)
        {
            m_activePrompters.Add(prompter);
        }

        /// <summary>
        /// Removes a prompter from the list of active prompters
        /// </summary>
        /// <param name="prompter">Prompter being removed</param>
        public void RemoveFromActivePrompts(IPrompter prompter)
        {
            m_activePrompters.Remove(prompter);
        }

        

        #endregion UI Updates


        #region Score

        /// <summary>
        /// Changes the score. Positive inputs will add to the score, negative inputs will detract from it.
        /// </summary>
        /// <param name="score">How much to add to the score. Negative integers will subtract.</param>
        public void AddOrRemoveScore(float score)
        {
            //Add to or subtract from score and clamp score (score cannot be negative)
            m_score = (m_score + score > 0)? m_score + score : 0;


            //Notify UI of the change
            UserInterface.Instance.UpdateScore(m_score);
        }

        #endregion Score


        #region Player Death, Respawn and Restart

        /// <summary>
        /// Transitions to next stage of game (afer player death)
        /// </summary>
        /// <returns>Yield return for Coroutine</returns>
        public IEnumerator OnPlayerDeath()
        {
            yield return new WaitForSeconds(m_deathTransitionTimer); //Wait so that death animation can finish
        
            //Account for player lives
        
            if(m_playerScript.GetLives() > 1)
            {
                //Respawns player
                StartCoroutine(OnPlayerRespawn());
            }
            else
            {
                m_gameState = GameState.LOSEGAME;   //transition to lose game state
            }
        }

        /// <summary>
        /// Respawns Player
        /// </summary>
        /// <returns>Yield return for coroutine</returns>
        public IEnumerator OnPlayerRespawn()
        {
            //Behavior dependent on respawn type
            switch (m_respawnType)
            {
                case RespawnType.RESPAWNINPLACE:
                    //Respawns player character in the same location as where they died
                    m_playerScript.OnRespawn(m_respawnInvincibilityTimer);
                    ConcretePublisher.Instance.Publish("Player Alive");
                    break;
        
                case RespawnType.LOADLASTSAVE:
                    //Respawns player by loading the save file. This will, however, still decrease the number of lives the player has.
                    yield return StartCoroutine(UserInterface.Instance.FadeOut());
                    StartCoroutine(LoadSaveTransition());
                    m_gameState = GameState.LOADSAVE;
                    m_playerScript.OnRespawn(m_respawnInvincibilityTimer);
                    break;
        
                case RespawnType.RESPAWNATSAVELOCATION:
                    //Respawns player character at location of last loaded save, without loading any other data from that save.
        
                    //Find player spawn point, and relocate player character to that point.
                    PlayerSpawnPoint spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();
                    if (spawnPoint != null)
                    {
                        spawnPoint.LoadData(DataPersistenceManager.Instance.GetData());
                        m_playerScript.SetPlayerTransform(spawnPoint.transform.position, spawnPoint.transform.rotation);
                        m_playerScript.OnRespawn(m_respawnInvincibilityTimer);
                        ConcretePublisher.Instance.Publish("Player Alive");
                    }
                    else
                    {
                        //Notify user if there is no player spawn point at which to spawn the player
                        Debug.LogError("No PlayerSpawnPoint was located in the scene when respawning! Player cannot respawn!");
                    }
        
                    break;
        
                case RespawnType.RESPAWNATSTATICLOCATION:
                    //Respawns player at a static location
                    bool spawnPointFound = false;
                    StaticSpawnPoint[] spawns = FindObjectsByType<StaticSpawnPoint>(FindObjectsSortMode.None); //find all static spawn points in the loaded scene
        
                    //search for the first static spawn point configured for the player
                    foreach (StaticSpawnPoint spawn in spawns)
                    {
                        if (spawn.spawnTag == "Player")
                        {
                            //respawn player at that static spawn point
                            spawnPointFound = true;
                            m_playerScript.SetPlayerTransform(spawn.transform.position, spawn.transform.rotation);
                            m_playerScript.OnRespawn(m_respawnInvincibilityTimer);
                            ConcretePublisher.Instance.Publish("Player Alive");

                            break;
                        }
                    }
        
                    //Notify user if static spawn point configued for player has not been found
                    if (!spawnPointFound) Debug.LogError("No StaticSpawnPoint was located in the scene with tag 'Player'! Player cannot respawn!");
        
                    break;
        
                default:
                    break;
            }
        
            m_playerScript.AddOrReduceLives(-1);    //Reduce player lives by one (executed here so that the "load last save" respawn type will still evaluate number of lives left correctly
        }

        /// <summary>
        /// Restarts game. Intended for use from the LoseScreen of the UI
        /// </summary>
        public void RestartFromScreen()
        {
            //Behavior varies depending on Restart Mode
            switch (m_restartMode)
            {
                case RestartMode.RESTARTFROMLASTSAVE:
                    //Restarts game from the last save by loading the last save
                    m_loadOnPlay = true;
                    m_gameState = GameState.STARTGAME;
                    break;

                case RestartMode.RESTARTFROMBEGINNING:
                    //Restarts game from the beginning without loading any saved data
                    m_loadOnPlay = false;
                    m_gameState = GameState.STARTGAME;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Restarts game. Intended for use from the PauseMenu of the UI
        /// </summary>
        public void RestartFromGame()
        {
            //Reloads game scene, and restarts game from the beginning.
            //Effectively does the same thing as starting a "New Game" from the main menu.
            m_restartingGame = true;
            m_loadOnPlay = false;
            m_gameState = GameState.STARTGAME;
        }

        #endregion Player Death, Respawn and Restart


        #region State Transitioning

        /// <summary>
        /// Transitions from a new game to a game loaded from a save file
        /// </summary>
        /// <returns>Yield return for coroutine</returns>
        public IEnumerator LoadSaveTransition()
        {
            //Can't load a save file that doesn't exist, and we don't want to load everytime the
            //coroutine executes. Only the first time after it is started
            if (m_validSaveFile && !m_saveHasAlreadyLoaded) 
            {
                //Clear inventory to prepare for loading the save file
                Inventory.Instance.ClearInventory();


                //Loads game from Data Manager
                DataPersistenceManager.Instance.LoadGame();


                //Once game is loaded, moves player to the current transform of the Player Spawn Point
                PlayerSpawnPoint spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();
                if (spawnPoint != null)
                {
                    m_playerScript.SetPlayerTransform(spawnPoint.transform.position, spawnPoint.transform.rotation);
                }
                else
                {
                    //Notifies user if there is no Player Spawn Point in the scene after loading the data
                    Debug.LogError("No PlayerSpawnPoint was located in the scene when loading! Player will not be moved correctly!");
                }


                m_saveHasAlreadyLoaded = true;  //prevents this code from executing more than once while the coroutine executes
            }

            //Check for fade in completed
            if(!m_FadeInCompleted)
            {
                //Fade in
                yield return StartCoroutine(UserInterface.Instance.FadeIn());

                m_FadeInCompleted = true;   //indicate fade in has completed
            }


           
            //With the coroutine finished executing, set this back to false so
            //that the coroutine executes all code the next time it is started
            m_saveHasAlreadyLoaded = false;
            m_FadeInCompleted = false;

            ConcretePublisher.Instance.Publish("Player Alive");   
            m_gameState = GameState.PLAYGAME;   //Transition to "Play Game" game state
        }

        #endregion State Transitioning


        #region Spawn and Manage Objects

        /// <summary>
        /// Spawns an object using the GameInstance as the parent
        /// </summary>
        /// <param name="objectPrefab">Prefab of the object being spawned</param>
        /// <returns>The object being spawned</returns>
        public GameObject SpawnObjectInWorld(GameObject objectPrefab)
        {
            //if a null object prefab was provided, return a null object
            if (objectPrefab == null) return null;

            //create/spawn object using the game manager's transform as the parent transform
            GameObject gameObject = GameObject.Instantiate(objectPrefab, transform);

            //return the spawned game object
            return gameObject;
        }

        /// <summary>
        /// Plays a sound at a specific location
        /// </summary>
        /// <param name="audioClip">AudioClip of the sound to be played</param>
        /// <param name="location">Vector3 of the location to play sound at</param>
        /// <returns>The SpawnableSound object playing the sound</returns>
        public GameObject SpawnSoundAtLocation(AudioClip audioClip, Vector3 location)
        {
            //if audio clip provided is null, returns a null game object
            if(audioClip == null) return null;

            //Creates a SpawnableSound at the desired location
            GameObject soundObject = GameObject.Instantiate(m_SpawnableSound, transform);

            //Sets up and starts the SpawnableSound
            SpawnableSound spawnableSound = soundObject.GetComponent<SpawnableSound>();
            spawnableSound.SetAudio(audioClip);
            spawnableSound.SetLifespan(audioClip.length + 1);
            spawnableSound.PlayAudio();

            //Starts the lifespan timer of the SpawnableSound so that it will despawn when the sound has finished playing
            spawnableSound.StartLifespanTimer();

            //returns SpawnableSound that is now playing the specified sound at the specified location
            return soundObject;
        }

        /// <summary>
        /// Finds an AmmunitionTracker that has ammunition of a specified name
        /// </summary>
        /// <param name="ammunitionName">Name of the ammunition whose tracker is being searched for</param>
        /// <returns>The first AmmunitionTracker tracker with the proper name that is found, or null if no such tracker is found</returns>
        public AmmunitionTracker FindAmmunitionTracker(string ammunitionName)
        {
            //Create a null AmmunitionTracker
            AmmunitionTracker tracker = null;

            //Search through list of all AmmunitionTrackers
            foreach(AmmunitionTracker aT in m_ammunitionList)
            {
                //Check if AmmunitionTracker is the correct one
                if(aT.GetAmmunition().GetName() == ammunitionName)
                {
                    tracker = aT;   //Keep reference to tracker
                    break;  //leave loop now that a proper tracker has been found
                }
            }

            return tracker; //return the tracker that was found, or null if none were found
        }

        #endregion Spawn and Manage Objects
        
        
        public void UpdateState(string newState)
        {
            switch (newState)
            {
                case "Player Dead":
                    StartCoroutine(OnPlayerDeath());
                    break;
                case "Restarting Game":
                    m_restartingGame = true;
                    break;
                case "Not Restarting Game":
                    m_restartingGame = false;
                    break;
                default:
                    break;
            }
        }

    }
}
