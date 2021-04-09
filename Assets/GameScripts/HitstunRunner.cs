using System.IO;
using Unity.Collections;
using UnityEngine;
using Newtonsoft.Json;

using HitstunConstants;

public class HitstunRunner : MonoBehaviour
{
    // Settings
    public bool showHitboxes = true;
    public bool manualStep = false;
    public CharacterName player1Character;
    public CharacterName player2Character;

    // Rendering
    public CharacterView characterView;
    CharacterView[] characterViews;
    public Camera mainCamera;

    // Character Data
    CharacterData[] characterDatas;

    // Internal
    NativeArray<byte> buffer;
    private bool running;
    private bool nextStep;

    void Start()
    {   
        // Fix the FPS
        Application.targetFrameRate = Constants.FPS;
        Time.fixedDeltaTime = 1f/(float)Constants.FPS;
        // Init LocalSession
        LocalSession.Init(new GameState(), new NonGameState());
        // Init NonGameState
        for (int i = 0; i <= 1; i++)
        {
            LocalSession.ngs.players = new PlayerConnectionInfo[Constants.NUM_PLAYERS];
            LocalSession.ngs.players[i] = new PlayerConnectionInfo
            {
                handle = i,
                type = PlayerType.LOCAL,
                controllerId = i
            };
            LocalSession.ngs.SetConnectState(i, PlayerConnectState.RUNNING);
        }
        // Init GameState
        LocalSession.gs.Init();
        // load character data from JSON
        LoadCharacterData();
        // Init View
        InitView(LocalSession.gs);
        running = true;
    }

    void FixedUpdate() {
        if (Time.deltaTime < 0.016f || Time.deltaTime > 0.017f)
        {
            Debug.Log("Unstable update tick!" + Time.deltaTime.ToString());
        }
        // handles function key debugging inputs
        HandleDevKeys();
        if (running || nextStep)
        {
            LocalSession.RunFrame();
            UpdateGameView(LocalSession.gs, LocalSession.ngs);
            nextStep = false;
        }
    }

    void InitView(GameState gs)
    {
        characterViews = new CharacterView[Constants.NUM_PLAYERS];

        for (int i = 0; i < Constants.NUM_PLAYERS; ++i)
        {
            characterViews[i] = Instantiate(characterView, transform);
            characterViews[i].LoadResources(characterDatas[i]);
            characterViews[i].showHitboxes = showHitboxes;
        }
    }

    void UpdateGameView(GameState gs, NonGameState ngs)
    {
        // update characterView objects
        for (int i = 0; i < Constants.NUM_PLAYERS; ++i)
        {
            characterViews[i].showHitboxes = showHitboxes;
            characterViews[i].UpdateCharacterView(gs.characters[i], ngs.players[i]);
        }
        // update cameraPosition
        float xMean = (gs.characters[0].position.x + gs.characters[1].position.x) / 2.0f;
        float xMeanTranslated = (xMean - Constants.BOUNDS_WIDTH / 2.0f) / Constants.SCALE;
        float newCamPos = xMeanTranslated;
        if (newCamPos < Constants.CAM_LOWER_BOUND)
        {
            newCamPos = Constants.CAM_LOWER_BOUND;
        }
        if (newCamPos > Constants.CAM_UPPER_BOUND)
        {
            newCamPos = Constants.CAM_UPPER_BOUND;
        }
        mainCamera.transform.position = new Vector3(newCamPos, 1, -3);
    }

    void HandleDevKeys()
    {
        // quit
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
        {
            Application.Quit();
        }
        // toggle hitboxes
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F1))
        {
            showHitboxes = !showHitboxes;
            if (showHitboxes)
            {
                Debug.Log("Hitboxes ON");
            }
            else
            {
                Debug.Log("Hitboxes OFF");
            }
        }
        // manual stepping
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F2))
        {
            manualStep = !manualStep;
            if (manualStep)
            {
                Debug.Log("Manual mode on: Press F3 to advance a single frame");
                running = false;
                nextStep = false;
            }
            else
            {
                Debug.Log("Manual mode off");
                running = true;
            }
        }
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F3))
        {
            Debug.Log("Manual step");
            nextStep = true;
        }
        // save and load
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F5))
        {
            Debug.Log("SAVE");
            TestSave();
        }
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F6))
        {
            Debug.Log("LOAD");
            TestLoad();
        }
    }

    void LoadCharacterData()
    {
        characterDatas = new CharacterData[Constants.NUM_PLAYERS];
        string jsonPath = string.Format("Assets/Resources/CharacterData/{0}.json", player1Character.ToString());
        characterDatas[0] = JsonConvert.DeserializeObject<CharacterData>(File.ReadAllText(jsonPath));
        jsonPath = string.Format("Assets/Resources/CharacterData/{0}.json", player2Character.ToString());
        characterDatas[1] = JsonConvert.DeserializeObject<CharacterData>(File.ReadAllText(jsonPath));
        LocalSession.characterDatas = characterDatas;
        LocalSession.gs.characterDatas = characterDatas;
    }

    void OnDestroy()
    {
        if (buffer.IsCreated)
        {
            buffer.Dispose();
        }
    }

    public void TestSave()
    {
        if (buffer.IsCreated)
        {
            buffer.Dispose();
        }
        buffer = GameState.ToBytes(LocalSession.gs);
    }

    public void TestLoad()
    {
        GameState.FromBytes(LocalSession.gs, buffer);
    }
}
