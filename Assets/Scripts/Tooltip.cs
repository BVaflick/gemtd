using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public string tip;
	public float range;
	public Vector3 rangeCirclePos;
	private float timeToWait = 0.2f;
	
	public void OnPointerEnter(PointerEventData eventData) {
		StopAllCoroutines();
		StartCoroutine(StartTimer());
	}

	public void OnPointerExit(PointerEventData eventData) {
		StopAllCoroutines();
		TooltipManager.OnMouseDehover();
	}

	private void showMessage() {
		TooltipManager.OnMouseHover(tip, range, rangeCirclePos, Input.mousePosition);
	}

	private IEnumerator StartTimer() {
		yield return new WaitForSeconds(timeToWait);
		showMessage();
	}
}