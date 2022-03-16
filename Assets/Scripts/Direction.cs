using UnityEngine;

public enum Direction {
	North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
}

public enum DirectionChange {
	None, TurnRight45, TurnRight90, TurnRight135, TurnLeft45, TurnLeft90, TurnLeft135, TurnAround
}

public static class DirectionExtensions {
	
	private static Quaternion[] rotations = {
		Quaternion.Euler(0f, 0f, 0f),
		Quaternion.Euler(0f, 45f, 0f),
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 135f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 225f, 0f),
		Quaternion.Euler(0f, 270f, 0f),
		Quaternion.Euler(0f, 315f, 0f)
	};

	public static float GetAngle (this Direction direction) {
		return (float)direction * 45f;
	}
	public static DirectionChange GetDirectionChangeTo (
		this Direction current, Direction next
	) {
		if (current == next) {
			return DirectionChange.None;
		}
		else if (current + 2 == next || current - 6 == next) {
			return DirectionChange.TurnRight90;
		}
		else if (current - 2 == next || current + 6 == next) {
			return DirectionChange.TurnLeft90;
		}
		else if (current + 1 == next || current - 7 == next) {
			return DirectionChange.TurnRight45;
		}
		else if (current - 1 == next || current + 7 == next) {
			return DirectionChange.TurnLeft45;
		}
		else if (current + 3 == next || current - 5 == next) {
			return DirectionChange.TurnRight135;
		}
		else if (current - 3 == next || current + 5 == next) {
			return DirectionChange.TurnLeft135;
		}
		return DirectionChange.TurnAround;
	}

	public static Quaternion GetRotation (this Direction direction) {
		return rotations[(int)direction];
	}
}