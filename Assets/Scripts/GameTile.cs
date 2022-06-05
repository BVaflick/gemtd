using System;
using UnityEngine;
using Random = System.Random;

public class GameTile : MonoBehaviour {
    [SerializeField]
    Transform arrow = default;
    
    Transform grass = default;
    
    [SerializeField]
    Transform[] grassPrefabs = default;
    
    [SerializeField]
    Material hoverMaterial = default;
    
    [SerializeField]
    Material pathMaterial = default;


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

    public int checkpointIndex = -1;

    int distance;

    GameTileContent content;

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

    public void setTower(GameTileContent tower) {
        content = tower;
        content.transform.localPosition = transform.localPosition;
    }
    public bool IsAlternative { get; set; }

    public bool HasPath() => distance != int.MaxValue;

    public bool IsDestination() => distance == 0;

    public GameTile NextTileOnPath() => nextOnPath;

    public void Initialize(Transform parent, Material material, Vector3 position, int angleY, int angleX, int prefabIndex) {
        transform.SetParent(parent, false);
        grass = grassPrefabs[prefabIndex];
        grass.gameObject.SetActive(true);
        Vector3 rot = grass.transform.eulerAngles;
        rot.y = angleY;
        rot.x = angleX;
        if (angleX == 180) {
            Vector3 grassPosition = grass.transform.position;
            grassPosition.y = -1.1f;
            grass.transform.position = grassPosition;
        }
        grass.transform.eulerAngles = rot;
        Material = material;
        transform.localPosition = position;
    }

    public void BecomeDestination(int num) {
        distance = 0;
        nextOnPath = null;
    }

    public void ClearPath() {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    public GameTile GrowPathNorth() => GrowPathTo(north);
    public GameTile GrowPathNorthEast() => GrowPathTo(northEast);

    public GameTile GrowPathEast() => GrowPathTo(east);
    public GameTile GrowPathSouthEast() => GrowPathTo(southEast);

    public GameTile GrowPathSouth() => GrowPathTo(south);
    public GameTile GrowPathSouthWest() => GrowPathTo(southWest);

    public GameTile GrowPathWest() => GrowPathTo(west);
    public GameTile GrowPathNorthWest() => GrowPathTo(northWest);

    GameTile GrowPathTo(GameTile neighbor) {
        Debug.Assert(HasPath(), "No path!");
        if (neighbor == null || 
            neighbor.HasPath() ||
            neighbor == southWest && south.Content.BlocksPath && west.Content.BlocksPath || 
            neighbor == southEast && south.Content.BlocksPath && east.Content.BlocksPath ||
            neighbor == northWest && north.Content.BlocksPath && west.Content.BlocksPath ||
            neighbor == northEast && north.Content.BlocksPath && east.Content.BlocksPath) {
            return null;
        }

        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;
        
        return neighbor.Content.BlocksPath ? null : neighbor;
    }

    public void ShowPath() {
        // arrow.gameObject.SetActive(true);
        grass.transform.GetComponent<MeshRenderer>().material = pathMaterial;
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

    public void Hover() {
        grass.transform.GetComponent<MeshRenderer>().material = hoverMaterial;
    }

    public void Dehover() {
        grass.transform.GetComponent<MeshRenderer>().material = Material;
    }
}