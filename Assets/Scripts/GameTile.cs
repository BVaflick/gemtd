using System;
using UnityEngine;

public class GameTile : MonoBehaviour {
    [SerializeField]
    Transform arrow = default;
    
    [SerializeField]
    Transform grass = default;

    [SerializeField]
    public Material[] Materials;

    [SerializeField]
    Material hoverMaterial = default;

    private Material material = default;

    public Material Material {
        get => material; 
        set {
            material = value;
            grass.transform.GetComponent<MeshRenderer>().material = value;
        } 
    }

    private GameTile north, northEast, east, southEast, south, southWest, west, northWest;

    GameTile[] nextOnPath = new GameTile[6];

    int[] distances;

    GameTileContent content;

    public Vector3[] exitPoint = new Vector3[6];

    public Direction[] PathDirection = new Direction[6];


    public GameTileContent Content {
        get => content;
        set {
            Debug.Assert(value != null, "Null assigned to content!");
            if (content != null) {
                content.Recycle();
            }

            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    public bool IsAlternative { get; set; }

    public bool HasPath(int num) => distances[num] != int.MaxValue;

    public bool IsDestination(int num) => distances[num] == 0;

    public GameTile NextTileOnPath(int num) => nextOnPath[num];

    public void BecomeDestination(int num) {
        distances[num] = 0;
        nextOnPath[num] = null;
        exitPoint[num] = transform.localPosition;
    }

    public void ClearPath() {
        distances = new int[] {int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue};
        nextOnPath = new GameTile[] {null, null, null, null, null, null};
        exitPoint = new Vector3[]
            {new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3()};
        PathDirection = new Direction[]
            {new Direction(), new Direction(), new Direction(), new Direction(), new Direction(), new Direction()};
    }

    public GameTile GrowPathNorth(int num) => GrowPathTo(num, north, Direction.South);
    public GameTile GrowPathNorthEast(int num) => GrowPathTo(num, northEast, Direction.SouthWest);

    public GameTile GrowPathEast(int num) => GrowPathTo(num, east, Direction.West);
    public GameTile GrowPathSouthEast(int num) => GrowPathTo(num, southEast, Direction.NorthWest);

    public GameTile GrowPathSouth(int num) => GrowPathTo(num, south, Direction.North);
    public GameTile GrowPathSouthWest(int num) => GrowPathTo(num, southWest, Direction.NorthEast);

    public GameTile GrowPathWest(int num) => GrowPathTo(num, west, Direction.East);
    public GameTile GrowPathNorthWest(int num) => GrowPathTo(num, northWest, Direction.SouthEast);

    GameTile GrowPathTo(int num, GameTile neighbor, Direction direction) {
        Debug.Assert(HasPath(num), "No path!");
        if (neighbor == null || 
            neighbor.HasPath(num) ||
            direction == Direction.NorthEast && south.Content.BlocksPath && west.Content.BlocksPath || 
            direction == Direction.NorthWest && south.Content.BlocksPath && east.Content.BlocksPath ||
            direction == Direction.SouthEast && north.Content.BlocksPath && west.Content.BlocksPath ||
            direction == Direction.SouthWest && north.Content.BlocksPath && east.Content.BlocksPath) {
            return null;
        }

        neighbor.distances[num] = distances[num] + 1;
        neighbor.nextOnPath[num] = this;
        // neighbor.exitPoint[num] = neighbor.transform.localPosition + direction.GetHalfVector();
        neighbor.PathDirection[num] = direction;
        
        return neighbor.Content.BlocksPath ? null : neighbor;
    }

    public void HidePath() {
        arrow.gameObject.SetActive(false);
    }

    public void ShowPath(int num) {
        if (distances[num] == 0) {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath[num] == north ? northRotation :
            nextOnPath[num] == northEast ? northEastRotation :
            nextOnPath[num] == east ? eastRotation :
            nextOnPath[num] == southEast ? southEastRotation :
            nextOnPath[num] == south ? southRotation :
            nextOnPath[num] == southWest ? southWestRotation :
            nextOnPath[num] == northWest ? northWestRotation :
            westRotation;
    }

    static Quaternion
        northRotation = Quaternion.Euler(0f, 0f, 0f),
        northEastRotation = Quaternion.Euler(0f, 0f, 315f),
        eastRotation = Quaternion.Euler(0f, 0f, 270f),
        southEastRotation = Quaternion.Euler(0f, 0f, 225f),
        southRotation = Quaternion.Euler(0f, 0f, 180f),
        southWestRotation = Quaternion.Euler(0f, 0f, 135f),
        westRotation = Quaternion.Euler(0f, 0f, 90f),
        northWestRotation = Quaternion.Euler(0f, 0f, 45f);

    public static void MakeEastWestNeighbors(GameTile east, GameTile west) {
        Debug.Assert(
            west.east == null && east.west == null, "Redefined neighbors!"
        );
        west.east = east;
        east.west = west;
    }

    public static void MakeNorthSouthNeighbors(GameTile north, GameTile south) {
        Debug.Assert(
            south.north == null && north.south == null, "Redefined neighbors!"
        );
        south.north = north;
        north.south = south;
    }
    
    public static void MakeDiagonalNeighbors1(GameTile northEast, GameTile southWest) {
        Debug.Assert(
            southWest.northEast == null && northEast.southWest == null, "Redefined neighbors!"
        );
        southWest.northEast = northEast;
        northEast.southWest = southWest;
    }

    public static void MakeDiagonalNeighbors2(GameTile northWest, GameTile southEast) {
        Debug.Assert(
            southEast.northWest == null && northWest.southEast == null, "Redefined neighbors!"
        );
        southEast.northWest = northWest;
        northWest.southEast = southEast;
    }

    private void OnMouseEnter() {
        grass.transform.GetComponent<MeshRenderer>().material = hoverMaterial;
    }

    private void OnMouseExit() {
        grass.transform.GetComponent<MeshRenderer>().material = Material;
    }
}