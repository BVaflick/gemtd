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

    GameTile nextOnPath = null;

    int distance;

    GameTileContent content;

    public Direction PathDirection = new Direction();


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

    public bool HasPath() => distance != int.MaxValue;

    public bool IsDestination() => distance == 0;

    public GameTile NextTileOnPath() => nextOnPath;

    public void BecomeDestination(int num) {
        distance = 0;
        nextOnPath = null;
    }

    public void ClearPath() {
        distance = int.MaxValue;
        nextOnPath = null;
        PathDirection = new Direction();
    }

    public GameTile GrowPathNorth(int num) => GrowPathTo(north, Direction.South);
    public GameTile GrowPathNorthEast(int num) => GrowPathTo(northEast, Direction.SouthWest);

    public GameTile GrowPathEast(int num) => GrowPathTo(east, Direction.West);
    public GameTile GrowPathSouthEast(int num) => GrowPathTo(southEast, Direction.NorthWest);

    public GameTile GrowPathSouth(int num) => GrowPathTo(south, Direction.North);
    public GameTile GrowPathSouthWest(int num) => GrowPathTo(southWest, Direction.NorthEast);

    public GameTile GrowPathWest(int num) => GrowPathTo(west, Direction.East);
    public GameTile GrowPathNorthWest(int num) => GrowPathTo(northWest, Direction.SouthEast);

    GameTile GrowPathTo(GameTile neighbor, Direction direction) {
        Debug.Assert(HasPath(), "No path!");
        if (neighbor == null || 
            neighbor.HasPath() ||
            direction == Direction.NorthEast && south.Content.BlocksPath && west.Content.BlocksPath || 
            direction == Direction.NorthWest && south.Content.BlocksPath && east.Content.BlocksPath ||
            direction == Direction.SouthEast && north.Content.BlocksPath && west.Content.BlocksPath ||
            direction == Direction.SouthWest && north.Content.BlocksPath && east.Content.BlocksPath) {
            return null;
        }

        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;
        neighbor.PathDirection = direction;
        
        return neighbor.Content.BlocksPath ? null : neighbor;
    }

    public void HidePath() {
        arrow.gameObject.SetActive(false);
    }

    public void ShowPath() {
        arrow.gameObject.SetActive(true);
    }

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