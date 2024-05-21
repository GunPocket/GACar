using UnityEngine;

public class CarController : MonoBehaviour {
    public float[] actions;
    public float speed = 10f;
    private int actionIndex = 0;
    private float turnSpeed = 200f; // Velocidade de rotação
    private PopulationManager populationManager;
    private Rigidbody2D rb;
    public GameObject target;

    private Vector3 initialPosition;
    public float distanceTraveled; // Variável para armazenar a distância percorrida
    public int targetsReached = 0; // Número de alvos atingidos

    void Start() {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        if (actionIndex < actions?.Length && target != null) {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.rotation = Mathf.MoveTowardsAngle(rb.rotation, angle, turnSpeed * Time.fixedDeltaTime);
            rb.velocity = transform.up * speed;
            actionIndex++;
        }
        UpdateDistanceTraveled();
    }

    void UpdateDistanceTraveled() {
        distanceTraveled = Vector3.Distance(initialPosition, transform.position);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Target")) {
            targetsReached++;
            if (targetsReached >= populationManager.maxTargets) {
                populationManager.CarDestroyed(this.gameObject);
                populationManager.MoveTargets();
            } else {
                target.SetActive(false);
            }
        }
    }

    public float GetDistanceTraveled() {
        return distanceTraveled;
    }

    public void SetPopulationManager(PopulationManager manager) {
        populationManager = manager;
        target = manager.targets[targetsReached];
    }
}
