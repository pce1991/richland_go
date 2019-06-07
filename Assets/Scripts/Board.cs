using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    public int width;
    public int height;

    public Mesh pieceMesh;
    public Mesh playerMesh;

    public float orthographicSizeSlop;
    public GameObject cameraObj;

    public Material whitePieceMaterial;
    public Material blackPieceMaterial;

    List<GameObject> whitePieces = new List<GameObject>();
    List<GameObject> blackPieces = new List<GameObject>();
    
    List<GameObject> whitePlayerObjs = new List<GameObject>();
    List<GameObject> blackPlayerObjs = new List<GameObject>();

    List<Player> whitePlayers = new List<Player>();
    List<Player> blackPlayers = new List<Player>();

    List<MoveTransaction> moveTransactions = new List<MoveTransaction>();

    
    List<GameObject> horzLines;
    List<GameObject> vertLines;

    enum PieceColor {
        Black,
        White
    }

    // @NOTE: we dont need a whole bulky GameObject when doing calculations for our game.
    //        When we move something we dont need the renderer, the mesh, the 3D transform even
    //        If we are iterating over the game object then we are brining a lot of unecessary data
    //        into the cache.
    struct Piece {
        public PieceColor color;
        public Vector2 pos;
    };

    // @NOTE: these might diverge at some point so we'll define them separately. Not sure if this is good or not
    struct Player {
        public Vector2 pos;

        // @TODO: this may not be enough...
        public int length;

        public Player(Vector2 pos_) {
            pos = pos_;
            length = 1;
        }
    }

    enum ActionType {
        None,
        Left,
        Right,
        Up,
        Down,
    }

    struct MoveTransaction {
        public PieceColor color;
        public int index;
        public GameObject obj;
        
        public Vector2 startPos;
        public Vector2 endPos;
    }

    

    enum GamePhase {
        Setup,
        Main,
        End,
    }

    GamePhase phase;

    GameObject CreatePieceObj(int x, int y, bool black) {
        GameObject o = new GameObject("piece");

        MeshFilter meshFilter = o.AddComponent<MeshFilter>();

        meshFilter.mesh = pieceMesh;

        MeshRenderer meshRenderer = o.AddComponent<MeshRenderer>();

        if (black) {
            meshRenderer.material = blackPieceMaterial;
        }
        else {
            meshRenderer.material = whitePieceMaterial;
        }

        o.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);

        o.transform.localScale = new Vector3(1, 1.0f, 0.25f);

        return o;
    }

    // @NOTE: code dupe above except for the specific mesh
    GameObject CreatePlayer(int x, int y, bool black) {
        GameObject o = new GameObject("player");

        MeshFilter meshFilter = o.AddComponent<MeshFilter>();

        meshFilter.mesh = playerMesh;

        MeshRenderer meshRenderer = o.AddComponent<MeshRenderer>();

        if (black) {
            meshRenderer.material = blackPieceMaterial;
        }
        else {
            meshRenderer.material = whitePieceMaterial;
        }

        o.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);

        o.transform.localScale = new Vector3(1, 1.0f, 0.25f);

        Player p = new Player();
        p.pos = new Vector2(x, y);
        p.length = 1;

        if (black) {
            blackPlayerObjs.Add(o);
            blackPlayers.Add(p);
        }
        else {
            whitePlayerObjs.Add(o);
            whitePlayers.Add(p);
        }

        return o;
    }

    // Use this for initialization
    void Start () {
        whitePieces = new List<GameObject>();
        blackPieces = new List<GameObject>();

        whitePlayerObjs = new List<GameObject>();
        blackPlayerObjs = new List<GameObject>();

        {
            CreatePlayer(width / 2 - 1, 0, true);

            CreatePlayer(width / 2, height - 1, false);
        }

        horzLines = new List<GameObject>();
        vertLines = new List<GameObject>();
        
        phase = GamePhase.Setup;

        {
            bool black = true;
            for (int y = 1; y < 4; y++) {
                
                for (int x = 0; x < width; x++) {

                    if (y % 2 == 0 && x % 2 == 0) {
                        continue;
                    }
                    else if (y % 2 != 0 && x % 2 != 0) {
                        continue;
                    }
                
                    GameObject p = CreatePieceObj(x, y, black);
                    blackPieces.Add(p);
                }
            }
        }

        {
            bool black = false;
            for (int y = height - 2; y > height - 5; y--) {
                
                for (int x = 0; x < width; x++) {

                    if (y % 2 == 0 && x % 2 == 0) {
                        continue;
                    }
                    else if (y % 2 != 0 && x % 2 != 0) {
                        continue;
                    }
                
                    GameObject p = CreatePieceObj(x, y, black);
                    whitePieces.Add(p);
                }
            }
        }

        // Setup camera
        {
            Camera cam = cameraObj.GetComponent<Camera>();

            // The orthographicSize is half the size of the vertical viewing volume.
            // The horizontal size of the viewing volume depends on the aspect ratio.
            if (width > height) {
                cam.orthographicSize = width / 2.0f;
            }
            else {
                cam.orthographicSize = height / 2.0f;
            }

            cam.orthographicSize += orthographicSizeSlop;
            cameraObj.transform.position = new Vector3((width * 0.5f) - 0.5f, (height * 0.5f) - 0.5f, -10);
        }

        // Create line renderers
        {
            Vector3 start = new Vector3();
            Vector3 end = new Vector3();
            for (int y = 0; y <= height; y++) {
                GameObject o = new GameObject();

                LineRenderer line = o.AddComponent<LineRenderer>();

                line.material = whitePieceMaterial;

                line.startWidth = 0.1f;
                line.endWidth = 0.1f;

            
                start.x = 0;
                start.y = y;
                
                end.x = width;
                end.y = y;

                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        
            for (int x = 0; x <= width; x++) {
                GameObject o = new GameObject();

                LineRenderer line = o.AddComponent<LineRenderer>();

                line.material = whitePieceMaterial;

                line.startWidth = 0.1f;
                line.endWidth = 0.1f;

                start.x = x;
                start.y = 0;
                
                end.x = x;
                end.y = height;

                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        }
    }

    Vector3 Pos2DTo3D(Vector2 pos) {
        return new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
    }
    
    ActionType GetPlayerAction() {
        ActionType type = ActionType.None;

        if (Input.GetKeyDown(KeyCode.W)) {
            type = ActionType.Up;
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            type = ActionType.Down;
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            type = ActionType.Left;
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            type = ActionType.Right;
        }


        return type;
    }

    Vector2 ActionTypeToDirection(ActionType action) {
        Vector2 dir = new Vector2();
        
        if (Input.GetKeyDown(KeyCode.W)) {
            dir.y = 1;
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            dir.y = -1;
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            dir.x = -1;
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            dir.x = 1;
        }

        return dir;
    }

    void CreateMoveTransactions(List<Player> players, List<GameObject> playerObjs, ActionType action) {
        Vector2 dir  = ActionTypeToDirection(action);

        Debug.Log(dir);

        for (int i = 0; i < players.Count; i++) {
            Player p = players[i];
            
            MoveTransaction move = new MoveTransaction();

            move.index = i;
            move.obj = playerObjs[i];
            move.startPos = p.pos;
            move.endPos = p.pos + dir;

            moveTransactions.Add(move);
        }
        
        // @TODO: create moves for pieces of both colors, and invalidate certain moves
        // What happens when you push against a structure that has a part that doesnt move?
        // Does nothing move, or do we allow the players to become separated? Could be interesting!
    }

    void ProcessMoves() {
        for (int i = 0; i < moveTransactions.Count; i++) {
            MoveTransaction move = moveTransactions[i];

            // @TODO: know if its a player, which color it is, and the obj
            blackPlayers[move.index] = new Player(move.endPos);

            move.obj.transform.position = Pos2DTo3D(move.endPos);
        }

        moveTransactions.Clear();
    }

    void Update () {

        ActionType action = GetPlayerAction();

        if (action != ActionType.None) {
            
            CreateMoveTransactions(blackPlayers, blackPlayerObjs, action);
            
            ProcessMoves();
        }

	switch (phase) {
            case GamePhase.Setup : {

            } break;

            case GamePhase.Main : {
                
            } break;

            case GamePhase.End : {

            } break;
        }
    }
}
