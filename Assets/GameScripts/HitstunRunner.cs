using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

using HitstunConstants;

public class HitstunRunner : MonoBehaviour {
    // Unity Settings
    public bool localMode = true;
    public bool showHitboxes = true;
    public CharacterName player1Character;
    public CharacterName player2Character;

    // Rendering
    public CharacterView characterView;
    CharacterView[] characterViews;
    public Camera mainCamera;

    // Character Data
    CharacterData[] characterDatas;
    
    // Internal
    float next;
    NativeArray<byte> buffer;
    bool Running { get; set; }

    void Start() {
        Vector3[] verts = new Vector3[]
        {
            new Vector3(0, 0, 2),
            new Vector3(0, Constants.BOUNDS_HEIGHT / Constants.SCALE, 2),
            new Vector3(Constants.BOUNDS_WIDTH / Constants.SCALE, 0, 2),
            new Vector3(Constants.BOUNDS_WIDTH / Constants.SCALE, Constants.BOUNDS_HEIGHT / Constants.SCALE, 2)
        };
        //Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0, 0, 0, 1));
        if (localMode) {
            // Init LocalSession
            LocalSession.Init(new GameState(), new NonGameState());
            // Init NonGameState
            for (int i=0; i<=1;i++) {
                LocalSession.ngs.players = new PlayerConnectionInfo[Constants.NUM_PLAYERS];
                LocalSession.ngs.players[i] = new PlayerConnectionInfo {
                    handle = i,
                    type = PlayerType.LOCAL,
                    controllerId = i
                };
                LocalSession.ngs.SetConnectState(i, PlayerConnectState.RUNNING);
            }
            // Init GameState
            LocalSession.gs.Init();
            // load character data from JSON
            characterDatas = new CharacterData[Constants.NUM_PLAYERS];
            string jsonPath = string.Format("Assets/Resources/CharacterData/{0}.json", player1Character.ToString());
            characterDatas[0] = JsonConvert.DeserializeObject<CharacterData>(File.ReadAllText(jsonPath));
            jsonPath = string.Format("Assets/Resources/CharacterData/{0}.json", player2Character.ToString());
            characterDatas[1] = JsonConvert.DeserializeObject<CharacterData>(File.ReadAllText(jsonPath));
            LocalSession.characterDatas = characterDatas;
            LocalSession.gs.characterDatas = characterDatas;
            // Init View
            InitView(LocalSession.gs);
        }
        Running = true;
    }

    void Update() {
        if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.Escape)) {
            Application.Quit();
        }
        if (Running) {
            var now = Time.time;
            var extraMs = Mathf.Max(0, (int)((next - now) * 1000f) - 1);
            if (localMode) {
                LocalSession.Idle(extraMs);
            }

            if (now >= next) {
                if (localMode) {
                    //long before = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    LocalSession.RunFrame();
                    if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.F1)) {
                        Debug.Log("SAVE");
                        TestSave();
                    }
                    if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.F2)) {
                        Debug.Log("LOAD");
                        TestLoad();
                    }
                }
                next = now + 1f / 60f;
            }

            if (localMode) {
                UpdateGameView(LocalSession.gs, LocalSession.ngs);
            }
        }
    }

    void UpdateGameView(GameState gs, NonGameState ngs) {
        // create characterView objects
        if (characterViews.Length != Constants.NUM_PLAYERS) {
            InitView(gs);
        }
        // update characterView objects
        for (int i = 0; i < Constants.NUM_PLAYERS; ++i) {
            characterViews[i].UpdateCharacterView(gs.characters[i], ngs.players[i]);
        }
        // update cameraPosition
        float xMean = (gs.characters[0].position.x + gs.characters[1].position.x) / 2.0f;
        float xMeanTranslated = (xMean - Constants.BOUNDS_WIDTH / 2.0f) / Constants.SCALE;
        float newCamPos = xMeanTranslated;
        if (newCamPos <Constants.CAM_LOWER_BOUND) {
            newCamPos = Constants.CAM_LOWER_BOUND;
        }
        if (newCamPos > Constants.CAM_UPPER_BOUND) {
            newCamPos = Constants.CAM_UPPER_BOUND;
        }
        mainCamera.transform.position = new Vector3(newCamPos, 1, -3);
    }

    void InitView(GameState gs) {
        characterViews = new CharacterView[Constants.NUM_PLAYERS];

        for (int i = 0; i < Constants.NUM_PLAYERS; ++i) {       
            characterViews[i] = Instantiate(characterView, transform);
            characterViews[i].LoadResources(characterDatas[i]);
            characterViews[i].showHitboxes = showHitboxes;
        }
    }

    void OnDestroy() {
        if (buffer.IsCreated) {
            buffer.Dispose();
        }
    }

    public void TestSave() {
        if (localMode) {
            if (buffer.IsCreated) {
                buffer.Dispose();
            }
            buffer = GameState.ToBytes(LocalSession.gs);
        }
    }

    public void TestLoad() {
        if (localMode) {
            GameState.FromBytes(LocalSession.gs, buffer);
        }
    }
}
