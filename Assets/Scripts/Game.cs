using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
	[SerializeField] Vector2Int boardSize = new Vector2Int(11, 11);

	public const int enemyLayerMask = 1 << 9;
	public const int towerLayerMask = 1 << 10;

	[SerializeField] GameBoard board = default;

	int playerHealth = 100;

	int level = 1;

	private int temp = 0;

	float[][] towerLevelProbability = {
		new[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
		new[] {0.8f, 0.2f, 0.0f, 0.0f, 0.0f},
		new[] {0.6f, 0.3f, 0.1f, 0.0f, 0.0f},
		new[] {0.4f, 0.3f, 0.2f, 0.1f, 0.0f},
		new[] {0.1f, 0.4f, 0.2f, 0.2f, 0.1f}
	};

	float[] towerTypeProbability = {0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f};
	//float[] towerTypeProbability = { 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	string[] towerTypeNames = {"Amethyst", "Aquamarine", "Diamond", "Emerald", "Opal", "Ruby", "Sapphire", "Topaz"};

	[SerializeField] GameTileContentFactory tileContentFactory = default;

	[SerializeField] WarFactory warFactory = default;

	[SerializeField] GameScenario scenario = default;

	GameScenario.State activeScenario;

	int availableBuilds = 5;

	bool isBuildPhase = true;

	List<GameTile> newTowers = new List<GameTile>();

	static Game instance;

	GameBehaviorCollection enemies = new GameBehaviorCollection();

	GameBehaviorCollection nonEnemies = new GameBehaviorCollection();

	Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

	Vector3 PressRay() {
		Vector3 mouse = Input.mousePosition;
		mouse.z = 15f;
		return Camera.main.ScreenToWorldPoint(mouse);
	}

	bool scenarioIsInProgress = false;

	const float pausedTimeScale = 0f;

	[SerializeField, Range(1f, 10f)] float playSpeed = 1f;

	void Awake() {
		board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
	}

	void OnEnable() {
		instance = this;
	}

	void OnValidate() {
		if (boardSize.x < 2) {
			boardSize.x = 2;
		}

		if (boardSize.y < 2) {
			boardSize.y = 2;
		}
	}

	void Update() {
		handleInput();
		if (playerHealth <= 0) {
			Debug.Log("Defeat!");
			BeginNewGame();
		}

		if (scenarioIsInProgress && !activeScenario.Progress() && enemies.IsEmpty) {
			Debug.Log("Victory!");
			BeginNewGame();
		}
		else if (scenarioIsInProgress && !activeScenario.WaveIsInProgress() && enemies.IsEmpty) {
			if (!isBuildPhase) {
				isBuildPhase = true;
				availableBuilds = 5;
			}
		}

		nonEnemies.GameUpdate();
		enemies.GameUpdate();
		Physics.SyncTransforms();
		board.GameUpdate();
	}

	void handleInput() {
		if (Input.GetKeyDown(KeyCode.Q)) {
			BuildTower();
		}
		else if (Input.GetKeyDown(KeyCode.W)) {
			RemoveWall();
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				CombineSame(false);
			} else if (Input.GetKey(KeyCode.LeftControl)) {
				CombineOneshot();
			} else {
				CombineSame(true);
			}
			
		}
		if (Input.GetMouseButtonDown(0)) {
			// HandleTouch();
		}

		if (Input.GetMouseButtonDown(1)) {
			HandleAlternativeTouch();
		}

		if (Input.GetKeyDown(KeyCode.P)) {
			if (board.ShowPath == 9) {
				board.ShowPath = 0;
			}
			else if (board.ShowPath == 5) {
				board.ShowPath = 9;
			}
			else {
				board.ShowPath++;
			}
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
		}
		else if (Time.timeScale > pausedTimeScale) {
			Time.timeScale = playSpeed;
		}

		if (Input.GetKeyDown(KeyCode.G)) {
			board.ShowGrid = !board.ShowGrid;
		}

		if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
			level++;
		}

		if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
			level--;
		}

		if (Input.GetKeyDown(KeyCode.N)) {
			BeginNewGame();
		}
	}

	void BeginNewGame() {
		newTowers.Clear();
		availableBuilds = 5;
		scenarioIsInProgress = false;
		playerHealth = 100;
		enemies.Clear();
		nonEnemies.Clear();
		board.Clear();
	}

	public static void EnemyReachedDestination(int damage) {
		instance.playerHealth -= damage;
	}

	void HandleAlternativeTouch() {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				board.ToggleDestination(tile);
			}
			else {
				board.ToggleSpawnPoint(tile);
			}
		}
	}

	void HandleTouch() {
		// GameTile tile = board.GetTile(TouchRay);
		// if (tile != null) {
		// 	GameObject obj = tile.Content.gameObject;
		// 	Selection.activeGameObject = obj;
		// 	print("1" + obj);
		// } else {
		if (Physics.Raycast(TouchRay, out RaycastHit hit)) {
			GameObject obj = hit.transform.gameObject;
			Selection.activeGameObject = obj;
			// }
		}
	}

	void OnDrawGizmos() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.white;
		Vector3 position = new Vector3(-15f, 0f, 8f);
		Handles.Label(position, "Wave: " + (1 + activeScenario.CurrentWave()), style);
		position.z -= 0.3f;
		Handles.Label(position, "Enemies: " + enemies.Count, style);
		position.z -= 0.3f;
		Handles.Label(position, "HP: " + playerHealth, style);
		position.z -= 0.3f;
		Handles.Label(position, "Level: " + level, style);
		position.z -= 0.3f;
		Handles.Label(position, "Build phase: " + isBuildPhase, style);
		position.z -= 0.3f;
		Handles.Label(position, "Builds: " + availableBuilds, style);
		position.z -= 0.3f;
		Handles.Label(position,
			"Towers built: " + string.Join(" ", newTowers.Select(item => $"{item.Content.name.Split('(')[0]}")), style);
		position.z -= 0.3f;
		Handles.Label(position, "Wave is in progress: " + activeScenario.WaveIsInProgress(), style);
		position.z -= 0.3f;
		Handles.Label(position, "Scenario is in progress: " + scenarioIsInProgress, style);
	}

	void RemoveWall() {
		GameTile tile = board.GetTile(PressRay());
		// bool isTower = newTowers.Find(t => t == tile);
		// if (tile != null && !isTower && availableBuilds > 0) {
		if (tile != null && availableBuilds > 0) {
			board.RemoveWall(tile);
		}
	}

	void BuildTower() {
		GameTile tile = board.GetTile(PressRay());
		bool isTower = newTowers.Find(t => t == tile);
		if (tile != null && !isTower && availableBuilds > 0) {
			TowerType type = prepareTowerType();
			bool built = board.ToggleTower(tile, type);
			// bool built = board.ToggleTower(tile, TowerType.Silver);
			// bool built;
			// int t = Random.Range(0, 3);
			// int t = temp++;
			// if (t == 0) {
			// 	built = board.ToggleTower(tile, TowerType.Ruby1);
			// } else if (t == 1) {
			// 	built = board.ToggleTower(tile, TowerType.Ruby2);
			// } else if (t == 2) {
			// 	built = board.ToggleTower(tile, TowerType.Ruby3);
			// } else if (t == 3) {
			// 	built = board.ToggleTower(tile, TowerType.Ruby4);
			// } else {
			// 	built = board.ToggleTower(tile, TowerType.Ruby5);
			// }
			// if (temp == 5) temp = 0;
			if (built) {
				newTowers.Add(tile);
				availableBuilds--;
			}
		}
		else if (isTower && availableBuilds == 0) {
			chooseTower(tile);
		}
	}

	TowerType prepareTowerType() {
		double rand = Random.value;
		string result = "";
		float sum = 0;
		for (int i = 0; i < towerTypeProbability.Length; i++) {
			sum += towerTypeProbability[i];
			if (sum > rand) {
				result += towerTypeNames[i];
				break;
			}
		}
		rand = Random.value;
		for (int i = 0; i < towerLevelProbability[level - 1].Length; i++) {
			sum += towerLevelProbability[level - 1][i];
			if (sum > Math.Round(rand, 3)) {
				result += i+1;
				break;
			}
		}

		return (TowerType) Enum.Parse(typeof(TowerType), result);
	}

	void chooseTower(GameTile tile) {
		newTowers.Remove(tile);
		if (!scenarioIsInProgress) {
			activeScenario = scenario.Begin();
			scenarioIsInProgress = true;
		}
		else {
			activeScenario.NextWave();
		}

		isBuildPhase = false;
		// for(var i = 0; i < TargetPoint.BufferedCount; i++) {
		// 	TargetPoint t = TargetPoint.GetBuffered(i);				
		// }
		foreach (GameTile t in newTowers) {
			board.ToggleWall(t);
		}

		newTowers.Clear();
	}

	void CombineOneshot() {
		GameTile tile = board.GetTile(PressRay());
		Tower tower = (Tower) newTowers.Find(t => t == tile).Content;
		if (tower && availableBuilds == 0) {
			int type = (int) findCombos(tower, tileContentFactory.TowerPrefabs.ToList(), newTowers.Select(x => (Tower) x.Content).ToList(), false)[0];
			board.ToggleTower(tile, (TowerType) type);
			chooseTower(tile);
		}
	}	
	
	static List<TowerType> findCombos(Tower tower, List<Tower> availableTowers, List<Tower> towers, bool oneshot) {
		List<TowerType> combosTypes = availableTowers.FindAll(t => t.Combo != null &&
		                                                           t.Combo.Contains(tower.TowerType) &&
		                                                           t.Combo.All(value =>
			                                                           towers.Select(x => x.TowerType).Contains(value)))
			.Select(x => x.TowerType).ToList();
		// if (oneshot) {
		// 	combosTypes.AddRange(availableTowers.FindAll(t => t.oneshotCombo != null &&
		// 	                                                  t.oneshotCombo.Contains(tower.type) &&
		// 	                                                  t.oneshotCombo.All(value =>
		// 		                                                  towers.Select(x => x.type).Contains(value)))
		// 		.Select(x => x.type).ToList());
		// }

		return combosTypes;
	}
	
	void CombineSame(bool two) {
		GameTile tile = board.GetTile(PressRay());
		Tower tower = (Tower) newTowers.Find(t => t == tile).Content;
		if (tower && 
		    availableBuilds == 0 &&
		    char.IsDigit(tower.TowerType.ToString().Last()) &&
		    newTowers.FindAll(towerTile => ((Tower) towerTile.Content).TowerType == tower.TowerType).Count > (two ? 1 : 3)) {
			int type = (int) tower.TowerType;
			board.ToggleTower(tile, (TowerType) type + (two ? 1 : 2));
			chooseTower(tile);
		}
	}

	public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
		GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		enemy.SpawnOn(spawnPoint);
		instance.enemies.Add(enemy);
	}

	public static Shell SpawnShell() {
		Shell shell = instance.warFactory.Shell;
		instance.nonEnemies.Add(shell);
		return shell;
	}

	public static Explosion SpawnExplosion(bool flag) {
		Explosion explosion = instance.warFactory.Explosion;
		if(flag) instance.nonEnemies.Add(explosion);
		return explosion;
	}

	public static Corrosion SpawnCorrosion() {
		Corrosion corrosion = instance.warFactory.Corrosion;
		// instance.nonEnemies.Add(explosion);
		return corrosion;
	}

	public static Ice SpawnIce() {
		Ice ice = instance.warFactory.Ice;
		// instance.nonEnemies.Add(slow);
		return ice;
	}

	public static Toxin SpawnToxin() {
		Toxin toxin = instance.warFactory.Toxin;
		// instance.nonEnemies.Add(poison);
		return toxin;
	}
}

/*
 * TODO:
 * 1. Поиск пути по диагонали											-
 * 2. Комбинирование башен не только в режиме постройки					-
 * 3. Привести атрибуты башен, юнитов и способностей в соответствие		-
 * 3а. Адаптировать фабрику для удобной настройки волн: 				
 *     (жизни фиксированные для волны, но машстаб зависит от урона)		-
 * 4. Добавить механики:			
 * 		а. Иммунитет к магии											-
 * 		б. Увороты														-
 * 		в. Отскок снарядов для цепной молнии и чайника					-
 * 		г. ПВО															-
 * 		д. Невидимость													-
 * 5. Добавить способности для башен:		
 * 		а. Точность														-
 * 		б. Дальность													-
 * 		в. Замедление по области (Желтый сапфир)						-
 * 		г. Иммунитет к магии для башен									-
 * 6. Летающие юниты													-
 * 7. Выделение объектов												-
 * 8. Прогресс волн														-
 * 9. Система опыта														-
 * 10. Убрать возможность строить поверх существующей башни				-
 * 11. Педали															-
 * 12. Визуализация нанесенного урона									-
 * 13. Шкала здоровья													-
 * 14. MVP																-
 * 15. GUI																-
 */