using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class GameBoard : MonoBehaviour {
	[SerializeField]
	Material greenGround = default;

	[SerializeField]
	Material darkGreenGround = default;

	[SerializeField]
	GameTile tilePrefab = default;

	[SerializeField]
	Texture2D gridTexture = default;

	Vector2Int size;

	GameTile[] tiles;

	List<GameTile> spawnPoints = new List<GameTile>();

	List<GameTile> checkpoints = new List<GameTile>();

	List<GameTile> groundPath = new List<GameTile>();

	List<GameTile> flyingPath = new List<GameTile>();

	List<Queue<GameTile>> searchFrontiers = new List<Queue<GameTile>>();

	List<GameTileContent> updatingContent = new List<GameTileContent>();

	GameTileContentFactory contentFactory;

	bool showMaze;

	bool showPath = false;
	
	public int CurrentMaze { get; set; }

	public int[,,] mazes = {
		{
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0},
			{0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 2, 1, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0},
			{0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0},
			{0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0},
			{0, 1, 2, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 2, 0, 0},
			{1, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0}
		}, {
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
			{0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0},
			{0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0},
			{1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 2, 1, 0},
			{0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 0, 0},
			{0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
		},  {
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 2, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
			{0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1},
			{0, 1, 2, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 2, 0, 0},
			{0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 0},
			{0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0},
			{0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0},
			{0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 1, 0, 2, 0, 1, 0, 0, 0, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
		}, {
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
		}
	};
	
	public bool ShowMaze {
		get => showMaze;
		set {
			showMaze = value;
			foreach (GameTile t in tiles) {
				t.Arrow.gameObject.SetActive(false);
			}
			if (showMaze) {
				
				for (int x = 0; x < mazes.GetLength(1); x++) {
					for (int y = 0; y < mazes.GetLength(2); y++) {
						if (mazes[CurrentMaze, x, y] == 1) {
							GetTile(size.x - 1 - x, y).Arrow.gameObject.SetActive(true);
						}
					}
				}
			}
		}
	}

	public bool ShowPath {
		get => showPath;
		set {
			showPath = value;
			foreach (GameTile t in tiles) {
				t.Dehover();
			}

			if (showPath) {
				for (int i = 1; i < groundPath.Count - 1; i++) {
					groundPath[i].ShowPath();
				}
			}
		}
	}

	public int SpawnPointCount => spawnPoints.Count;
	public List<GameTile> GroundPath => groundPath;
	public List<GameTile> FlyingPath => flyingPath;

	public void Initialize(Vector2Int size, GameTileContentFactory contentFactory) {
		this.size = size;
		this.contentFactory = contentFactory;
		var random = new Random();
		// ground.localScale = new Vector3(size.x, size.y, 1f);

		Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
		tiles = new GameTile[size.x * size.y];
		for (int i = 0, y = 0; y < size.y; y++) {
			for (int x = 0; x < size.x; x++, i++) {
				GameTile tile = tiles[i] = Instantiate(tilePrefab);
				tile.Initialize(transform, i % 2 == 0 ? greenGround : darkGreenGround,
					new Vector3(x - offset.x, 0f, y - offset.y), 90 * random.Next(0, 4), 180 * random.Next(0, 2),
					random.Next(0, 2));
				if (x > 0) {
					GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
				}

				if (y > 0) {
					GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
				}

				if (y > 0 && x < size.x - 1) GameTile.MakeDiagonalNeighbors2(tile, tiles[i - size.x + 1]);
				if (y > 0 && x > 0) GameTile.MakeDiagonalNeighbors1(tile, tiles[i - size.x - 1]);
				tile.IsAlternative = (x & 1) == 0;
				if ((y & 1) == 0) {
					tile.IsAlternative = !tile.IsAlternative;
				}

				tile.Content = contentFactory.Get(GameTileContentType.Empty);
			}
		}

		prepareCheckPoints();
		flyingPath.Add(spawnPoints[0]);
		flyingPath.AddRange(checkpoints);
	}

	void prepareCheckPoints() {
		ToggleSpawnPoint(tiles[size.x * (size.y - 3) + 2]); //Respawn (2,14)
		ToggleDestination(tiles[size.x * (size.y / 2) + 2]); //(2,8) 
		ToggleDestination(tiles[size.x * (size.y / 2) + size.y - 3]); //(14,8)
		ToggleDestination(tiles[size.x * (size.y - 3) + size.y - 3]); //(14,14)
		ToggleDestination(tiles[size.x * (size.y - 3) + (size.y / 2)]); //(8,14)
		ToggleDestination(tiles[size.x * 2 + (size.y / 2)]); //(8,2)
		ToggleDestination(tiles[size.x * 2 + size.y - 3]); //(14,2)
		FindPaths();
	}

	public void Clear() {
		foreach (GameTile tile in tiles) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
		}

		spawnPoints.Clear();
		checkpoints.Clear();
		updatingContent.Clear();
		prepareCheckPoints();
	}

	public void GameUpdate() {
		/*
		 * Предполагаем, что кроме башен там ничего нет
		 */
		updatingContent.ForEach(content => {
			if (typeof(Tower).IsInstanceOfType(content)) {
				((Tower) content).StatusEffects.Clear();
			}
		});
		updatingContent.ForEach(content => {
			if (typeof(Tower).IsInstanceOfType(content)) {
				Tower t = (Tower) content;
				t.Auras.ForEach(aura => aura.Modify(t));
			}
		});
		for (int i = 0; i < updatingContent.Count; i++) {
			updatingContent[i].GameUpdate();
		}
	}

	public void ToggleDestination(GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Flag) {
			if (checkpoints.Count > 1) {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				checkpoints.Remove(tile);
				// FindPaths();
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			if (checkpoints.Count == 5) {
				tile.Content = contentFactory.Get(GameTileContentType.Destination);
				tile.Material = tile.Content.transform.GetComponent<MeshRenderer>().material;
			}
			else {
				tile.Content = contentFactory.Get(GameTileContentType.Flag);
				tile.checkpointIndex = checkpoints.Count;
			}

			checkpoints.Add(tile);
			// FindPaths();
		}
	}

	public void ToggleWall(GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Wall) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			FindPaths();
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(GameTileContentType.Wall);
			if (!FindPaths()) {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
		}
		else if (tile.Content.Type == GameTileContentType.Tower) {
			updatingContent.Remove(tile.Content);
			tile.Content = contentFactory.Get(GameTileContentType.Wall);
		}
	}

	public void RemoveWall(GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Wall) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			FindPaths();
		}
	}

	public bool ToggleTower(GameTile tile, TowerType towerType) {
		if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(towerType);
			if (FindPaths()) {
				updatingContent.Add(tile.Content);
			}
			else {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
				return false;
			}

			return true;
		}

		if (tile.Content.Type == GameTileContentType.Wall) {
			tile.Content = contentFactory.Get(towerType);
			updatingContent.Add(tile.Content);
			return true;
		}

		if (tile.Content.Type == GameTileContentType.Tower) {
			updatingContent.Remove(tile.Content);
			tile.Content = contentFactory.Get(towerType);
			updatingContent.Add(tile.Content);
			return true;
		}

		return false;
	}

	public void ToggleSpawnPoint(GameTile tile) {
		if (tile.Content.Type == GameTileContentType.SpawnPoint) {
			if (spawnPoints.Count > 1) {
				spawnPoints.Remove(tile);
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
			tile.Material = tile.Content.transform.GetComponent<MeshRenderer>().material;
			spawnPoints.Add(tile);
		}
	}

	public void ToggleGift(GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Gift) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			FlyingPath.RemoveAt(1);
			checkpoints.RemoveAt(0);
			FindPaths();
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(GameTileContentType.Gift);
			FlyingPath.Insert(1, tile);
			checkpoints.Insert(0, tile);
			if (!FindPaths()) {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FlyingPath.RemoveAt(1);
				checkpoints.RemoveAt(0);
				FindPaths();
			}
		}
	}

	public GameTile GetSpawnPoint(int index) {
		return spawnPoints[index];
	}

	public GameTile GetTile(Ray ray) {
		if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1)) {
			int x = (int) (hit.point.x + size.x * 0.5f);
			int y = (int) (hit.point.z + size.y * 0.5f);
			if (x >= 0 && x < size.x && y >= 0 && y < size.y) {
				return tiles[x + y * size.x];
			}
		}

		return null;
	}

	public GameTile GetTile(Vector3 position) {
		int row = (int) (position.z + size.y * 0.5f);
		int column = (int) (position.x + size.x * 0.5f);
		if (column >= 0 && column < size.x && column >= 0 && column < size.y) {
			return tiles[row * size.x + column];
		}

		return null;
	}

	public GameTile GetTile(int row, int column) {
		if (row >= 0 && row < size.x && column >= 0 && column < size.y) {
			return tiles[row * size.x + column];
		}

		return null;
	}

	public void saveMaze(Transform mazePanel) {
		string path = "maze.txt";
		StreamWriter writer = new StreamWriter(path, false);
		for (int i = 0; i < tiles.Length; i++) {
			GameTile t = tiles[i];
			int x = i / size.x;
			int y = i - size.x * x;
			int newX = size.x - x - 1;
			if (t.Content.Type == GameTileContentType.Wall || t.Content.Type == GameTileContentType.Tower) {
				mazes[3, newX, y] = 1;
				writer.Write(1);
			} else if (t.Content.Type == GameTileContentType.Flag || t.Content.Type == GameTileContentType.Destination ||
			          t.Content.Type == GameTileContentType.SpawnPoint) {
				mazes[3, newX, y] = 2;
				writer.Write(2);
			} else {
				mazes[3, newX, y] = 0;
				writer.Write(0);
			}
		}
		writer.Close();
	}

	bool FindPaths() {
		groundPath = new List<GameTile>();
		for (int i = 0; i < checkpoints.Count; i++) {
			foreach (GameTile tile in tiles) {
				tile.ClearPath();
			}

			GameTile checkpoint = checkpoints[i];
			checkpoint.BecomeDestination(i);
			Queue<GameTile> searchFrontier = new Queue<GameTile>();
			searchFrontier.Enqueue(checkpoint);
			while (searchFrontier.Count > 0) {
				GameTile tile = searchFrontier.Dequeue();
				if (i == 0 && spawnPoints[0] == tile || i != 0 && checkpoints[i - 1] == tile)
					break;
				if (tile != null) {
					searchFrontier.Enqueue(tile.GrowPathNorth());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathWest());
					searchFrontier.Enqueue(tile.GrowPathNorthEast());
					searchFrontier.Enqueue(tile.GrowPathSouthEast());
					searchFrontier.Enqueue(tile.GrowPathSouthWest());
					searchFrontier.Enqueue(tile.GrowPathNorthWest());
				}
			}

			if ((i == 0 && !spawnPoints[0].HasPath()) || (i != 0 && !checkpoints[i - 1].HasPath())) {
				return false;
			}

			GameTile t = i == 0 ? spawnPoints[0] : checkpoints[i - 1];
			while (!t.IsDestination()) {
				groundPath.Add(t);
				t = t.NextTileOnPath();
			}
		}

		groundPath.Add(checkpoints[checkpoints.Count - 1]);
		ShowPath = showPath;
		return true;
	}

	private void OnDrawGizmos() {
		// if (GizmoExtensions.showPath) {
		// 	for (int i = 0; i <= 5; i++) {
		// 		GameTile tile = i == 0 ? spawnPoints[0] : checkpoints[i - 1];
		// 		Handles.color = new Color(1, 1, 1, 0.075f);
		// 		Handles.DrawSolidDisc(tile.transform.position, transform.up, (float) 0.0375f);
		// 		while (!tile.IsDestination()) {
		// 			if (tile.PathDirection.GetDirectionChangeTo(tile.NextTileOnPath().PathDirection) != DirectionChange.None)
		// 				Handles.DrawSolidDisc(tile.NextTileOnPath().transform.position, transform.up, (float) 0.0375f);
		// 			Handles.DrawDottedLine(tile.transform.position, tile.NextTileOnPath().transform.position, 4f);
		// 			tile = tile.NextTileOnPath();
		// 		}
		// 	}
		// }
	}
}