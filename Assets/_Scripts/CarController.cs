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

    private float startTime;

    private Rigidbody2D rb;
    private List<Vector3> targetSequence;
    private int currentTargetIndex = 0;
    private float[] rayDistances = new float[8];
    private float rayLength = 5f;
    private float longerRayLength = 10f;

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

        float[] outputs = Brain.NeuralNetwork.FeedForward(inputs);
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
                Brain.Fitness += 20f;
                currentTargetIndex = (currentTargetIndex + 1) % 5;

                float timeToTarget = Time.time - startTime;

                float reward = 100f / (timeToTarget + 1f);
                Brain.Fitness += Mathf.Clamp(reward, 0.1f, 10f);
            }
        }
    }

    public void ResetCar() {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        currentTargetIndex = 0;
    }

    private void OnDrawGizmos() {
        // direção do alvo
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetSequence[currentTargetIndex]);

        // vetores de velocidade
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rb.velocity);

        /*
        // direção do carro
        Vector2 forward = transform.up;
        Vector2 right = transform.right;
        Vector2 leftWheelDirection = Quaternion.Euler(0, 0, maxSteeringAngle) * forward;
        Vector2 rightWheelDirection = Quaternion.Euler(0, 0, -maxSteeringAngle) * forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftWheelDirection * rayLength);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightWheelDirection * rayLength);

        
        // raycasts
        Gizmos.color = Color.green;
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
            Vector2 direction = directions[i];
            float length = (i % 2 == 0) ? longerRayLength : rayLength;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, length);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * (hit.collider ? hit.distance : length));
        }*/
    }

}
