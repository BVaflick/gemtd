﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class SpeedUp : TowerBuff {

   public override void Modify(Tower tower) {
      tower.AttackSpeed += 3f;
   }
}