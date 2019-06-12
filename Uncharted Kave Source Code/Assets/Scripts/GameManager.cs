using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


/// <summary>
/// State machine that handles the game logic
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Drag and drop fields

    // Other game objects
    [Tooltip("Game title text mesh")]
    public TextMesh gameTitle;

    [Tooltip("Afektiva Games logo gameobject")]
    public GameObject afektivagames;

    [Tooltip("Textual Instructions")]
    public GameObject instructions;

    [Tooltip("Demonstration Video gameobject")]
    public GameObject DemoVideoParent;

    [Tooltip("Press to continue button gameobject")]
    public GameObject continueButton;

    [Tooltip("Puzzle world transition animator")]
    public Animator puzzleWorldTransitionAnimator;

    [Tooltip("Puzzle world door animator")]
    public Animator puzzleWorldDoorAnimator;

    [Tooltip("Victory world door animator")]
    public Animator victoryWorldDoorAnimator;

    [Tooltip("Chest animator")]
    public Animator chestAnimator;

    [Tooltip("Timer countdown gameobject")]
    public TextMesh countdown;

    [Tooltip("Code display gameobject")]
    public TextMesh codeDisplay;

    [Tooltip("Demonstration video parent gameobject")]
    public VideoPlayer DemoVideo;


    // Worlds
    [Tooltip("Main menu world")]
    public GameObject mainMenuWorld;

    [Tooltip("Puzzle world")]
    public GameObject puzzleWorld;

    [Tooltip("Defeat world")]
    public GameObject defeatWorld;

    [Tooltip("Victory world")]
    public GameObject victoryWorld;

    [Tooltip("Puzzle levels' parent gameobject")]
    public GameObject _puzzleLevelsParent;

    // Audio
    [Tooltip("Game over audio clip")]
    public AudioClip gameOverSound;

    [Tooltip("Treasure chest audio clip")]
    public AudioClip treasureChestSound;

    [Tooltip("Right answer audio clip")]
    public AudioClip rightAnswerSound;

    [Tooltip("Wrong answer audio clip")]
    public AudioClip wrongAnswerSound;

    [Tooltip("Door opening audio clip")]
    public AudioClip doorOpeningSound;

    [Tooltip("Rocks falling audio clip")]
    public AudioClip rocksFallingSound;

    [Tooltip("Wind blowing audio clip")]
    public AudioClip windBlowingSound;

    [Tooltip("Water dripping audio clip")]
    public AudioClip waterDrippingSound;

    [Tooltip("Mice squeaking audio clip")]
    public AudioClip miceSqueakingSound;

    [Tooltip("Bats flying audio clip")]
    public AudioClip batsFlyingSound;

    [Tooltip("Audio source")]
    public AudioSource audioSource;


    // Lights
    [Tooltip("Point light behind chest")]
    public Light chestLight;

    [Tooltip("Player 1 wants to play again light - Defeat")]
    public Light player1DefeatLight;

    [Tooltip("Player 2 wants to play again light - Defeat")]
    public Light player2DefeatLight;

    [Tooltip("Player 1 wants to play again light - Victory")]
    public Light player1VictoryLight;

    [Tooltip("Player 2 wants to play again light - Victory")]
    public Light player2VictoryLight;

    #endregion


    // States
    private Dictionary<string, IState> _states = new Dictionary<string, IState>();
    private IState _current;


    // Flags
    private bool _timerActive;
    public bool CooldownActive { get; private set; }


    // Others
    private const int _MAX_TIME = 300;
    private float _time;
    private const int _COOLDOWN_TIME = 1;
    private string _currentWorld;
    private bool _sounds_played_1;
    private bool _sounds_played_2;
    private bool _sounds_played_3;
    private bool _sounds_played_4;
    private bool _sounds_played_5;
    private bool _sounds_played_6;


    /// <summary>
    /// Called by a button when its collision box is entered
    /// </summary>
    public bool TouchButton(string button_name)
    {
        // Returns true if a button was pressed successfully
        return _current.TouchButton(button_name);
    }


    /// <summary>
    /// Changes the state machine's current state
    /// </summary>
    public void SetState(string state)
    {
        _current = _states[state];

        // Let the new current state set itself up
        _current.Setup();

        //Debug.Log("State: " + _current);
    }


    /// <summary>
    /// Changes the active game world
    /// </summary>
    public void ChangeWorld(string aWorld)
    {
        // If the given world is already active then return
        if (_currentWorld == aWorld)
            return;

        // Update the current world
        _currentWorld = aWorld;

        // Disable the current world game object
        mainMenuWorld.SetActive(false);
        puzzleWorld.SetActive(false);
        defeatWorld.SetActive(false);
        victoryWorld.SetActive(false);
        
        // Activate the new world game object
        switch (_currentWorld)
        {
            case "mainMenuWorld": mainMenuWorld.SetActive(true); break;
            case "puzzleWorld": puzzleWorld.SetActive(true); break;
            case "defeatWorld": defeatWorld.SetActive(true); break;
            case "victoryWorld": victoryWorld.SetActive(true); break;
            default: break;
        }
    }


    /// <summary>
    /// Starts playing the instructions video
    /// </summary>
    public void StartVideo()
    {
        instructions.SetActive(false);
        continueButton.transform.GetChild(0).GetComponent<TextMesh>().text = "Start game";
        DemoVideoParent.SetActive(true);
        DemoVideo.Play();
    }


    /// <summary>
    /// Stops playing the instructions video
    /// </summary>
    public void StopVideo()
    {
        afektivagames.SetActive(false);
        continueButton.SetActive(false);
        DemoVideoParent.SetActive(false);
        continueButton.transform.GetChild(0).GetComponent<TextMesh>().text = "Press to continue";
        DemoVideo.Stop();
    }


    /// <summary>
    /// Starts the door timer
    /// </summary>
    public void StartTimer()
    {
        // Call the "Timeout" method after "_MAX_TIME" seconds pass. Game is over
        Invoke("Timeout", _MAX_TIME);

        // Update flag. Enables timer and countdown updating
        _timerActive = true;
    }


    /// <summary>
    /// Called when the door timer has finished. Changes state to the defeat state
    /// </summary>
    private void Timeout()
    {
        // The players failed to escape the cave, change the current state
        SetState("DefeatState");
    }


    /// <summary>
    /// Resets all audio flags, allowing audio clips to be played again during the game
    /// </summary>
    public void ResetAudioFlags()
    {
        _sounds_played_1 = false;
        _sounds_played_2 = false;
        _sounds_played_3 = false;
        _sounds_played_4 = false;
        _sounds_played_5 = false;
        _sounds_played_6 = false;
    }


    /// <summary>
    /// Cancels all scheduled tasks and resets the timer and cooldown
    /// </summary>
    public void ResetScheduledTasks()
    {
        CancelInvoke();
        _timerActive = false;
        _time = _MAX_TIME;
        CooldownActive = false;
    }


    /// <summary>
    /// Starts a cooldown that prevents the user from clicking another keypad key
    /// </summary>
    public void StartCooldown()
    {
        CooldownActive = true;
        Invoke("OnCooldownOver", _COOLDOWN_TIME);
    }


    /// <summary>
    /// Updates the cooldown flag after it is over
    /// </summary>
    private void OnCooldownOver()
    {
        CooldownActive = false;
    }


    /// <summary>
    /// Updates the code display with the current code
    /// </summary>
    /// <param name="code"></param>
    public void UpdateCodeDisplay(string code)
    {
        switch(code.Length)
        {
            case 0: codeDisplay.text = "_ _ _"; break;
            case 1: codeDisplay.text = code + " _ _"; break;
            case 2: codeDisplay.text = code[0] + " " + code[1] + " _"; break;
            case 3: codeDisplay.text = code[0] + " " + code[1] + " " + code[2]; break;
            default: break;
        }
    }


    /// <summary>
    /// Plays a given audio clip
    /// </summary>
    public void PlaySound(AudioClip audioClip)
    {
        if (audioSource != null && audioClip != null)
            audioSource.PlayOneShot(audioClip, 1f);
    }


    /// <summary>
    /// Plays a given audio clip at a given position (used for surround sound)
    /// </summary>
    public void PlaySoundAtPosition(AudioClip audioClip, Vector3 position)
    {
        if (audioClip == null)
            return;

        AudioSource.PlayClipAtPoint(audioClip, position);
    }


    /// <summary>
    /// Plays a given audio clip at a random position (used for surround sound)
    /// </summary>
    public void PlaySoundAtRandomPosition(AudioClip audioClip)
    {
        if (audioClip == null)
            return;

        // Play the audio clip in or around the cave, in the cubic region of space with corner points (-3, -2, -3) to (3, 2, 3)
        // Intended to feel like cave noises are just on the other side of the walls and around players
        // Game world's center is at +- (0, 1.1, 0)
        AudioSource.PlayClipAtPoint(audioClip, new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, 2f), Random.Range(-3f, 3f)));
    }


    // Not implemented
    void Awake()
    {
        
    }


    /// <summary>
    /// Initialize the game state machine and variables
    /// </summary>
    void Start()
    {
        // Initialize and add states to the state machine

        // First game state. In this state, the game is in standby waiting for players to enter the KAVE.
        _states.Add("SplashScreen", new SplashScreen(this));

        // Second game state. In this state, the splash screen animation is played before changing to the main menu state
        _states.Add("SplashScreenAnimation", new SplashScreenAnimation(this, chestLight, gameTitle, afektivagames, instructions, continueButton, treasureChestSound, chestAnimator));

        // Third game state. In this state, the game is displaying the instructions and waiting for the players to start the game
        _states.Add("MainMenu", new MainMenu(this, chestLight, rocksFallingSound));

        // Fourth game state. In this state, the main menu animation is played before changing to the puzzle state
        _states.Add("MainMenuAnimation", new MainMenuAnimation(this, chestLight, gameTitle));

        // Fifth game state. In this state, puzzles are presented to the players
        _states.Add("Puzzle", new Puzzle(this, puzzleWorldTransitionAnimator, puzzleWorldDoorAnimator, _puzzleLevelsParent, rightAnswerSound, wrongAnswerSound, doorOpeningSound));

        // Possible sixth game state. In this state, the victory world is presented to the players
        _states.Add("VictoryState", new VictoryState(this, player1VictoryLight, player2VictoryLight, victoryWorldDoorAnimator, doorOpeningSound));

        // Possible sixth game state. In this state, the defeat world is presented to the players
        _states.Add("DefeatState", new DefeatState(this, player1DefeatLight, player2DefeatLight, gameOverSound));

        // Initialize flags
        _timerActive = false;
        CooldownActive = false;

        // Initialize others
        _time = _MAX_TIME;
        _currentWorld = "mainMenuWorld";

        // Set the initial state
        SetState("SplashScreen");
    }


    /// <summary>
    /// Updates the current state. If the timer is active, updates it and plays sounds at specific times.
    /// </summary>
    void Update()
    {
        // Let the current state update itself
        _current.Update();

        // If the timer is active we want to update it and the countdown display, and play sounds at specific times
        if (_timerActive)
        {
            // Update time
            _time -= Time.deltaTime;

            if (_time <= 0)
            {
                countdown.text = "";
                return;
            }

            // Convert the time from seconds to the correct formatting
            int current_seconds = (int) _time;
            int current_minutes = (int) _time / 60;
            current_seconds -= current_minutes * 60;

            // Format
            if(current_seconds >= 10)
                countdown.text = current_minutes + " : " + current_seconds;
            else
                countdown.text = current_minutes + " : 0" + current_seconds;

            // Play sounds at specific times
            // Flags prevent the respective sounds from being played more than once
            if (_time < 10 && !_sounds_played_1)
            {
                // Play sounds
                PlaySoundAtRandomPosition(miceSqueakingSound);
                PlaySoundAtRandomPosition(rocksFallingSound);
                PlaySoundAtRandomPosition(windBlowingSound);
                PlaySoundAtRandomPosition(waterDrippingSound);
                PlaySoundAtRandomPosition(batsFlyingSound);

                // Prevent these sounds from being played again
                _sounds_played_1 = true;
            }
            else if (_time < 30 && !_sounds_played_2)
            {
                PlaySoundAtRandomPosition(windBlowingSound);
                PlaySoundAtRandomPosition(rocksFallingSound);
                PlaySoundAtRandomPosition(batsFlyingSound);
                _sounds_played_2 = true;
            }
            else if (_time < 70 && !_sounds_played_3)
            {
                PlaySoundAtRandomPosition(rocksFallingSound);
                PlaySoundAtRandomPosition(miceSqueakingSound);
                _sounds_played_3 = true;
            }
            else if (_time < 120 && !_sounds_played_4)
            {
                PlaySoundAtRandomPosition(waterDrippingSound);
                PlaySoundAtRandomPosition(rocksFallingSound);
                PlaySoundAtRandomPosition(batsFlyingSound);
                _sounds_played_4 = true;
            }
            else if (_time < 200 && !_sounds_played_5)
            {
                PlaySoundAtRandomPosition(rocksFallingSound);
                PlaySoundAtRandomPosition(windBlowingSound);
                PlaySoundAtRandomPosition(miceSqueakingSound);
                _sounds_played_5 = true;
            }
            else if (_time < 280 && !_sounds_played_6)
            {
                PlaySoundAtRandomPosition(waterDrippingSound);
                PlaySoundAtRandomPosition(windBlowingSound);
                PlaySoundAtRandomPosition(batsFlyingSound);
                _sounds_played_6 = true;
            }
        }
    }
}


