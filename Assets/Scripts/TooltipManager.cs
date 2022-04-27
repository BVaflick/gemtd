using System;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour {
	public Text tooltipText;
	public RectTransform tooltip;
	public Transform rangeCircle;

	public static Action<string, float, Vector3, Vector2> OnMouseHover;
	public static Action OnMouseDehover;

	private void OnEnable() {
		OnMouseHover += showTip;
		OnMouseDehover += hideTip;
	}

	private void OnDisable() {
		OnMouseHover -= showTip;
		OnMouseDehover -= hideTip;
	}

	private void Start() {
		hideTip();
	}

	private void showTip(string tip, float range, Vector3 rangeCirclePosition, Vector2 mousePosition) {
		tooltipText.text = tip;
		tooltip.sizeDelta = new Vector2(tooltipText.preferredWidth > 200 ? 200 : tooltipText.preferredWidth, tooltipText.preferredHeight);
		tooltip.transform.position = new Vector2(mousePosition.x + tooltip.sizeDelta.x / 2, mousePosition.y);
		tooltip.gameObject.SetActive(true);
		if (range > 0) {
			rangeCircle.transform.localScale = new Vector3(0.2f * range, 0.2f * range, 0.2f * range);
			rangeCircle.transform.position = new Vector3(rangeCirclePosition.x, 0.02f, rangeCirclePosition.z);
			rangeCircle.gameObject.SetActive(true);
		}
	}
	
	private void hideTip() {
		tooltipText.text = default;
		tooltip.gameObject.SetActive(false);
		rangeCircle.gameObject.SetActive(false);
	}
}