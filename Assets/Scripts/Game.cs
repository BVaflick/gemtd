using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
	[SerializeField]
	Vector2Int boardSize = new Vector2Int(11, 11);

	public const int enemyLayerMask = 1 << 9;
	public const int uiLayerMask = 1 << 5;
	public const int towerLayerMask = 1 << 10;

	[SerializeField]
	Camera camera = default;

	[SerializeField]
	GameBoard board = default;

	[SerializeField]
	Ability[] MVPAbilities = default;

	[SerializeField]
	Aura MVPAuraPrefab = default;

	private int MVPMaxLevel = 3;

	[SerializeField]
	UIManager uiManager = default;

	[SerializeField]
	RectTransform mainPanel = default;

	[SerializeField]
	RectTransform headerPanel = default;

	[SerializeField]
	RectTransform damagePanel = default;

	[SerializeField]
	RectTransform recipesPanel = default;
	
	[SerializeField]
	RectTransform mazePanel = default;

	[SerializeField]
	RectTransform wallConstructionPanel = default;

	// [SerializeField]
	// RectTransform towerConstructionPanel = default;

	[SerializeField]
	RectTransform towerDescriptionPanel = default;

	[SerializeField]
	RectTransform enemyDescriptionPanel = default;

	[SerializeField]
	RectTransform towerDamagePrefab = default;

	[SerializeField]
	RectTransform statusEffectIconPrefab = default;

	[SerializeField]
	RectTransform towerRecipePrefab = default;

	[SerializeField]
	RectTransform AbilityIconPrefab = default;

	[SerializeField]
	RectTransform BuildButtonPrefab = default;

	[SerializeField]
	RectTransform Upgrade1ButtonPrefab = default;

	[SerializeField]
	RectTransform Upgrade2ButtonPrefab = default;

	[SerializeField]
	RectTransform CombineButtonPrefab = default;

	[SerializeField]
	RectTransform selectionBox = default;
	
	private float time;

	int playerHealth = 100;

	int level = 1;

	private int wave = 1;

	private int enemiesLeft = 10;

	private int kills = 0;

	private int gold = 0;

	private float progress = 10.5f;

	private int experience = 0;

	public int Experience {
		get => experience;
		set {
			experience = value;
			if (value >= 100) {
				experience = value - 100;
				level++;
			}
			else {
				experience = value;
			}
		}
	}

	private Dictionary<Tower, float> dealtDamage = new Dictionary<Tower, float>();

	private bool quickCast = false;

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

	[SerializeField]
	GameTileContentFactory tileContentFactory = default;

	[SerializeField]
	WarFactory warFactory = default;

	[SerializeField]
	private PlayerAbility[] playerAbilities = default;

	[SerializeField]
	GameScenario scenario = default;

	GameScenario.State activeScenario;

	int availableBuilds = 5;
	bool isBuildPhase = true;
	private bool isBuilding = false;

	private bool isSpawningGift = false;
	private bool giftAvailable = false;
	private GameTile giftTile;

	private bool isSwaping = false;
	private GameTile swapBuffer = null;

	List<GameTile> newTowers = new List<GameTile>();
	List<GameTile> builtTowers = new List<GameTile>();

	private GameTile hoveredTile = null;
	// private GameTile selectedTile = null;

	private Enemy selectedEnemy = null;
	private List<GameTileContent> selectedStructures = new List<GameTileContent>();
	private bool isDragSelection;
	Rect selectionRect = new Rect();
	private Vector2 dragStartPosition = Vector2.zero;
	private Vector2 dragEndPosition = Vector2.zero;

	static Game instance;

	GameBehaviorCollection enemies = new GameBehaviorCollection();

	GameBehaviorCollection nonEnemies = new GameBehaviorCollection();

	Ray TouchRay => camera.ScreenPointToRay(Input.mousePosition);

	Vector3 PressRay() {
		Vector3 mouse = Input.mousePosition;
		mouse.z = 15f;
		return camera.ScreenToWorldPoint(mouse);
	}

	bool scenarioIsInProgress = false;

	const float pausedTimeScale = 0f;

	[SerializeField]
	private Tower flyingTower = null;

	[SerializeField, Range(1f, 10f)]
	float playSpeed = 1f;

	void Awake() {
		board.Initialize(boardSize, tileContentFactory);
		// board.saveMaze(mazePanel);
		initMazePanel();
		flyingTower = Instantiate(flyingTower);
		flyingTower.gameObject.SetActive(false);
		// uiManager.showPlayerAbilities(playerAbilities);
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
		time += Time.deltaTime;
		handleInput();
		if (playerHealth <= 0) {
			playerHealth = 0;
			Debug.Log("Defeat!");
			BeginNewGame();
		}

		if (scenarioIsInProgress && !activeScenario.Progress() && enemies.IsEmpty) {
			Debug.Log("Victory!");
			BeginNewGame();
		}
		else if (scenarioIsInProgress && !activeScenario.WaveIsInProgress() && enemies.IsEmpty) {
			if (!isBuildPhase) {
				calculateMVP();
				wave++;
				enemiesLeft = (int) progress;
				isBuildPhase = true;
				availableBuilds = 5;
				if (giftAvailable) board.ToggleGift(giftTile);
			}
		}

		else if (wallConstructionPanel.gameObject.activeSelf) {
			Vector3 pos = selectedStructures[0].transform.position;
			pos.z += 1f;
			wallConstructionPanel.position = camera.WorldToScreenPoint(pos);
		}

		if (isSpawningGift || isSwaping || isBuilding && availableBuilds > 0) {
			GameTile tile = board.GetTile(TouchRay);
			if (tile != null) {
				if (hoveredTile != null && hoveredTile != tile) hoveredTile.Dehover();
				tile.Hover();
				hoveredTile = tile;
				if (isBuilding) {
					flyingTower.gameObject.SetActive(true);
					flyingTower.transform.position = tile.transform.position;
				}
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
		if (selectedEnemy != null) {
			showEnemyDescription();
		}
		else if (selectedStructures.Count != 0) {
			// showTowerDescription();
		}
		else {
			showMainPanel();
		}

		showHeader();
	}

	private void initMazePanel() {
		string path = "Assets/Resources/test.txt";
		StreamReader reader = new StreamReader(path); 
		String mazeString = reader.ReadToEnd();
		for (var i = 0; i < mazeString.Length; i++) {
			int x = i / boardSize.x;
			int y = i - boardSize.x * x;
			int newX = boardSize.x - x - 1;
			board.mazes[3, newX, y] = Int32.Parse(mazeString[i].ToString());
		}
		reader.Close();
		for (int maze = 0; maze < board.mazes.GetLength(0); maze++) {
			Transform m = mazePanel.GetChild(maze);
			int maze1 = maze;
			m.GetComponent<Button>().onClick.AddListener(() => {
				if (board.CurrentMaze == maze1) {
					board.ShowMaze = !board.ShowMaze;
					if (board.ShowMaze) {
						board.CurrentMaze = maze1;
						m.GetComponent<Image>().color = Color.gray;
					}
					else {
						board.CurrentMaze = maze1;
						m.GetComponent<Image>().color = Color.clear;
					}
				}
				else {
					mazePanel.GetChild(board.CurrentMaze).GetComponent<Image>().color = Color.clear;
					m.GetComponent<Image>().color = Color.gray;
					board.CurrentMaze = maze1;
					board.ShowMaze = true;
				}
			});
			drawMazeOnPanel(maze1);
		}
	}

	void drawMazeOnPanel(int index) {
		Transform maze = mazePanel.GetChild(index);
		for (int row = 0; row < board.mazes.GetLength(1); row++) {
			for (int column = 0; column < board.mazes.GetLength(2); column++) {
				switch (board.mazes[index, row, column]) {
					case 0:
						maze.GetChild(row * boardSize.x + column).GetComponent<Image>().color = Color.white;
						break;
					case 1:
						maze.GetChild(row * boardSize.x + column).GetComponent<Image>().color = new Color(.39f, .52f, .52f, 1);
						break;
					case 2:
						maze.GetChild(row * boardSize.x + column).GetComponent<Image>().color = new Color(1f, .41f, .41f, 1);
						break;
				}
			}
		}
	}

	void calculateMVP() {
		var towersWithoutMaxMVP = dealtDamage.Where(x => !x.Key.Auras.Exists(aura => aura.buff is MVPAura))
			.ToDictionary(x => x.Key, x => x.Value);
		dealtDamage.Clear();
		if (towersWithoutMaxMVP.Count == 0) return;
		Tower mvp = towersWithoutMaxMVP.Keys.First();
		Ability mvpAbility = mvp.Abilities.Find(x => x.Buff.name1.Contains("MVP"));
		if (mvpAbility == null) {
			mvp.Abilities.Add(Instantiate(MVPAbilities[0]));
			return;
		}

		int mvpLevel = mvpAbility.level + 1;
		mvp.Abilities.Remove(mvpAbility);
		Destroy(mvpAbility);
		if (mvpLevel >= MVPMaxLevel)
			mvp.Auras.Add(Instantiate(MVPAuraPrefab));
		else
			mvp.Abilities.Add(Instantiate(MVPAbilities[mvpLevel - 1]));
	}

	void handleInput() {
		Transform cameraTransform = camera.transform;
		if (Input.GetKeyDown(KeyCode.Q)) {
			if (quickCast) {
				GameTile tile = board.GetTile(TouchRay);
				BuildTower(tile);
			}
			else {
				if (availableBuilds > 0) isBuilding = true;
			}
		}
		else if (Input.GetKeyDown(KeyCode.W)) {
			// RemoveWall();
			board.ToggleWall(board.GetTile(TouchRay));
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			GameTile tile = board.GetTile(TouchRay);
			if (Input.GetKey(KeyCode.LeftShift)) {
				CombineSame(tile, false);
			}
			else if (Input.GetKey(KeyCode.LeftControl)) {
				CombineOneshot(tile, null);
			}
			else {
				CombineSame(tile, true);
			}
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			if (recipesPanel.gameObject.activeSelf) closeTowerRecipes();
			else showTowerRecipes();
		}

		if (Input.GetMouseButtonDown(0)) {
			dragStartPosition = Input.mousePosition;
		}
		else if (Input.GetMouseButton(0)) {
			dragEndPosition = Input.mousePosition;
			if (!isDragSelection && (Mathf.Abs(dragStartPosition.x - dragEndPosition.x) > 5 ||
			                         Mathf.Abs(dragStartPosition.y - dragEndPosition.y) > 5)) {
				isDragSelection = true;
			}

			if (isDragSelection) showSelectionBox();
		}
		else if (Input.GetMouseButtonUp(0)) {
			if (isDragSelection) {
				selectStructureWithDrag();
				dragStartPosition = Vector2.zero;
				dragEndPosition = Vector2.zero;
				showSelectionBox();
				isDragSelection = false;
			}
			else {
				if (Input.GetKey(KeyCode.LeftControl)) {
					deselectAndClose();
					builtTowers.ForEach(towerTile => {
						towerTile.Content.switchSelection();
						selectedStructures.Add(towerTile.Content);
					});
					if (selectedStructures.Count > 0) showTowerDescription();
				}
				else HandleTouch();
			}
		}

		if (Input.GetMouseButtonDown(1)) {
			HandleAlternativeTouch();
		}

		float a = Input.mouseScrollDelta.y;
		if (a > 0) {
			Vector3 pos = cameraTransform.position;
			Vector3 rot = cameraTransform.eulerAngles;
			if (pos.y > 5) {
				pos.y -= a;
				if (pos.y < 8) {
					rot.x -= 10f;
					pos.z -= 1.25f;
					cameraTransform.eulerAngles = rot;
				}
			}

			cameraTransform.position = pos;
			print(pos + " " + rot);
		}
		else if (a < 0) {
			Vector3 pos = cameraTransform.position;
			Vector3 rot = cameraTransform.eulerAngles;
			if (pos.y < 18) {
				pos.y -= a;
				if (pos.y <= 8) {
					rot.x += 10f;
					pos.z += 1.25f;
					cameraTransform.eulerAngles = rot;
				}
			}

			cameraTransform.position = pos;
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
			if (quickCast) SpawnGift();
			else startSpawningGift();
		}

		if (Input.GetKeyDown(KeyCode.S)) {
			if (quickCast) SwapTowers();
			else startSwaping();
		}
		
		if (Input.GetKeyDown(KeyCode.Z)) {
			board.saveMaze(mazePanel);
			// initMazePanel();
			if (board.CurrentMaze == 3)
				deselectMazeOnPanel();
			drawMazeOnPanel(3);
		}
		
		if (Input.GetKeyDown(KeyCode.X)) {
			deselectMazeOnPanel();
		}
		
		if (Input.GetKeyDown(KeyCode.C)) {
			mazePanel.gameObject.SetActive(!mazePanel.gameObject.activeSelf);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			// if (selectedTile != null) {
			// 	towerConstructionPanel.gameObject.SetActive(false);
			// 	towerDescription.SetActive(false);
			// 	wallConstructionPanel.gameObject.SetActive(false);
			// 	mainPanel.SetActive(true);
			// 	if (selectedTile != null && selectedTile.Content.Type == GameTileContentType.Tower) {
			// 		((Tower) selectedTile.Content).switchSelection();
			// 	}
			// 	selectedTile = null;
			// }

			if (isBuilding) isBuilding = !isBuilding;
			else if (isSwaping) isSwaping = !isSwaping;
			else if (isSpawningGift) isSpawningGift = !isSpawningGift;
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

		Vector3 pos1 = cameraTransform.position;
		if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - 10) {
			if (pos1.z < 5) pos1.z += 20f * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= 10) {
			if (pos1.z > -10) pos1.z -= 20f * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= 10) {
			if (pos1.x > -5) pos1.x -= 20f * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - 10) {
			if (pos1.x < 5) pos1.x += 20f * Time.deltaTime;
		}

		cameraTransform.position = pos1;
	}

	void BeginNewGame() {
		deselectAndClose();
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

	public static void EnemyReachedFlag(int index) {
		float regress = 0.0005f * (index == 0 ? 0 : Mathf.Pow(2, index - 1));
		float newProgress = instance.progress - regress;
		instance.progress = (int) newProgress < (int) instance.progress ? newProgress - .5f : newProgress;
	}

	public static void EnemyReachedDestination(int damage) {
		instance.playerHealth -= damage;
		float newProgress = instance.progress - 0.05f;
		instance.progress = (int) newProgress < (int) instance.progress ? newProgress - .5f : newProgress;
	}

	public static void EnemyDied(int gold) {
		instance.gold += gold;
		float newProgress = instance.progress + 0.0075f;
		instance.progress = (int) newProgress > (int) instance.progress ? newProgress + .5f : newProgress;
		instance.Experience += 2;
		instance.enemiesLeft--;
		instance.kills++;
	}

	public static void RecordDealtDamage(Tower tower, float damage) {
		Dictionary<Tower, float> dictionary = instance.dealtDamage;
		if (!dictionary.ContainsKey(tower)) dictionary.Add(tower, damage);
		else {
			dictionary[tower] += damage;
		}

		instance.dealtDamage = dictionary.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
		instance.showTowerDamage();
	}

	public void startBuilding() {
		if (availableBuilds > 0) isBuilding = true;
	}

	public void startSpawningGift() {
		if (isBuildPhase) isSpawningGift = true;
	}

	public void startSwaping() {
		isSwaping = true;
	}

	public void buildSelected() {
		if (isBuildPhase && availableBuilds == 0) {
			chooseTower(board.GetTile(selectedStructures[0].transform.localPosition));
			(selectedStructures[0] as Tower).switchNewTowerCircle();
			deselectAndClose();
		}
	}

	public void combineSelected(bool two) {
		if (isBuildPhase && availableBuilds == 0) {
			CombineSame(board.GetTile(selectedStructures[0].transform.localPosition), two);
			deselectAndClose();
		}
	}

	public void combineOneshotSelected(Tower combo) {
		// if (availableBuilds == 0) {
		CombineOneshot(board.GetTile(selectedStructures[0].transform.localPosition), combo);
		deselectAndClose();
		// }
	}

	public void removeSelectedWall() {
		if (isBuildPhase && selectedStructures[0].Type == GameTileContentType.Wall) {
			RemoveWall(board.GetTile(selectedStructures[0].transform.localPosition));
			wallConstructionPanel.gameObject.SetActive(false);
			deselectAndClose();
		}
	}

	void HandleAlternativeTouch() {
		if (isBuilding) isBuilding = !isBuilding;
		else if (isSwaping) isSwaping = !isSwaping;
		else if (isSpawningGift) isSpawningGift = !isSpawningGift;
		if (selectedStructures.Count > 0 &&
		    Physics.Raycast(TouchRay, out RaycastHit hit, float.MaxValue, enemyLayerMask)) {
			selectedStructures.FindAll(structure => structure.Type == GameTileContentType.Tower).ForEach(tower =>
				(tower as Tower).aimTarget(hit.collider.GetComponent<TargetPoint>()));
			hit.transform.root.gameObject.GetComponent<Enemy>().showAim();
		}

		// Добавление респаунов и точек назначения
		// GameTile tile = board.GetTile(TouchRay);
		// if (tile != null) {
		// 	if (Input.GetKey(KeyCode.LeftShift)) {
		// 		board.ToggleDestination(tile);
		// 	}
		// 	else {
		// 		board.ToggleSpawnPoint(tile);
		// 	}
		// }
	}

	void HandleTouch() {
		if (EventSystem.current.IsPointerOverGameObject()) {
			return;
		}

		if (isBuilding) {
			BuildTower(board.GetTile(TouchRay));
			isBuilding = false;
			return;
		}

		if (isSpawningGift) {
			SpawnGift();
			return;
		}

		if (isSwaping) {
			SwapTowers();
			return;
		}

		if (Physics.Raycast(TouchRay, out RaycastHit hit, float.MaxValue, enemyLayerMask)) {
			selectEnemy(hit.transform.root.GetComponent<Enemy>());
		}
		else {
			GameTile selectedTile = board.GetTile(TouchRay);
			if (selectedTile == null || selectedTile.Content.Type != GameTileContentType.Tower) {
				if (!Input.GetKey(KeyCode.LeftShift)) deselectAndClose();
				return;
			}

			selectStructure(selectedTile.Content);
		}
	}

	void selectEnemy(Enemy enemy) {
		deselectAndClose();
		selectedEnemy = enemy;
		enemy.swithSelection();
		showEnemyDescription();
	}

	void showSelectionBox() {
		Vector2 boxPosition = (dragStartPosition + dragEndPosition) / 2;
		selectionBox.position = boxPosition;
		Vector2 boxSize = new Vector2(Mathf.Abs(dragStartPosition.x - dragEndPosition.x),
			Mathf.Abs(dragStartPosition.y - dragEndPosition.y));
		Vector2 extents = boxSize / 2.0f;
		selectionBox.sizeDelta = boxSize;
		selectionRect.min = boxPosition - extents;
		selectionRect.max = boxPosition + extents;
	}

	void selectStructureWithDrag() {
		if (!Input.GetKey(KeyCode.LeftShift) || selectedEnemy != null) deselectAndClose();
		List<GameTile> allTowers = builtTowers;
		allTowers.AddRange(newTowers);
		allTowers.ForEach(newTower => {
			if (selectionRect.Contains(camera.WorldToScreenPoint(newTower.transform.position)) &&
			    !selectedStructures.Contains(newTower.Content)) {
				selectedStructures.Insert(0, newTower.Content);
				newTower.Content.switchSelection();
			}
		});
		if (selectedStructures.Count > 0) showTowerDescription();
	}

	void selectStructure(GameTileContent structure) {
		if (selectedEnemy != null) deselectAndClose();
		else if (selectedStructures.Contains(structure) && selectedStructures.Count == 1) {
			if (Input.GetKey(KeyCode.LeftShift)) deselectAndClose();
			return;
		}

		if (Input.GetKey(KeyCode.LeftShift) && selectedStructures.Contains(structure) && selectedStructures.Count > 1) {
			selectedStructures.Remove(structure);
		}
		else {
			if (!Input.GetKey(KeyCode.LeftShift)) deselectAll();
			selectedStructures.Insert(0, structure);
		}

		// if (isBuildPhase && availableBuilds == 0 && selectedStructures.Count == 1) showTowerConstructionPanel();
		// else towerConstructionPanel.gameObject.SetActive(false);
		structure.switchSelection();
		showTowerDescription();
	}

	void deselectAll() {
		if (selectedEnemy != null) {
			selectedEnemy.swithSelection();
			selectedEnemy = null;
		}
		else if (selectedStructures.Count > 0) {
			selectedStructures.ForEach(s => s.switchSelection());
			selectedStructures.Clear();
		}
	}

	void deselectAndClose() {
		deselectAll();
		closeAllPanels();
		// mainPanel.gameObject.SetActive(true);
		showMainPanel();
	}

	void closeAllPanels() {
		mainPanel.gameObject.SetActive(false);
		wallConstructionPanel.gameObject.SetActive(false);
		// towerConstructionPanel.gameObject.SetActive(false);
		towerDescriptionPanel.gameObject.SetActive(false);
		enemyDescriptionPanel.gameObject.SetActive(false);
	}

	void deselectMazeOnPanel() {
		board.ShowMaze = !board.ShowMaze;
		mazePanel.GetChild(board.CurrentMaze).GetComponent<Image>().color = board.ShowMaze ? Color.gray : Color.clear;
	}

	// void showTowerConstructionPanel() {
	// 	towerConstructionPanel.gameObject.SetActive(true);
	// 	Tower selectedTower = selectedStructures[0] as Tower;
	// 	foreach (Transform child in towerConstructionPanel.transform) {
	// 		switch (child.name) {
	// 			case "Upgrade1":
	// 				child.gameObject.SetActive(newTowers
	// 					.FindAll(tower => (tower.Content as Tower).TowerType == selectedTower.TowerType).Count >= 2);
	// 				break;
	// 			case "Upgrade2":
	// 				child.gameObject.SetActive(newTowers
	// 					.FindAll(tower => (tower.Content as Tower).TowerType == selectedTower.TowerType).Count >= 4);
	// 				break;
	// 			case "Combine":
	// 				child.gameObject.SetActive(
	// 					findCombos(selectedTower, newTowers.Select(x => (Tower) x.Content).ToList()).Count > 0);
	// 				break;
	// 		}
	// 	}
	// }

	void showEnemyDescription() {
		enemyDescriptionPanel.gameObject.SetActive(true);
		foreach (Transform child in enemyDescriptionPanel.transform) {
			if (child.name == "Label") {
				child.GetComponent<Text>().text = selectedEnemy.name.Replace("(Clone)", "");
			}
			else if (child.name == "EnemyParams") {
				RectTransform panel = child.GetComponent<RectTransform>();
				foreach (Transform towerParam in panel.transform) {
					switch (towerParam.name) {
						case "Damage Value":
							towerParam.GetComponent<Text>().text =
								selectedEnemy.Health +
								""; // + (selectedTower.Damage != 0 ? "<color=green>+" + selectedTower.Damage + "</color>" : "");
							break;
						case "Speed Value":
							towerParam.GetComponent<Text>().text = selectedEnemy.speed +
							                                       getColoredAdditionalParam(selectedEnemy
								                                       .additionalSpeed);
							break;
						case "Armor Value":
							towerParam.GetComponent<Text>().text = selectedEnemy.armor +
							                                       getColoredAdditionalParam(selectedEnemy
								                                       .additionalArmor);
							break;
					}
				}
			}
			else if (child.name == "HealthBar") {
				child.GetComponent<Slider>().value = selectedEnemy.Health / selectedEnemy.FullHealth;
			}
			else if (child.name == "HP") {
				child.GetComponent<Text>().text = Math.Ceiling(selectedEnemy.Health) + "/" + selectedEnemy.FullHealth;
			}
			else if (child.name == "EnemyStatusEffects") {
				RectTransform panel = child.GetComponent<RectTransform>();
				if (selectedEnemy.Health > 0) {
					foreach (Transform image in panel) {
						Destroy(image.gameObject);
					}
				}

				for (var i = 0; i < selectedEnemy.VisualEffects.Count; i++) {
					RectTransform image = Instantiate(statusEffectIconPrefab);
					image.GetComponent<Tooltip>().tip = (selectedEnemy.VisualEffects.Behaviors[i] as WarEntity).name1;
					Image icon = image.GetComponent<Image>();
					icon.sprite = (selectedEnemy.VisualEffects.Behaviors[i] as WarEntity).icon;
					icon.transform.SetParent(panel);
				}
			}
		}
	}

	void showTowerDescription() {
		towerDescriptionPanel.gameObject.SetActive(true);
		Tower selectedTower = selectedStructures[0] as Tower;
		foreach (Transform child in towerDescriptionPanel.transform) {
			if (child.name == "Label") {
				child.GetComponent<Text>().text = selectedTower.name.Replace("(Clone)", "");
			}
			else if (child.name == "TowerParams") {
				RectTransform panel = child.GetComponent<RectTransform>();
				panel.GetComponent<Tooltip>().tip = "привет";
				panel.GetComponent<Tooltip>().range = selectedTower.Range;
				panel.GetComponent<Tooltip>().rangeCirclePos = selectedTower.transform.position;
				foreach (Transform towerParam in panel.transform) {
					switch (towerParam.name) {
						case "Damage Value":
							towerParam.GetComponent<Text>().text =
								selectedTower.Dmg + getColoredAdditionalParam(selectedTower.Damage);
							break;
						case "Attackspeed Value":
							towerParam.GetComponent<Text>().text =
								selectedTower.AS + getColoredAdditionalParam(selectedTower.AttackSpeed);
							break;
						case "Range Value":
							towerParam.GetComponent<Text>().text = selectedTower.Range + "";
							break;
					}
				}
			}
			else if (child.name == "TowerAbilities") {
				RectTransform panel = child.GetComponent<RectTransform>();
				foreach (Transform ability in panel) {
					Destroy(ability.gameObject);
				}

				foreach (Ability ability in selectedTower.Abilities) {
					RectTransform image = Instantiate(AbilityIconPrefab);
					Image icon = image.GetComponent<Image>();
					icon.sprite = ability.icon;
					image.GetComponent<Tooltip>().tip = ability.Buff.name1;
					image.GetComponent<Tooltip>().range = 0f;
					image.SetParent(panel);
				}

				foreach (Aura aura in selectedTower.Auras) {
					RectTransform image = Instantiate(AbilityIconPrefab, panel, true);
					Image icon = image.GetComponent<Image>();
					icon.sprite = aura.buff ? aura.buff.icon : aura.icon;
					image.GetComponent<Tooltip>().tip = aura.buff ? aura.buff.name1 : aura.GetType().Name;
					image.GetComponent<Tooltip>().range = 3.5f;
					image.GetComponent<Tooltip>().rangeCirclePos = selectedTower.transform.position;
				}

				if (isBuildPhase && availableBuilds == 0) {
					RectTransform buildButton = Instantiate(BuildButtonPrefab, panel, true);
					buildButton.GetComponent<Button>().onClick.AddListener(buildSelected);
					if (newTowers.FindAll(tower =>
						(tower.Content as Tower).TowerType == selectedTower.TowerType).Count >= 2) {
						RectTransform upgrade1Button = Instantiate(Upgrade1ButtonPrefab);
						upgrade1Button.GetComponent<Button>().onClick.AddListener(() => combineSelected(true));
						upgrade1Button.SetParent(panel);
					}

					if (newTowers.FindAll(tower =>
						(tower.Content as Tower).TowerType == selectedTower.TowerType).Count >= 4) {
						RectTransform upgrade2Button = Instantiate(Upgrade2ButtonPrefab, panel, true);
						upgrade2Button.GetComponent<Button>().onClick.AddListener(() => combineSelected(false));
					}
				}

				if (isBuildPhase && availableBuilds == 0 || !isBuildPhase) {
					List<GameTile> list;
					if (isBuildPhase) list = newTowers.FindAll(tile => tile.Content is Tower);
					else list = builtTowers.FindAll(tile => tile.Content is Tower);
					List<Tower> combos = findCombos(selectedTower, list.Select(x => (Tower) x.Content).ToList());
					if (combos.Count > 0) {
						foreach (Tower combo in combos) {
							RectTransform combineButton = Instantiate(CombineButtonPrefab, panel, true);
							Button button = combineButton.GetComponent<Button>();
							button.onClick.AddListener(() => combineOneshotSelected(combo));
							combineButton.transform.GetChild(2).GetComponent<Text>().text = combo.TowerType.ToString();
						}
					}
				}
			}
			else if (child.name == "TowerStatusEffects") {
				RectTransform panel = child.GetComponent<RectTransform>();
				foreach (Transform image in panel) {
					Destroy(image.gameObject);
				}

				List<Buff> thirdPartyStatusEffects =
					selectedTower.StatusEffects.FindAll(se =>
						!selectedTower.Abilities.Select(a => a.Buff).Contains(se));
				foreach (Buff buff in thirdPartyStatusEffects) {
					RectTransform image = Instantiate(statusEffectIconPrefab);
					image.GetComponent<Tooltip>().tip = buff.name1;
					Image icon = image.GetComponent<Image>();
					icon.sprite = buff.icon;
					icon.gameObject.SetActive(true);
					icon.transform.SetParent(panel);
				}
			}
			else if (child.name == "HP") {
				child.GetComponent<Text>().text = playerHealth + "/100";
			}
			else if (child.name == "HealthBar") {
				child.GetComponent<Slider>().value = playerHealth / 100f;
			}
		}
	}

	void showTowerDamage() {
		foreach (Transform line in damagePanel) {
			Destroy(line.gameObject);
		}

		foreach (var item in dealtDamage) {
			RectTransform line = Instantiate(towerDamagePrefab, damagePanel, true);
			Tower tower = item.Key;
			Ability mvp = tower.Abilities.Find(x => x.Buff.name1.Contains("MVP"));
			bool isMvpMax = tower.Auras.Exists(x => x.buff.name1.Contains("MVP"));
			string towerName = tower.name.Replace("(Clone)", "");
			line.GetChild(0).GetComponent<Text>().text = mvp || isMvpMax
				? getColoredString(towerName + " (MVP" + (isMvpMax ? " MAX" : mvp.level + "") + ")")
				: towerName;
			line.GetChild(1).GetComponent<Text>().text = (int) item.Value + "";
			line.gameObject.SetActive(true);
		}
	}

	void showTowerRecipes() {
		recipesPanel.gameObject.SetActive(true);
		foreach (Transform line in recipesPanel) {
			Destroy(line.gameObject);
		}

		List<Tower> combosTypes = tileContentFactory.TowerPrefabs.ToList().FindAll(t => t.Combo.Length > 0);
		foreach (Tower comboType in combosTypes) {
			RectTransform line = Instantiate(towerRecipePrefab, recipesPanel, true);
			line.GetChild(0).GetChild(0).GetComponent<Text>().text = comboType.name.Replace("(Clone)", "");
			for (int i = 0; i < comboType.Combo.Length; i++) {
				TowerType ingredient = comboType.Combo[i];
				bool doesTowerExist = builtTowers.Exists(tile => (tile.Content as Tower).TowerType == ingredient);
				line.GetChild(i + 1).GetChild(0).GetComponent<Text>().text =
					doesTowerExist ? getColoredString(ingredient.ToString()) : ingredient.ToString();
				if (newTowers.Exists(tile => (tile.Content as Tower).TowerType == ingredient)) {
					line.GetChild(i + 1).GetComponent<Image>().color = new Color(1f, 1f, 0.5f, 0.5f);
					;
				}
			}
		}
	}

	void closeTowerRecipes() {
		recipesPanel.gameObject.SetActive(false);
	}

	void showMainPanel() {
		mainPanel.gameObject.SetActive(true);
		foreach (Transform child in mainPanel.transform) {
			if (child.name == "MainParams") {
				RectTransform panel = child.GetComponent<RectTransform>();
				foreach (Transform mainParam in panel.transform) {
					switch (mainParam.name) {
						case "Gold Value":
							mainParam.GetComponent<Text>().text = gold.ToString();
							break;
						case "Builds Left Value":
							mainParam.GetComponent<Text>().text = availableBuilds + "/5";
							break;
						case "Level Value":
							mainParam.GetComponent<Text>().text = level + getColoredAdditionalParam(experience) + "%";
							break;
					}
				}
			}
			else if (child.name == "HP") {
				child.GetComponent<Text>().text = playerHealth + "/100";
			}
			else if (child.name == "HealthBar") {
				child.GetComponent<Slider>().value = playerHealth / 100f;
			}
		}
	}

	void showHeader() {
		headerPanel.GetChild(1).GetComponent<Text>().text = getTime();
		headerPanel.GetChild(3).GetComponent<Text>().text = "" + wave;
		headerPanel.GetChild(5).GetComponent<Text>().text = "" + enemiesLeft;
		headerPanel.GetChild(7).GetComponent<Text>().text = "" + kills;
		headerPanel.GetChild(9).GetComponent<Text>().text = (int) progress + "+" + (int)(progress % 1 * 100) + "%";
		headerPanel.GetChild(11).GetComponent<Text>().text = "" + board.GroundPath.Count;
	}

	string getTime() {
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
	}

	string getColoredAdditionalParam(float additionalParam) {
		return additionalParam == 0 ? "" :
			additionalParam > 0 ? "<color=green>+" + additionalParam + "</color>" :
			"<color=red>" + additionalParam + "</color>";
	}
	
	string getColoredString(string text) {
		return "<color=green>" + text + "</color>";
	}

	void OnDrawGizmos() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.white;
		Vector3 position = new Vector3(-15f, 0f, 8f);
		Handles.Label(position, "Wave: " + wave, style);
		position.z -= 0.4f;
		Handles.Label(position, "Enemies: " + enemies.Count, style);
		position.z -= 0.4f;
		Handles.Label(position, "HP: " + playerHealth, style);
		position.z -= 0.4f;
		Handles.Label(position, "Level: " + level + "+" + experience + "%", style);
		position.z -= 0.4f;
		Handles.Label(position, "Progress: " + (int) progress + "+" + (progress - (int) progress) * 100 + "%", style);
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
		if (selectedStructures.Count == 1) {
			Tower selectedTower = selectedStructures[0] as Tower;
			position = selectedTower.transform.localPosition;
			
			Handles.color = new Color(85, 215, 55, 0.01f);
			Handles.DrawSolidDisc(position, transform.up, selectedTower.Range);
			Handles.color = new Color(85, 215, 55, 1f);
			Handles.DrawWireDisc(position, transform.up, selectedTower.Range);
		}
	}

	void OnGUI() {
		// if (isDragSelection) {
		if (selectedStructures.Count > 0) {
			Vector3 position = selectedStructures[0].transform.position;
			position.y += 0.01f;
			Handles.color = new Color(1, 1, 1, 0.05f);
			Handles.DrawSolidDisc(position, transform.up, 2f);
		}
		// }

		GizmoExtensions.showPath = GUI.Toggle(new Rect(220, 805, 100, 20), GizmoExtensions.showPath, "Show paths");
		GizmoExtensions.showTowerRange = GUI.Toggle(new Rect(220, 825, 150, 20), GizmoExtensions.showTowerRange,
			"Show attack range");
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

	void RemoveWall(GameTile tile) {
		// bool isTower = newTowers.Find(t => t == tile);
		// if (tile != null && !isTower && availableBuilds > 0) {
		if (tile != null && availableBuilds > 0) {
			board.RemoveWall(tile);
		}
	}

	void BuildTower(GameTile tile) {
		bool isTower = newTowers.Find(t => t == tile);
		if (tile != null && !isTower && availableBuilds > 0) {
			TowerType type;
			if (GizmoExtensions.buildButtonText == "Choose tower" || GizmoExtensions.buildButtonText == "Random")
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
				(tile.Content as Tower).switchNewTowerCircle();
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

		sum = 0;
		rand = Random.value;
		for (int i = 0; i < towerLevelProbability[level - 1].Length; i++) {
			sum += towerLevelProbability[level - 1][i];
			if (sum > Math.Round(rand, 3)) {
				result += i + 1;
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

	void CombineOneshot(GameTile tile, Tower combined) {
		if (isBuildPhase) {
			Tower tower = (Tower) newTowers.Find(t => t == tile).Content;
			if (tower && availableBuilds == 0) {
				if(combined == null) combined = findCombos(tower, newTowers.Select(x => (Tower) x.Content).ToList())[0];
				board.ToggleTower(tile, combined.TowerType);
				deselectAndClose();
				chooseTower(tile);
			}
		}
		else {
			GameTileContent content = tile.Content;
			if (content.Type == GameTileContentType.Tower) {
				Tower tower = (Tower) content;
				if(combined == null) combined = findCombos(tower, builtTowers.Select(x => (Tower) x.Content).ToList())[0];
				Ability currentMvp = tower.Abilities.Find(ability => ability.Buff is MVPAbility);
				int comboMVP = currentMvp ? currentMvp.level : 0;
				combined.Combo.ToList().ForEach(t => {
					if (t != tower.TowerType) {
						GameTile t2 = builtTowers.Find(tile => ((Tower) tile.Content).TowerType == t);
						calculateMVPCombo(ref comboMVP, t2.Content as Tower);
						board.ToggleWall(t2);
						builtTowers.Remove(t2);
					}
				});
				board.ToggleTower(tile, combined.TowerType);
				Tower combinedTower = tile.Content as Tower;
				if (comboMVP != 0 && comboMVP < MVPMaxLevel) {
					Ability mvp = Instantiate(MVPAbilities[comboMVP - 1]);
					mvp.level = comboMVP;
					combinedTower.Abilities.Add(mvp);
				} else if (comboMVP >= MVPMaxLevel) {
					combinedTower.Auras.Add(Instantiate(MVPAuraPrefab));
				}
				deselectAndClose();
			}
		}
	}

	List<Tower> findCombos(Tower tower, List<Tower> towers) {
		List<Tower> combosTypes = tileContentFactory.TowerPrefabs.ToList().FindAll(t => t.Combo != null &&
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

	void calculateMVPCombo(ref int mvpLevel, Tower tower) {
		if (tower.Auras.Find(aura => aura.buff is MVPAura)) {
			mvpLevel = MVPMaxLevel;
			return;
		}

		Ability mvp = tower.Abilities.Find(ability => ability.Buff is MVPAbility);
		if (mvp) {
			mvpLevel += mvp.level;
		}
	}

	void CombineSame(GameTile tile, bool two) {
		Tower tower = (Tower) newTowers.Find(t => t == tile).Content;
		if (tower &&
		    availableBuilds == 0 &&
		    char.IsDigit(tower.TowerType.ToString().Last()) &&
		    newTowers.FindAll(towerTile => ((Tower) towerTile.Content).TowerType == tower.TowerType).Count >
		    (two ? 1 : 3)) {
			int type = (int) tower.TowerType;
			board.ToggleTower(tile, (TowerType) type + (two ? 1 : 2));
			deselectAndClose();
			chooseTower(tile);
		}
	}

	void SpawnGift() {
		GameTile tile = board.GetTile(TouchRay);
		giftAvailable = true;
		giftTile = tile;
		board.ToggleGift(tile);
		isSpawningGift = false;
	}

	void SwapTowers() {
		GameTile tile = board.GetTile(TouchRay);
		if (!(tile.Content is Tower) && (tile.Content.Type != GameTileContentType.Wall)) return;
		if (swapBuffer == null) {
			swapBuffer = tile;
		}
		else {
			GameTileContent temp = swapBuffer.Content;
			swapBuffer.setTower(tile.Content);
			tile.setTower(temp);
			if (tile.Content.Type == GameTileContentType.Wall) {
				if (builtTowers.Contains(tile)) builtTowers[builtTowers.IndexOf(tile)] = swapBuffer;
				else newTowers[newTowers.IndexOf(tile)] = swapBuffer;
			} else if (swapBuffer.Content.Type == GameTileContentType.Wall) {
				if (builtTowers.Contains(swapBuffer)) builtTowers[builtTowers.IndexOf(swapBuffer)] = tile;
				else newTowers[newTowers.IndexOf(swapBuffer)] = tile;
			}
			isSwaping = false;
			swapBuffer = null;
		}
	}

	public static int getCurrentProgress() {
		return instance.activeScenario.CurrentWave() == 9 ? 1 : (int) instance.progress;
	}

	public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
		// GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		if (type == EnemyType.Bee)
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
		return instance.warFactory.Explosion;
	}
	
	public static Fire SpawnFire(bool flag) {
		return instance.warFactory.Fire;
	}

	public static Corrosion SpawnCorrosion() {
		return instance.warFactory.Corrosion;
	}

	public static Ice SpawnIce() {
		return instance.warFactory.Ice;
	}

	public static Toxin SpawnToxin() {
		return instance.warFactory.Toxin;
	}
}

/*
 * TODO:
 * 1. Поиск пути по диагонали											+
 * 2. Комбинирование башен не только в режиме постройки					+
 * 3. Привести атрибуты башен, юнитов и способностей в соответствие		-
 * 3а. Адаптировать фабрику для удобной настройки волн: 				
 *     (жизни фиксированные для волны, но машстаб зависит от урона)		-
 * 4. Добавить механики:			
 * 		а. Иммунитет к магии											-
 * 		б. Увороты														-
 * 		в. Отскок снарядов для цепной молнии и чайника					-
 * 		г. ПВО															-
 * 		д. Невидимость													+
 * 5. Добавить способности для башен:		
 * 		а. Точность														-
 * 		б. Дальность													-
 * 		в. Замедление по области (Желтый сапфир)						+
 * 		г. Иммунитет к магии для башен									-
 * 6. Летающие юниты													+
 * 7. Выделение объектов												+-
 * 8. Прогресс волн														+
 * 9. Система опыта														+
 * 10. Убрать возможность строить поверх существующей башни				-
 * 11. Педали															-
 * 12. Визуализация нанесенного урона									-
 * 13. Шкала здоровья													+
 * 14. MVP																+-
 * 15. GUI																+-
 */