// Interface implemented by all states
interface IState
{
    void Setup();
    bool TouchButton(string button_name);
    void Update();
}


/// <summary>
/// First game state. In this state, the game is in standby waiting for players to enter the KAVE
/// </summary>
public class SplashScreen : IState
{
    private GameManager _machine;
    private string _buttonName; // Name of the button that can be interacted with in this state

    // Initialize fields
    public SplashScreen(GameManager gsm)
    {
        _machine = gsm;
        _buttonName = "SplashScreenTrigger";
    }


    /// <summary>
    /// Sets up the splash screen state
    /// </summary>
    public void Setup()
    {
        // Make sure the correct world is active
        _machine.ChangeWorld("mainMenuWorld");
    }


    /// <summary>
    /// Checks if a button that was touched is this state's interactable button
    /// </summary>
    public bool TouchButton(string button_name)
    {
        // If it's not the same button then return false (button press wasn't handled)
        if (button_name != _buttonName)
            return false;

        // Change the current state, triggering the splash screen animation
        _machine.SetState("SplashScreenAnimation");
        return true;
    }


    // Not Implemented
    public void Update()
    {
        
    }
}


/// <summary>
/// Second game state. In this state, the splash screen animation is played before changing to the main menu state
/// </summary>
public class SplashScreenAnimation : IState
{
    // Light constants
    private const float _MAX_INTENSITY = 5;
    private const float _INTENSITY_INCREASE_FACTOR = 2;

