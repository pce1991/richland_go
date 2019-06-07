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

    List<GameObject> whitePieces;
    List<GameObject> blackPieces;
    
    List<GameObject> whitePlayerObjs;
    List<GameObject> blackPlayerObjs;

    
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
        PieceColor color;
        Vector2 pos;
    };

    // @NOTE: these might diverge at some point so we'll define them separately. Not sure if this is good or not
    struct Player {
        PieceColor color;
        Vector2 pos;
    }

    List<Player> whitePlayers;
    List<Player> blackPlayers;

    enum GamePhase {
        Setup,
        Main,
        End,
    }

    GamePhase phase;

    GameObject CreatePiece(int x, int y, bool black) {
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

        o.transform.position = new Vector3(x, y, 0);

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

        o.transform.position = new Vector3(x, y, 0);

        o.transform.localScale = new Vector3(1, 1.0f, 0.25f);

        return o;
    }

    // Use this for initialization
    void Start () {
        whitePieces = new List<GameObject>();
        blackPieces = new List<GameObject>();

        whitePlayerObjs = new List<GameObject>();
        blackPlayerObjs = new List<GameObject>();

        {
            blackPlayerObjs.Add(CreatePlayer(width / 2, 0, true));

            whitePlayerObjs.Add(CreatePlayer(width / 2, height - 1, false));
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
                
                    GameObject p = CreatePiece(x, y, black);
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
                
                    GameObject p = CreatePiece(x, y, black);
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

            
                start.x = 0 - 0.5f;
                start.y = y - 0.5f;
                
                end.x = width - 0.5f;
                end.y = y - 0.5f;

                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        
            for (int x = 0; x <= width; x++) {
                GameObject o = new GameObject();

                LineRenderer line = o.AddComponent<LineRenderer>();

                line.material = whitePieceMaterial;

                line.startWidth = 0.1f;
                line.endWidth = 0.1f;

                start.x = x - 0.5f;
                start.y = 0 - 0.5f;
                
                end.x = x - 0.5f;
                end.y = height - 0.5f;

                line.SetPosition(0, start);
                line.SetPosition(1, end);
            }
        }
    }

    void Update () {

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
