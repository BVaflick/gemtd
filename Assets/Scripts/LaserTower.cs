using UnityEditor;
using UnityEngine;

public class LaserTower : Tower {

    // [SerializeField, Range(1f, 100f)]
    // float damage = 10f;

    // [SerializeField]
    // // Transform turret = default, laserBeam = default;

    // TargetPoint target;

    // Vector3 laserBeamScale;

    // // public override TowerType TowerType => TowerType.Topaz1;

    // void Awake() {
    //     laserBeamScale = laserBeam.localScale;
    // }

    // public override void GameUpdate() {
    //     if (TrackTarget(ref target) || AcquireTarget(out target)) {
    //         Shoot();
    //     } else {
    //         laserBeam.localScale = Vector3.zero;
    //         Vector3 point = this.transform.position;
    //         turret.LookAt(point);
    //     }
    // }

    // void Shoot() {
    //     Vector3 point = target.Position;
    //     turret.LookAt(point);
    //     float d = Vector3.Distance(turret.position, point);
    //     laserBeam.localRotation = turret.localRotation;
    //     laserBeamScale.z = d;
    //     laserBeam.localScale = laserBeamScale;
    //     laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;
    //     target.Enemy.ApplyDamage(damage * Time.deltaTime);
    // }
}