    // Title constants
    private const float _MAX_ALPHA = 1;
    private const float _ALPHA_INCREASE_FACTOR = 1;

    // Variables
    private GameManager _machine;
    private Light _chestLight;
    private TextMesh _gameTitle;
    private GameObject _afektivagames;
    private GameObject _instructions;
    private GameObject _continueButton;
    private AudioClip _treasureChestSound;
    private Animator _chestAnimator;

    // Initialize fields
    public SplashScreenAnimation(GameManager gsm, Light chestLight, TextMesh gameTitle, GameObject afektivagames, GameObject instructions, GameObject continueButton, AudioClip treasureChestSound, Animator chestAnimator)
    {
        _machine = gsm;
        _chestLight = chestLight;
        _gameTitle = gameTitle;
        _afektivagames = afektivagames;
        _instructions = instructions;
        _continueButton = continueButton;
        _treasureChestSound = treasureChestSound;
        _chestAnimator = chestAnimator;
    }


    /// <summary>
    /// Sets up the splash screen animation state
    /// </summary>
    public void Setup()
    {
        _machine.ChangeWorld("mainMenuWorld");
        _afektivagames.SetActive(false);
        _instructions.SetActive(false);
        _continueButton.SetActive(false);
        RenderSettings.ambientIntensity = 0.7f;
        _chestAnimator.Play("Chest_open", -1, 0f);
        _machine.PlaySoundAtPosition(_treasureChestSound, _chestLight.transform.position);
    }


