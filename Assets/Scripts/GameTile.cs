using UnityEngine;

public class GameTile : MonoBehaviour {

	[SerializeField]
	Transform arrow = default;
	
	[SerializeField]
	public Material[] Materials;

	GameTile north, east, south, west;

	GameTile[] nextOnPath = new GameTile[6];

	int[] distances;

	GameTileContent content;

	public Vector3[] exitPoint = new Vector3[6];

	public Direction[] PathDirection = new Direction[6];


	public GameTileContent Content   
	
	{
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

	public GameTile NextTileOnPath(int num) => nextOnPath[num];

	public void BecomeDestination(int num) {
		distances[num] = 0;
		nextOnPath[num] = null;
		exitPoint[num] = transform.localPosition;
	}

	public void ClearPath() {
		distances = new int[] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue };
		nextOnPath = new GameTile[] { null, null, null, null, null, null };
		exitPoint = new Vector3[] { new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3() };
		PathDirection = new Direction[] { new Direction(), new Direction(), new Direction(), new Direction(), new Direction(), new Direction() };
	}

	public GameTile GrowPathNorth(int num) => GrowPathTo(num, north, Direction.South);

	public GameTile GrowPathEast(int num) => GrowPathTo(num, east, Direction.West);

	public GameTile GrowPathSouth(int num) => GrowPathTo(num, south, Direction.North);

	public GameTile GrowPathWest(int num) => GrowPathTo(num, west, Direction.East);

	GameTile GrowPathTo(int num, GameTile neighbor, Direction direction) {
		Debug.Assert(HasPath(num), "No path!");
		if (neighbor == null || neighbor.HasPath(num)) {
			return null;
		}
		neighbor.distances[num] = distances[num] + 1;
		neighbor.nextOnPath[num] = this;
		neighbor.exitPoint[num] = neighbor.transform.localPosition + direction.GetHalfVector();
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
			nextOnPath[num] == east ? eastRotation :
			nextOnPath[num] == south ? southRotation :
			westRotation;
	}

	static Quaternion
	northRotation = Quaternion.Euler(90f, 0f, 0f),
		eastRotation = Quaternion.Euler(90f, 90f, 0f),
		southRotation = Quaternion.Euler(90f, 180f, 0f),
		westRotation = Quaternion.Euler(90f, 270f, 0f);

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
}