using UnityEngine;

[ExecuteInEditMode]
public class BrainDrawer : MonoBehaviour {
    public DNA dna;
    public float scale = 1f;

    void OnDrawGizmos() {
        if (dna == null || dna.genes == null || dna.genes.Length == 0)
            return;

        Vector3 startPos = transform.position;
        Vector3 prevPos = startPos;

        Gizmos.color = Color.red;

        foreach (float gene in dna.genes) {
            Vector3 direction = Quaternion.Euler(0, 0, gene * 360f) * Vector3.up * scale;
            Vector3 newPos = prevPos + direction;
            Gizmos.DrawLine(prevPos, newPos);
            prevPos = newPos;
        }
    }
}