    // Not Implemented
    public bool TouchButton(string button_name)
    {
        return false;
    }


    /// <summary>
    /// Updates this state. Used to play the splash screen animation.
    /// </summary>
    public void Update()
    {
        // If the chest light's intensity isn't at its max yet then increase it according to the delta time
        if (_chestLight.intensity < _MAX_INTENSITY)
            _chestLight.intensity += _INTENSITY_INCREASE_FACTOR * Time.deltaTime;

        // If the chest light's intensity is at its max, increase the game title's alpha
        else if (_gameTitle.color.a + (_ALPHA_INCREASE_FACTOR * Time.deltaTime) < _MAX_ALPHA)
        {
            Color c = _gameTitle.color;
            c.a += _ALPHA_INCREASE_FACTOR * Time.deltaTime;
            _gameTitle.color = c;
        }

        // If the game title's alpha is at its max, enable the instructions, start the instructions video and change state
        else
        {
            Color c = _gameTitle.color;
            c.a = _MAX_ALPHA;
            _gameTitle.color = c;
            _afektivagames.SetActive(true);
            _instructions.SetActive(true);
            _continueButton.SetActive(true);
            _machine.SetState("MainMenu");
        }
    }
}


/// <summary>
/// Third game state. In this state, the game is displaying the instructions and waiting for the players to start the game
/// </summary>
public class MainMenu : IState
{
    // Light constants
    private const float _MAX_INTENSITY = 5;
    private const float _INTENSITY_INCREASE_FACTOR = 2;
    private const float _MIN_INTENSITY = 1.7f;
    private const float _INTENSITY_DECREASE_FACTOR = 1;

    // Variables
    private string _buttonName;
    private GameManager _machine;
    private Light _chestLight;
    private AudioClip _rocksfallingsound;

    // Flags
    private bool _increasing;
    private bool _pressedNext;

