using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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
	// float[] towerTypeProbability = { 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	string[] towerTypeNames = {"Amethyst", "Aquamarine", "Diamond", "Emerald", "Opal", "Ruby", "Sapphire", "Topaz"};

	[SerializeField] GameTileContentFactory tileContentFactory = default;

	[SerializeField] WarFactory warFactory = default;

	[SerializeField] GameScenario scenario = default;

	GameScenario.State activeScenario;

	int availableBuilds = 5;

	bool isBuildPhase = true;

	private bool isBuilding = false;

	private bool giftAvailable = false;

	private GameTile giftTile;

	List<GameTile> newTowers = new List<GameTile>();
	List<GameTile> builtTowers = new List<GameTile>();
	private GameTile hoveredTile = null;

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

	[SerializeField] private Tower flyingTower = null;

	[SerializeField, Range(1f, 10f)] float playSpeed = 1f;

	void Awake() {
		board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
		flyingTower = Instantiate(flyingTower);
		flyingTower.gameObject.SetActive(false);
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
				if(giftAvailable) board.ToggleGift(giftTile);
			}
		}

		if (isBuilding && availableBuilds > 0) {
			GameTile tile = board.GetTile(TouchRay);
			if (tile != null) {
				if (hoveredTile != null && hoveredTile != tile) hoveredTile.Dehover();
				flyingTower.gameObject.SetActive(true);
				flyingTower.transform.position = tile.transform.position;
				tile.Hover();
				hoveredTile = tile;
			}
			else {
				flyingTower.gameObject.SetActive(false);
				if (hoveredTile != null) hoveredTile.Dehover();
			}
		}
		else {
			flyingTower.gameObject.SetActive(false);
			if (hoveredTile != null) hoveredTile.Dehover();
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
			// RemoveWall();
			board.ToggleWall(board.GetTile(TouchRay));
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
			HandleTouch();
		}

		if (Input.GetMouseButtonDown(1)) {
			HandleAlternativeTouch();
		}

		float a = Input.mouseScrollDelta.y;
		if (a > 0) {
			Vector3 pos = Camera.main.transform.position;
			Vector3 rot = Camera.main.transform.eulerAngles;
			if (pos.y > 5) {
				pos.y -= a;
				if (pos.y < 8) {
					rot.x -= 10f;
					pos.z -= 1.25f;
					Camera.main.transform.eulerAngles = rot;
				}
			}
			Camera.main.transform.position = pos;
			print(pos + " " + rot);
		}
		else if (a < 0) {
			Vector3 pos = Camera.main.transform.position;
			Vector3 rot = Camera.main.transform.eulerAngles;
			if (pos.y < 18) {
				pos.y -= a;
				if (pos.y <= 8) {
					rot.x += 10f;
					pos.z += 1.25f;
					Camera.main.transform.eulerAngles = rot;
				}
			}
			Camera.main.transform.position = pos;
			print(pos + " " + rot);
		}

		if (Input.GetKeyDown(KeyCode.P)) {
			board.ShowPath = !board.ShowPath;
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
		}
		else if (Time.timeScale > pausedTimeScale) {
			Time.timeScale = playSpeed;
		}

		if (Input.GetKeyDown(KeyCode.G)) {
			SpawnGift();
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
		Vector3 pos1 = Camera.main.transform.position;
		if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - 10) {
			if(pos1.z < 5) pos1.z += 20f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= 10) {
			if(pos1.z > -10) pos1.z -= 20f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= 10) {
			if(pos1.x > -5) pos1.x -= 20f * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - 10) {
			if(pos1.x < 5) pos1.x += 20f * Time.deltaTime;
		}
		Camera.main.transform.position = pos1;
	}

	void BeginNewGame() {
		newTowers.Clear();
		builtTowers.Clear();
		availableBuilds = 5;
		scenarioIsInProgress = false;
		isBuilding = false;
		isBuildPhase = true;
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
		if (isBuilding) {
			BuildTower();
		}
		else {
			GameTile tile = board.GetTile(TouchRay);
			if (tile != null) {
				GameObject obj = tile.Content.gameObject;
				Selection.activeGameObject = obj;
			}
		}
	}

	void OnDrawGizmos() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.white;
		Vector3 position = new Vector3(-15f, 0f, 8f);
		Handles.Label(position, "Wave: " + (1 + activeScenario.CurrentWave()), style);
		position.z -= 0.4f;
		Handles.Label(position, "Enemies: " + enemies.Count, style);
		position.z -= 0.4f;
		Handles.Label(position, "HP: " + playerHealth, style);
		position.z -= 0.4f;
		Handles.Label(position, "Level: " + level, style);
		position.z -= 0.4f;
		Handles.Label(position, "Build phase: " + isBuildPhase, style);
		position.z -= 0.4f;
		Handles.Label(position, "Builds: " + availableBuilds, style);
		position.z -= 0.4f;
		Handles.Label(position,
			"Towers built: " + string.Join(" ", newTowers.Select(item => $"{item.Content.name.Split('(')[0]}")), style);
		position.z -= 0.4f;
		Handles.Label(position, "Wave is in progress: " + activeScenario.WaveIsInProgress(), style);
		position.z -= 0.4f;
		Handles.Label(position, "Scenario is in progress: " + scenarioIsInProgress, style);
		position.z -= 0.4f;
	}

	void OnGUI() {
		GizmoExtensions.showPath = GUI.Toggle(new Rect(220, 805, 100, 20), GizmoExtensions.showPath, "Show paths");
		GizmoExtensions.showTowerRange = GUI.Toggle(new Rect(220, 825, 150, 20), GizmoExtensions.showTowerRange, "Show attack range");
		if (GUI.Button(new Rect(220, 785, 100, 20), GizmoExtensions.buildButtonText))
			GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
		if (GizmoExtensions.showTowerTypes) {
			if (GUI.Button(new Rect(220, 605, 100, 20), "Random")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Random";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}
			if (GUI.Button(new Rect(220, 625, 100, 20), "Amethyst")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Amethyst";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 645, 100, 20), "Aquamarine")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Aquamarine";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 665, 100, 20), "Diamond")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Diamond";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 685, 100, 20), "Emerald")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Emerald";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 705, 100, 20), "Opal")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Opal";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 725, 100, 20), "Ruby")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Ruby";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 745, 100, 20), "Sapphire")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Sapphire";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}

			if (GUI.Button(new Rect(220, 765, 100, 20), "Topaz")) {
				isBuilding = true;
				GizmoExtensions.buildButtonText = "Topaz";
				GizmoExtensions.showTowerTypes = !GizmoExtensions.showTowerTypes;
			}
		}
	}

	void RemoveWall() {
		GameTile tile = board.GetTile(TouchRay);
		// bool isTower = newTowers.Find(t => t == tile);
		// if (tile != null && !isTower && availableBuilds > 0) {
		if (tile != null && availableBuilds > 0) {
			board.RemoveWall(tile);
		}
	}

	void BuildTower() {
		GameTile tile = board.GetTile(TouchRay);
		bool isTower = newTowers.Find(t => t == tile);
		if (tile != null && !isTower && availableBuilds > 0) {
			TowerType type;
			if(GizmoExtensions.buildButtonText == "Choose tower" || GizmoExtensions.buildButtonText == "Random")
				type = prepareTowerType(); //Генератор случайного типа башни. Не забыть вернуть, после тестирования
			else type = (TowerType) Enum.Parse(typeof(TowerType), GizmoExtensions.buildButtonText + level);
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
		builtTowers.Add(tile);
		newTowers.Remove(tile);
		if (!scenarioIsInProgress) {
			activeScenario = scenario.Begin();
			scenarioIsInProgress = true;
		}
		else {
			activeScenario.NextWave();
		}

		isBuildPhase = false;
		isBuilding = false;
		// for(var i = 0; i < TargetPoint.BufferedCount; i++) {
		// 	TargetPoint t = TargetPoint.GetBuffered(i);				
		// }
		foreach (GameTile t in newTowers) {
			board.ToggleWall(t);
		}

		newTowers.Clear();
	}

	void CombineOneshot() {
		if (isBuildPhase) {
			GameTile tile = board.GetTile(TouchRay);
			Tower tower = (Tower) newTowers.Find(t => t == tile).Content;
			if (tower && availableBuilds == 0) {
				Tower combined = findCombos(tower, tileContentFactory.TowerPrefabs.ToList(),
					newTowers.Select(x => (Tower) x.Content).ToList(), false)[0];
				board.ToggleTower(tile, combined.TowerType);
				chooseTower(tile);
			}
		}
		else {
			GameTile tile = board.GetTile(TouchRay);
			GameTileContent content = tile.Content;
			if (content.Type == GameTileContentType.Tower) {
				Tower tower = (Tower) content;
				Tower combined = findCombos(tower, tileContentFactory.TowerPrefabs.ToList(), builtTowers.Select(x => (Tower) x.Content).ToList(), false)[0];
				combined.Combo.ToList().ForEach(t => {
					if (t != tower.TowerType) {
						GameTile t2 = builtTowers.Find(tile => ((Tower) tile.Content).TowerType == t);
						board.ToggleWall(t2);
						builtTowers.Remove(t2);
					}
				});
				board.ToggleTower(tile, combined.TowerType);
			}
		}
	}	
	
	static List<Tower> findCombos(Tower tower, List<Tower> availableTowers, List<Tower> towers, bool oneshot) {
		List<Tower> combosTypes = availableTowers.FindAll(t => t.Combo != null && 
																		  t.Combo.Contains(tower.TowerType) &&
																		  t.Combo.All(value => towers.Select(x => x.TowerType).Contains(value))).ToList();
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
		GameTile tile = board.GetTile(TouchRay);
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

	void SpawnGift() {
		GameTile tile = board.GetTile(TouchRay);
		giftAvailable = true;
		giftTile = tile;
		board.ToggleGift(tile);
	}

	public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
		// GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		if(type == EnemyType.Bee)
			enemy.Spawn(instance.board.FlyingPath);
		else 
			enemy.Spawn(instance.board.GroundPath);
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
 * 1. Поиск пути по диагонали											+
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