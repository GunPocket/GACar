using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CarController : MonoBehaviour {
    public DNA Brain;
    public float mass = 1500f;
    public float dragCoefficient = 0.3f;
    public float rollingResistanceCoefficient = 0.02f;
    public float frontalArea = 2.5f;
    public float wheelBase = 2.5f;
    public float maxSteeringAngle = 30f;
    public float maxAcceleration = 10f;
    public float maxBrakeForce = 2f;
    public int MaxTargets = 5;

    public bool IsElite { get; set; }

    private Rigidbody2D rb;
    private List<Vector3> targetSequence;
    private int currentTargetIndex = 0;

    private void Start() {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.mass = mass / 1000;
        rb.drag = dragCoefficient;
        rb.angularDrag = 0.5f;
    }

    public void SetPopulationManager(PopulationManager pm) {
        targetSequence = pm.GetTargetPositions();
    }

    public void SetBrain(DNA brain) {
        Brain = brain;
    }

    private void FixedUpdate() {
        if (targetSequence == null || currentTargetIndex >= targetSequence.Count) return;

        Vector2 targetPosition = targetSequence[currentTargetIndex];
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        float angleToTarget = Vector2.SignedAngle(transform.up, targetPosition - (Vector2)transform.position);

        float[] inputs = {
            transform.eulerAngles.z,
            rb.velocity.magnitude,
            distanceToTarget,
            angleToTarget
        };

        if (Brain != null && Brain.NeuralNetwork != null) {
            float[] outputs = Brain.NeuralNetwork.FeedForward(inputs);
            ApplyOutputs(outputs);
        }

    }

    private void ApplyOutputs(float[] outputs) {

        float acceleration = Mathf.Clamp(outputs[0], 0f, 1f);
        float brake = Mathf.Clamp(outputs[1], 0f, 1f);
        float steering = Mathf.Clamp(outputs[2] * 2f - 1f, -1f, 1f);

        Vector2 forward = transform.up;

        float motorForce = acceleration * maxAcceleration;
        float brakeForce = brake * maxBrakeForce;

        rb.AddForce(forward * motorForce);

        if (acceleration == 0 && brake > 0) {
            rb.AddForce(-rb.velocity.normalized * brakeForce);
        }

        float maxSteeringAngleScaled = maxSteeringAngle * (rb.velocity.magnitude / maxAcceleration);

        if (rb.velocity.magnitude > 0) {
            float steeringAngle = Mathf.Clamp(steering, -1f, 1f) * maxSteeringAngleScaled;
            rb.MoveRotation(rb.rotation + steeringAngle);
        }
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Target")) {
            if (currentTargetIndex < targetSequence.Count && other.transform.position == targetSequence[currentTargetIndex]) {
                currentTargetIndex = (currentTargetIndex + 1) % MaxTargets;
                float timeToTarget = Time.time;

                float timeReward = 100f / (timeToTarget + 1f);
                Brain.Fitness += 10 + timeReward;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Obstacle")) {
            Brain.Fitness -= 1000;
        }
    }

    public void ResetCar() {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        currentTargetIndex = 0;
    }

    private void OnDrawGizmos() {
        if (targetSequence == null) return;
        if (targetSequence.Count > 0) {
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            Gizmos.DrawLine(transform.position, targetSequence[currentTargetIndex]);
        }
    }
}