    // Initialize fields
    public MainMenu(GameManager gsm, Light chestLight, AudioClip rocksfallingsound)
    {
        _machine = gsm;
        _buttonName = "ContinueButton";
        _chestLight = chestLight;
        _rocksfallingsound = rocksfallingsound;
        _increasing = false;
        _pressedNext = false;
    }

    
    /// <summary>
    /// Sets up the main menu state
    /// </summary>
    public void Setup()
    {
        _increasing = false;
        _pressedNext = false;
        _machine.ChangeWorld("mainMenuWorld");
    }


    /// <summary>
    /// Checks if a button that was touched is this state's interactable button
    /// </summary>
    public bool TouchButton(string button_name)
    {
        // If it's not the same button then return false (button press wasn't handled)
        if (button_name != _buttonName)
            return false;

        // If a button was just pressed then ignore this press
        if (_machine.CooldownActive)
            return false;

        // If this press counted then start a cooldown
        _machine.StartCooldown();

        // If the textual instructions are being displayed, a button press hides them and displays the video
        if (!_pressedNext)
        {
            _machine.StartVideo();
            _pressedNext = true;
        }
        // Change the current state, triggering the main menu animation
        else
        {
            _machine.StopVideo();
            _machine.SetState("MainMenuAnimation");
            _machine.PlaySound(_rocksfallingsound);
        }

        return true;
    }


    /// <summary>
    /// Updates this state. Oscillates the chest light's intensity, making it look like it's shining
    /// </summary>
    public void Update()
    {
        // If the intensity is currently increasing then keep increasing it until it reaches its max
        if(_increasing)
        {
            // If it reached its max then update flag to start decreasing
            if (_chestLight.intensity + (_INTENSITY_INCREASE_FACTOR * Time.deltaTime) >= _MAX_INTENSITY)
            {
                _chestLight.intensity = _MAX_INTENSITY;
                _increasing = false;
            }

            // Else keep increasing it according to the increase factor and the delta time
            else
                _chestLight.intensity += _INTENSITY_INCREASE_FACTOR * Time.deltaTime;
        }

        // If the intensity is currently decreasing then keep decreasing it until it reaches its min
        else
        {
            if (_chestLight.intensity - (_INTENSITY_DECREASE_FACTOR * Time.deltaTime) <= _MIN_INTENSITY)
            {
                _chestLight.intensity = _MIN_INTENSITY;
                _increasing = true;
            }
            else
                _chestLight.intensity -= _INTENSITY_DECREASE_FACTOR * Time.deltaTime;
        }
    }
}


/// <summary>
/// Fourth game state. In this state, the main menu animation is played before changing to the puzzle state
/// </summary>
public class MainMenuAnimation : IState
{
    // Light constants
    private const int _MIN_INTENSITY = 0;
    private const float _INTENSITY_DECREASE_FACTOR = 4;

    // Title constants
    private const int _MIN_ALPHA = 0;
    private const float _ALPHA_DECREASE_FACTOR = 1;

    // Variables
    private GameManager _machine;
    private Light _chestLight;
    private TextMesh _gameTitle;
    
    // Initialize fields
    public MainMenuAnimation(GameManager gsm, Light chestLight, TextMesh gameTitle)
    {
        _machine = gsm;
        _chestLight = chestLight;
        _gameTitle = gameTitle;
    }


    // Not implemented
    public void Setup()
    {
        
    }


    // Not Implemented
    public bool TouchButton(string button_name)
    {
        return false;
    }


    /// <summary>
    /// Updates this state. Used to play the main menu animation.
    /// </summary>
    public void Update()
    {
        // If the intensity is above its min then decrease it according to delta time
        if (_chestLight.intensity > _MIN_INTENSITY)
            _chestLight.intensity -= _INTENSITY_DECREASE_FACTOR * Time.deltaTime;

        // If the intensity reached its min, if the game title's alpha is above its min then decrease it and also the global lighting
        else if (_gameTitle.color.a - (_ALPHA_DECREASE_FACTOR * Time.deltaTime) > _MIN_ALPHA)
        {
            Color c = _gameTitle.color;
            c.a -= _ALPHA_DECREASE_FACTOR * Time.deltaTime;
            _gameTitle.color = c;

            if (RenderSettings.ambientIntensity - (_ALPHA_DECREASE_FACTOR * Time.deltaTime) > 0f)
                RenderSettings.ambientIntensity -= _ALPHA_DECREASE_FACTOR * Time.deltaTime;
            else
                RenderSettings.ambientIntensity = 0f;
        }

        // If the game title's alpha reached its min, change to the puzzle state
        else
        {
            Color c = _gameTitle.color;
            c.a = _MIN_ALPHA;
            _gameTitle.color = c;
            _machine.SetState("Puzzle");
        }
    }
}


