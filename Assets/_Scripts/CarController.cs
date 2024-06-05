using System.Collections.Generic;
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

    private Rigidbody2D rb;
    private List<Vector3> targetSequence;
    private int currentTargetIndex = 0;
    private readonly float[] rayDistances = new float[8];
    private readonly float rayLength = 5f;
    private readonly float longerRayLength = 10f;

    public void SetPopulationManager(PopulationManager pm) {
        targetSequence = pm.GetTargetPositions();
        rb = GetComponent<Rigidbody2D>();
        rb.mass = mass / 1000;
        rb.drag = dragCoefficient;
        rb.angularDrag = 0.5f;
    }

    private void FixedUpdate() {
        if (targetSequence == null || currentTargetIndex >= targetSequence.Count) return;

        Vector2 targetPosition = targetSequence[currentTargetIndex];
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        float angleToTarget = Vector2.SignedAngle(transform.up, targetPosition - (Vector2)transform.position);

        UpdateRaycastDistances();

        float[] inputs = {
            transform.position.x,
            transform.position.y,
            transform.eulerAngles.z,
            rb.velocity.magnitude,
            distanceToTarget,
            angleToTarget
        };

        inputs = CombineInputs(inputs, rayDistances);

        float[] outputs = Brain.NeuralNetwork.TrainGPU(inputs);
        ApplyOutputs(outputs);
    }

    private void UpdateRaycastDistances() {
        Vector2[] directions = {
            Vector2.up,
            (Vector2.up + Vector2.right).normalized,
            Vector2.right,
            (Vector2.down + Vector2.right).normalized,
            Vector2.down,
            (Vector2.down + Vector2.left).normalized,
            Vector2.left,
            (Vector2.up + Vector2.left).normalized
        };

        for (int i = 0; i < directions.Length; i++) {
            float length = (i % 2 == 0) ? longerRayLength : rayLength;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], length);
            rayDistances[i] = hit.collider ? hit.distance : length;
        }
    }

    private float[] CombineInputs(float[] baseInputs, float[] additionalInputs) {
        float[] combined = new float[baseInputs.Length + additionalInputs.Length];
        baseInputs.CopyTo(combined, 0);
        additionalInputs.CopyTo(combined, baseInputs.Length);
        return combined;
    }

    private void ApplyOutputs(float[] outputs) {
        float acceleration = outputs[0];
        float brake = outputs[1];
        float steering = outputs[2];

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

                float reward = 100f / (timeToTarget + 1f);
                Brain.Fitness += Mathf.Clamp(reward, 0.1f, 10f);
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