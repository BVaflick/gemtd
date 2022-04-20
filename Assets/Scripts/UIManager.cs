using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
	[SerializeField]
	private RectTransform panel;

	[SerializeField]
	private RectTransform statusEffectsPanel;
	
	[SerializeField]
	private RectTransform parametersPanel;

	[SerializeField]
	private RectTransform firstParamLabel;
	[SerializeField]
	private RectTransform firstParamValue;
	[SerializeField]
	private RectTransform secondParamLabel;
	[SerializeField]
	private RectTransform secondParamValue;
	[SerializeField]
	private RectTransform thirdParamLabel;
	[SerializeField]
	private RectTransform thirdParamValue;
		
	[SerializeField]
	private RectTransform abilities;
	
	[SerializeField]
	private RawImage abilityImage;
	[SerializeField]
	private Image abilityButton;
	[SerializeField]
	private Image statusEffectImage;

	/*
	 * За неимением сериализуемых словарей, запоминаем порядок спрайтов:
	 * 1. Броня
	 * 2. Урон
	 * 3. Скорость
	 * 4. Радиус 
	 */
	[SerializeField]
	List<Sprite> paramsSprites;

	public void showPlayerAbilities(PlayerAbility[] playerAbilities) {
		foreach (PlayerAbility ability in playerAbilities) {
			Button abilityButton = new GameObject().AddComponent<Button>();
			
			// abilityButton.onClick = ability.action;
		}
	}
}