/// <summary>
/// Fifth game state. In this state, puzzles are presented to the players
/// </summary>
public class Puzzle : IState
{
    // Constants
    private const int _NUMBER_LEVELS = 3; // Number of puzzles to be played. If it's lower than the number of levels in the hierarchy then the puzzles are selected randomly from those
    private const float _INTENSITY_INCREASE_FACTOR = 0.1f;
    private const float _INTENSITY_DECREASE_FACTOR = 0.5f;

    // Puzzle List
    private List<Level> _levels;

    // Flags
    private bool _gameOver;
    private bool _fadeOut;
    private bool _transition;

    // Puzzles
    private GameObject _puzzleLevelsParent;

    // Others
    private float _MAX_LIGHTING;
    private string _code;
    private GameManager _machine;
    private Animator _transitionAnimator;
    private Animator _doorAnimator;
    private AudioClip _rightAnswerSound;
    private AudioClip _wrongAnswerSound;
    private AudioClip _doorOpeningSound;


    // Initialize fields
    public Puzzle(GameManager gsm, Animator transitionAnimator, Animator doorAnimator, GameObject puzzleLevelsParent, AudioClip rightAnswerSound, AudioClip wrongAnswerSound, AudioClip doorOpeningSound)
    {
        _machine = gsm;
        _transitionAnimator = transitionAnimator;
        _doorAnimator = doorAnimator;
        _code = "";
        _fadeOut = false;
        _transition = false;
        _puzzleLevelsParent = puzzleLevelsParent;
        _rightAnswerSound = rightAnswerSound;
        _wrongAnswerSound = wrongAnswerSound;
        _doorOpeningSound = doorOpeningSound;
        _levels = new List<Level>();
    }


    /// <summary>
    /// Gets all the levels in the hierarchy and adds them to the list of puzzles to be played
    /// </summary>
    private void GetLevels()
    {
        // Disable all levels in the list (there might be some in case a game was lost previously)
        for (int i = 1; i < _levels.Count; i++)
            _levels[i].GetLevelWorld().SetActive(false);

        _levels = new List<Level>();

        foreach (Transform levelWorld in _puzzleLevelsParent.transform)
            _levels.Add(new Level(levelWorld.gameObject));

        System.Random rand = new System.Random();

        while (_levels.Count > _NUMBER_LEVELS)
            _levels.RemoveAt(rand.Next(0, _NUMBER_LEVELS));
    }


    /// <summary>
    /// Sets up the puzzle state
    /// </summary>
    public void Setup()
    {
        // Make sure the correct world is active
        _machine.ChangeWorld("puzzleWorld");

        // Reset all scheduled tasks and audio flags (might have been used in a previous game)
        _machine.ResetScheduledTasks();
        _machine.ResetAudioFlags();

        // Start the timer
        _machine.StartTimer();

        // Get all levels and activate the first one
        GetLevels();

        for (int i = 1; i < _levels.Count; i++)
            _levels[i].GetLevelWorld().SetActive(false);

        _levels[0].GetLevelWorld().SetActive(true);

        // Set up the lighting intensity
        _MAX_LIGHTING = 0.7f;
        RenderSettings.ambientIntensity = _MAX_LIGHTING;

        _code = "";
        _fadeOut = false;
        _transition = false;
    }


    /// <summary>
    /// Checks if a button that was touched is this state's interactable button
    /// </summary>
    public bool TouchButton(string button_name)
    {
        // Ignore this press if a transition between puzzles is happening
        if (_transition && !_fadeOut)
            return false;

        // If a button was just pressed then ignore this press
        if (_machine.CooldownActive)
            return false;

        // If this press counted then start a cooldown
        _machine.StartCooldown();

        // If it was a keypad's key then that touch is handled by this state
        if (button_name.StartsWith("Key_"))
        {
            button_name = button_name.Substring(4, button_name.Length - 4);

            if (button_name == "clear")
                _code = "";

            // Update the code
            else
                _code += button_name;
        }
        else
            return false;

        // If the code is complete then check if its correct
        if(_code.Length >= 3)
        {
            // If it's correct then provide audio feedback and open the door
            if (_levels[0].CheckCode(_code))
            {
                _machine.PlaySound(_rightAnswerSound);
                _code = "";

                // If it's not the last puzzle then we open the door. If it is we will change to the victory state later
                if (_levels.Count > 1)
                {
                    _doorAnimator.Play("puzzle_door", -1, 0f);
                    _machine.PlaySoundAtPosition(_doorOpeningSound, _doorAnimator.transform.position);
                }

                // Trigger a transition animation
                _transition = true;
                _fadeOut = true;
            }

            // If it's incorrect then provide appropriate feedback and reset the code
            else
            {
                _machine.PlaySound(_wrongAnswerSound);
                _code = "";
            }
        }

        // Update the code display
        _machine.UpdateCodeDisplay(_code);

        return true;
    }


