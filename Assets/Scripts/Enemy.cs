using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class Enemy : GameBehavior {

	[SerializeField]
	Transform model = default;

	EnemyFactory originFactory;

	GameTile tileFrom, tileTo;
	Vector3 positionFrom, positionTo;
	Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;
	float progress, progressFactor;
	float pathOffset;
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
		additionalSpeed = 0f;
		additionalArmor = 0f;
		Effects.GameUpdate();
		if (Health <= 0f) {
			Recycle();
			return false;
		}

		if (direction == Direction.NorthEast || direction == Direction.NorthWest || direction == Direction.SouthEast ||
		    direction == Direction.SouthWest) progress += Time.deltaTime * (progressFactor / Mathf.Sqrt(2));
		else progress += Time.deltaTime * progressFactor;
		while (progress >= 1f) {
			if (tileTo == null) {
				Game.EnemyReachedDestination((int) Mathf.Ceil(Health));
				Recycle();
				return false;
			}
			progress = (progress - 1f) / progressFactor;
			PrepareNextState();
			progress *= progressFactor;
		}
		if (directionChange != DirectionChange.None && progress * 3 < 1) {
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress * 3);
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
		// Health = 20f * scale;
	}

	public override void Recycle() {
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

	public void SpawnOn(GameTile tile) {
		tileFrom = tile;
		tileTo = tile.NextTileOnPath(num);
		progress = 0f;
		PrepareIntro();
	}

	void PrepareNextState() {
		tileFrom = tileTo;
		tileTo = tileTo.NextTileOnPath(num);
		positionFrom = positionTo;
		if (tileTo == null) {
			if (num >= 5) {
				PrepareOutro();
				return;
			} else {
				num += 1;
				tileTo = tileFrom.NextTileOnPath(num);
			}
		}
		positionTo = tileTo.transform.localPosition;
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection[num]);
		direction = tileFrom.PathDirection[num];
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
		transform.localRotation = direction.GetRotation();
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
		positionTo = tileTo.transform.localPosition;
		direction = tileFrom.PathDirection[num];
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		transform.localRotation = direction.GetRotation();
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