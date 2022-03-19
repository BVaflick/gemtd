using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class Enemy : GameBehavior {

	[SerializeField]
	Transform model = default;

	[SerializeField]
	GameObject blastPatricals = null;
	
	[SerializeField]
	Material material = null;
	
	[SerializeField]
	EnemyAnimationConfig animationConfig = default;
	
	EnemyAnimator animator;
	
	EnemyFactory originFactory;

	GameTile tileFrom, tileTo;
	Vector3 positionFrom, positionTo;
	Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;
	float progress, progressFactor;
	List<GameTile> path;
	List<Direction> directions;
	public float armor { get; set; }
	public float additionalArmor { get; set; }
	public float speed { get; set; }
	public float additionalSpeed { get; set; }
	float originalSpeed;
	int num = 0;
	public float Health { get; set; }
	public float Scale { get; private set; }
	public GameBehaviorCollection Effects { get; set; }

	public EnemyFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	void OnDrawGizmos() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.white;
		Handles.color = Color.red;
		Vector3 position = transform.localPosition;
		Handles.Label(position, "HP: " + Mathf.Ceil(Health), style);
		position.z -= 0.3f;
		Handles.Label(position, "SP: " + speed + (additionalSpeed != 0 ? "" + additionalSpeed : ""), style);
		position.z -= 0.3f;
		Handles.Label(position, "Armor: " + armor + (additionalArmor != 0 ? "" + additionalArmor : ""), style);
		position.z -= 0.3f;
		Handles.Label(position, "Effects: " + Effects.Count, style);

	}

	public override bool GameUpdate() {
		animator.GameUpdate();
		if (animator.CurrentClip == EnemyAnimator.Clip.Intro) {
			if (!animator.IsDone) {
				return true;
			}
			animator.PlayMove(animationConfig.MoveAnimationSpeed * speed / Scale);
		}
		else if (animator.CurrentClip == EnemyAnimator.Clip.Outro) {
			if (animator.IsDone) {
				Recycle();
				return false;
			}
			return true;
		}
		additionalSpeed = 0f;
		additionalArmor = 0f;
		Effects.GameUpdate();
		if (Health <= 0f) {
			Vector3 position = transform.position;
			position.y += 0.5f;
			GameObject effectInstance = (GameObject) Instantiate(blastPatricals, position, transform.rotation);
			Destroy(effectInstance, 2f);
			Recycle();
			return false;
		}

		progressFactor = speed + additionalSpeed;
		progress += Time.deltaTime * (progressFactor / Vector3.Distance(positionFrom, positionTo));
		// if (direction == Direction.NorthEast || direction == Direction.NorthWest || direction == Direction.SouthEast ||
		    // direction == Direction.SouthWest) progress += Time.deltaTime * (progressFactor / Mathf.Sqrt(2));
		// else progress += Time.deltaTime * progressFactor;
		while (progress >= 1f) {
			progress = (progress - 1f) / progressFactor;
			progress = 0;
			PrepareNextState();
			if (tileTo == null) {
				Game.EnemyReachedDestination((int) Mathf.Ceil(Health));
				animator.PlayOutro();
				return true;
			}
			progress *= progressFactor;
		}
		if (progress * 3 / (progressFactor / Vector3.Distance(positionFrom, positionTo)) < 1) {
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress * 3 / (progressFactor / Vector3.Distance(positionFrom, positionTo)));
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
		transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
		return true;
	}

	public void Initialize(float scale, float speed, float pathOffset, float health, float armor) {
		Scale = scale;
		model.localScale = new Vector3(scale, scale, scale);
		this.speed = speed;
		this.armor = armor;
		Health = health;
		Effects = new GameBehaviorCollection();
		blastPatricals.transform.GetComponent<ParticleSystemRenderer>().material = material;
		animator.PlayIntro();
		// Health = 20f * scale;
	}
	
	void Awake () {
		animator.Configure(
			model.GetChild(0).gameObject.AddComponent<Animator>(),
			animationConfig
		);
	}

	public override void Recycle() {
		animator.Stop();
		OriginFactory.Reclaim(this);
	}

	public void ApplyDamage(float damage, bool isDamagePhysical) {
		Debug.Assert(damage >= 0f, "Negative damage applied.");
		float modifier = 1f;
		if (isDamagePhysical) {
			modifier = 1f - ((0.052f * (armor + additionalArmor)) / (0.9f + (0.048f * Math.Abs(armor + additionalArmor))));
		}
		Health -= damage * modifier;
	}

	public void Spawn(List<GameTile> path, List<Direction> directions) {
		this.path = path;
		this.directions = directions;
		tileFrom = path[num];
		direction = directions[num];
		tileTo = path[++num];
		progress = 0f;
		PrepareIntro();
	}

	void PrepareNextState() {
		tileFrom = tileTo;
		directionChange = direction.GetDirectionChangeTo(directions[num]);
		direction = directions[num];
		tileTo = ++num == path.Count ? null : path[num];
		positionFrom = positionTo;
		if (tileTo == null) {
			num = 0;
			PrepareOutro();
			return;
		}
		
		// transform.LookAt(tileTo.transform);
		
		
		positionTo = new Vector3(tileTo.transform.localPosition.x, transform.localPosition.y, tileTo.transform.localPosition.z);
		
		
		
		
		directionAngleFrom = directionAngleTo;
		switch (directionChange) {
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnRight45: PrepareTurnRight45(); break;
			case DirectionChange.TurnRight90: PrepareTurnRight90(); break;
			case DirectionChange.TurnRight135: PrepareTurnRight135(); break;
			case DirectionChange.TurnLeft45: PrepareTurnLeft45(); break;
			case DirectionChange.TurnLeft90: PrepareTurnLeft90(); break;
			case DirectionChange.TurnLeft135: PrepareTurnLeft135(); break;
			default: PrepareTurnAround(); break;
		}
	}

	void PrepareForward() {
		// transform.localRotation = direction.GetRotation();
		transform.LookAt(tileTo.transform);
		directionAngleTo = direction.GetAngle();
		progressFactor = speed + additionalSpeed;
	}

	void PrepareTurnRight45() {
		directionAngleTo = directionAngleFrom + 45f;
		progressFactor = (speed + additionalSpeed);
	}
	void PrepareTurnRight90() {
		directionAngleTo = directionAngleFrom + 90f;
		progressFactor = (speed + additionalSpeed);
	}
	void PrepareTurnRight135() {
		directionAngleTo = directionAngleFrom + 135f;
		progressFactor = (speed + additionalSpeed);
	}

	void PrepareTurnLeft45() {
		directionAngleTo = directionAngleFrom - 45f;
		progressFactor = (speed + additionalSpeed);
	}
	void PrepareTurnLeft90() {
		directionAngleTo = directionAngleFrom - 90f;
		progressFactor = (speed + additionalSpeed);
	}
	void PrepareTurnLeft135() {
		directionAngleTo = directionAngleFrom - 135f;
		progressFactor = (speed + additionalSpeed);
	}

	void PrepareTurnAround() {
		directionAngleTo = directionAngleFrom + 180f;
		transform.localPosition = positionFrom;
		progressFactor = (speed + additionalSpeed);
	}

	void PrepareIntro() {
		positionFrom = tileFrom.transform.localPosition;
		transform.localPosition = new Vector3(positionFrom.x, transform.localPosition.y, positionFrom.z);
		positionTo = new Vector3(tileTo.transform.localPosition.x, transform.localPosition.y, tileTo.transform.localPosition.z);
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		// transform.localRotation = direction.GetRotation();
		Quaternion rotation = Quaternion.LookRotation(positionTo);
		transform.LookAt(tileTo.transform);
		progressFactor = (speed + additionalSpeed);
	}

	void PrepareOutro() {
		positionTo = tileFrom.transform.localPosition;
		directionChange = DirectionChange.None;
		directionAngleTo = direction.GetAngle();
		transform.localRotation = direction.GetRotation();
		progressFactor = (speed + additionalSpeed);
	}
}