    /// <summary>
    /// Updates the puzzle state. Used to play the transition between puzzles animation
    /// </summary>
    public void Update()
    {
        if (_transition)
        {
            // If the transition is in its first part (lighting decreasing)
            if (_fadeOut)
            {
                // Decrease ambient lighting intensity if possible
                if (RenderSettings.ambientIntensity - (_INTENSITY_DECREASE_FACTOR * Time.deltaTime) > 0f)
                    RenderSettings.ambientIntensity -= _INTENSITY_DECREASE_FACTOR * Time.deltaTime;

                // Remove this puzzle from the list and if there are any left activate them
                // If there aren't then change to the victory state
                else
                {
                    // Remove this puzzle
                    RenderSettings.ambientIntensity = 0f;
                    _levels[0].GetLevelWorld().SetActive(false);
                    _levels.RemoveAt(0);

                    // If there are any left then activate the next one and play a transition
                    if (_levels.Count > 0)
                    {
                        // Increase max lighting intensity when we complete a puzzle
                        _MAX_LIGHTING += 0.1f;

                        // Activate the next puzzle
                        _levels[0].GetLevelWorld().SetActive(true);

                        // Flag that this part of the transition has finished
                        _fadeOut = false;

                        // Play the corridor transition
                        _transitionAnimator.Play("Puzzle_world_transition", -1, 0f);

                        // Reset the door animation (close the door)
                        _doorAnimator.Play("Rest", -1, 0f);

                        // Play sounds around the cavern
                        _machine.PlaySoundAtRandomPosition(_machine.windBlowingSound);
                        _machine.PlaySoundAtRandomPosition(_machine.miceSqueakingSound);
                    }

                    // If all puzzles have been completed then change to the victory state
                    else
                    {
                        _machine.ResetScheduledTasks();
                        _machine.SetState("VictoryState");
                    }
                }
            }

            // Second part of the transition. Increase lighting intensity to the new max
            else
            {
                if (RenderSettings.ambientIntensity + (_INTENSITY_INCREASE_FACTOR * Time.deltaTime) < _MAX_LIGHTING)
                    RenderSettings.ambientIntensity += _INTENSITY_INCREASE_FACTOR * Time.deltaTime;
                else
                    _transition = false;
            }
        }
    }
}


/// <summary>
/// Possible sixth game state. In this state, the victory world is presented to the players
/// </summary>
public class VictoryState : IState
{
    // Flags
    private bool _player_1;
    private bool _player_2;

    // Lights
    private const float _MAX_INTENSITY = 5;
    private const float _MIN_INTENSITY = 0;
    private Light _player1Light;
    private Light _player2Light;

    // Others
    private GameManager _machine;
    private Animator _doorAnimator;
    private AudioClip _doorOpeningSound;

    // Initialize fields
    public VictoryState(GameManager gsm, Light player1Light, Light player2Light, Animator doorAnimator, AudioClip doorOpeningSound)
    {
        _machine = gsm;
        _player1Light = player1Light;
        _player2Light = player2Light;
        _doorAnimator = doorAnimator;
        _doorOpeningSound = doorOpeningSound;
    }

    
    /// <summary>
    /// Sets up the victory state
    /// </summary>
    public void Setup()
    {
        // Make sure the correct world is active
        _machine.ChangeWorld("victoryWorld");

        // Set the play again flags
        _player_1 = false;
        _player_2 = false;

        // Set the ambient lighting intensity as normal
        RenderSettings.ambientIntensity = 1f;

        // Play the door animation
        _doorAnimator.enabled = true;
        _doorAnimator.Play("door", -1, 0f);
        _machine.PlaySoundAtPosition(_doorOpeningSound, _doorAnimator.transform.position);
    }


    /// <summary>
    /// Checks if a button that was touched is one of this state's interactable buttons
    /// </summary>
    public bool TouchButton(string button_name)
    {
        // If a button was just pressed then ignore this press
        if (_machine.CooldownActive)
            return false;

        // If this press counted then start a cooldown
        _machine.StartCooldown();

        // If the button pressed was one of the play again buttons
        // then toggle the flags and the feedback light intensity
        switch (button_name)
        {
            case "PlayAgain_player_1":
                _player_1 = !_player_1;
                if (_player_1)
                    _player1Light.intensity = _MAX_INTENSITY;
                else
                    _player1Light.intensity = _MIN_INTENSITY;
                break;
            case "PlayAgain_player_2":
                _player_2 = !_player_2;
                if (_player_2)
                    _player2Light.intensity = _MAX_INTENSITY;
                else
                    _player2Light.intensity = _MIN_INTENSITY;
                break;
            default: return false;
        }
        return true;
    }


