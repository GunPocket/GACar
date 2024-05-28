using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetOrderDrawer : MonoBehaviour {
    private List<GameObject> targets = new List<GameObject>();
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform targetParent;

    public void UpdateTargets(List<GameObject> newTargets) {
        ClearTargets();
        targets = new List<GameObject>(newTargets);
        DrawTargets();
    }

    private void ClearTargets() {
        foreach (Transform child in targetParent) {
            Destroy(child.gameObject);
        }
    }

    private void DrawTargets() {
        for (int i = 0; i < targets.Count; i++) {
            GameObject newTarget = Instantiate(targetPrefab, targets[i].transform.position, Quaternion.identity, targetParent);
            TMP_Text targetNumberText = newTarget.GetComponentInChildren<TMP_Text>();
            targetNumberText.text = (i + 1).ToString();
        }
    }
}