    /// <summary>
    /// Updates this state. If both play again buttons have been pressed and are active
    /// then restart the game by changing to the splash screen animation state
    /// </summary>
    public void Update()
    {
        if (_player_1 && _player_2)
        {
            // Reset flags, light intensities and scheduled tasks (cooldown, timer)
            _player_1 = false;
            _player_2 = false;
            _player1Light.intensity = _MIN_INTENSITY;
            _player2Light.intensity = _MIN_INTENSITY;
            _machine.ResetScheduledTasks();

            // Change state
            _machine.SetState("SplashScreenAnimation");
        }
    }
}


/// <summary>
/// Possible sixth game state. In this state, the defeat world is presented to the players
/// </summary>
public class DefeatState : IState
{
    // Flags
    private bool _player_1;
    private bool _player_2;

    // Lights
    private const float _MAX_INTENSITY = 5;
    private const float _MIN_INTENSITY = 0;
    private Light _player1Light;
    private Light _player2Light;

    // Others
    private GameManager _machine;
    private AudioClip _gameOverSound;

    // Initialize fields
    public DefeatState(GameManager gsm, Light player1Light, Light player2Light, AudioClip gameOverSound)
    {
        _machine = gsm;
        _player_1 = false;
        _player_2 = false;
        _player1Light = player1Light;
        _player2Light = player2Light;
        _gameOverSound = gameOverSound;
    }


    /// <summary>
    /// Sets up the defeat state
    /// </summary>
    public void Setup()
    {
        // Make sure the correct world is active
        _machine.ChangeWorld("defeatWorld");

        // Set the correct ambient lighting intensity
        RenderSettings.ambientIntensity = 0.7f;

        // Play the game over sound
        _machine.PlaySound(_gameOverSound);
    }


    /// <summary>
    /// Checks if a button that was touched is one of this state's interactable buttons
    /// </summary>
    public bool TouchButton(string button_name)
    {
        // If a button was just pressed then ignore this press
        if (_machine.CooldownActive)
            return false;

        // If this press counted then start a cooldown
        _machine.StartCooldown();

        // If the button pressed was one of the play again buttons
        // then toggle the flags and the feedback light intensity
        switch (button_name)
        {
            case "PlayAgain_player_1":
                _player_1 = !_player_1;
                if (_player_1)
                    _player1Light.intensity = _MAX_INTENSITY;
                else
                    _player1Light.intensity = _MIN_INTENSITY;
                break;
            case "PlayAgain_player_2":
                _player_2 = !_player_2;
                if (_player_2)
                    _player2Light.intensity = _MAX_INTENSITY;
                else
                    _player2Light.intensity = _MIN_INTENSITY;
                break;
            default: return false;
        }
        return true;
    }


    /// <summary>
    /// Updates this state. If both play again buttons have been pressed and are active
    /// then restart the game by changing to the splash screen animation state
    /// </summary>
    public void Update()
    {
        if (_player_1 && _player_2)
        {
            // Reset flags, light intensities and scheduled tasks (cooldown, timer)
            _player_1 = false;
            _player_2 = false;
            _player1Light.intensity = _MIN_INTENSITY;
            _player2Light.intensity = _MIN_INTENSITY;
            _machine.ResetScheduledTasks();

            // Change state
            _machine.SetState("SplashScreenAnimation");
        }
    }
}


/// <summary>
/// Stores all information needed for a puzzle to be played
/// </summary>
public class Level
{
    // Variables
    private string _code; // This puzzle's code
    private GameObject _levelWorld; // This puzzle's level world

    // Initialize fields
    public Level(GameObject levelWorld)
    {
        _levelWorld = levelWorld;

        // Assign the correct code to this puzzle
        switch(_levelWorld.name)
        {
            case "Level_1": _code = "253"; break;
            case "Level_2": _code = "943"; break;
            case "Level_3": _code = "163"; break;
            default: break;
        }
    }


    /// <summary>
    /// Returns this level's game object
    /// </summary>
    public GameObject GetLevelWorld()
    {
        return _levelWorld;
    }


    /// <summary>
    /// Checks if a given code is correct
    /// </summary>
    public bool CheckCode(string aCode)
    {
        return _code == aCode;
    }
}
