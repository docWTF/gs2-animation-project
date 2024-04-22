using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Requires Rigidbody to move around
[RequireComponent(typeof(Animation))] // Requires Animation component for animations

public class RandomFlyer : MonoBehaviour
{
    [SerializeField] float idleSpeed, turnSpeed, switchSeconds, idleRatio;
    [SerializeField] Vector2 moveSpeedMinMax, changeAnimEveryFromTo, changeTargetEveryFromTo;
    [SerializeField] Transform homeTarget, flyingTarget;
    [SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 yMinMax;
    [SerializeField] public bool returnToBase = false;
    [SerializeField] public float randomBaseOffset = 5, delayStart = 0f;

    private Animation animator;
    private Rigidbody body;
    [System.NonSerialized]
    public float changeTarget = 0f, changeAnim = 0f, timeSinceTarget = 0f, timeSinceAnim = 0f, prevAnim, currentAnim = 0f, prevSpeed, speed, zturn, prevz,
        turnSpeedBackup;
    private Vector3 rotateTarget, position, direction, velocity, randomizedBase;
    private Quaternion lookRotation;
    [System.NonSerialized] public float distanceFromBase, distanceFromTarget;


    void Start()
    {
        // Initialize
        animator = GetComponent<Animation>();
        body = GetComponent<Rigidbody>();
        turnSpeedBackup = turnSpeed;
        direction = transform.forward;
        if (delayStart < 0f) body.velocity = idleSpeed * direction;
    }

    void FixedUpdate()
    {
        // Wait if start should be delayed (useful to add small differences in large flocks)
        if (delayStart > 0f)
        {
            delayStart -= Time.fixedDeltaTime;
            return;
        }

        // Calculate distances
        distanceFromBase = Vector3.Magnitude(randomizedBase - body.position);
        distanceFromTarget = Vector3.Magnitude(flyingTarget.position - body.position);

        // Allow drastic turns close to base to ensure target can be reached
        if (returnToBase && distanceFromBase < 10f)
        {
            if (turnSpeed != 300f && body.velocity.magnitude != 0f)
            {
                turnSpeedBackup = turnSpeed;
                turnSpeed = 300f;
            }
            else if (distanceFromBase <= 2f)
            {
                body.velocity = Vector3.zero;
                turnSpeed = turnSpeedBackup;
                return;
            }
        }

        // Time for a new animation speed
        if (changeAnim < 0f)
        {
            prevAnim = currentAnim;
            currentAnim = ChangeAnim(currentAnim);
            changeAnim = Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
            timeSinceAnim = 0f;
            prevSpeed = speed;
            if (currentAnim == 0) speed = idleSpeed;
            else speed = Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y, (currentAnim - changeAnimEveryFromTo.x) / (changeAnimEveryFromTo.y - changeAnimEveryFromTo.x));
        }

        // Time for a new target position
        if (changeTarget < 0f)
        {
            rotateTarget = ChangeDirection(body.transform.position);
            if (returnToBase) changeTarget = 0.2f; else changeTarget = Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
            timeSinceTarget = 0f;
        }

        // Turn when approaching height limits
        if (body.transform.position.y < yMinMax.x + 10f ||
            body.transform.position.y > yMinMax.y - 10f)
        {
            if (body.transform.position.y < yMinMax.x + 10f) rotateTarget.y = 1f; else rotateTarget.y = -1f;
        }

        zturn = Mathf.Clamp(Vector3.SignedAngle(rotateTarget, direction, Vector3.up), -45f, 45f);

        // Update times
        changeAnim -= Time.fixedDeltaTime;
        changeTarget -= Time.fixedDeltaTime;
        timeSinceTarget += Time.fixedDeltaTime;
        timeSinceAnim += Time.fixedDeltaTime;

        // Rotate towards target
        if (rotateTarget != Vector3.zero) lookRotation = Quaternion.LookRotation(rotateTarget, Vector3.up);
        Vector3 rotation = Quaternion.RotateTowards(body.transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        body.transform.eulerAngles = rotation;

        // Rotate on z-axis to tilt body towards turn direction
        float temp = prevz;
        if (prevz < zturn) prevz += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zturn - prevz);
        else if (prevz >= zturn) prevz -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevz - zturn);
        prevz = Mathf.Clamp(prevz, -45f, 45f);
        body.transform.Rotate(0f, 0f, prevz - temp, Space.Self);

        // Move flyer
        direction = transform.forward;
        if (returnToBase && distanceFromBase < idleSpeed)
        {
            body.velocity = Mathf.Min(idleSpeed, distanceFromBase) * direction;
        }
        else body.velocity = Mathf.Lerp(prevSpeed, speed, Mathf.Clamp(timeSinceAnim / switchSeconds, 0f, 1f)) * direction;

        // Hard-limit the height
        if (body.transform.position.y < yMinMax.x || body.transform.position.y > yMinMax.y)
        {
            position = body.transform.position;
            position.y = Mathf.Clamp(position.y, yMinMax.x, yMinMax.y);
            body.transform.position = position;
        }
    }

    // Select a new animation speed randomly
    private float ChangeAnim(float currentAnim)
    {
        float newState;
        if (Random.Range(0f, 1f) < idleRatio) newState = 0f;
        else
        {
            newState = Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
        }
        if (newState != currentAnim)
        {
            // Set animation speed
            if (newState == 0) animator.Stop(); else animator.Play();
        }
        return newState;
    }

    // Select a new direction to fly in randomly
    private Vector3 ChangeDirection(Vector3 currentPosition)
    {
        Vector3 newDir;
        if (returnToBase)
        {
            randomizedBase = homeTarget.position;
            randomizedBase.y += Random.Range(-randomBaseOffset, randomBaseOffset);
            newDir = randomizedBase - currentPosition;
        }
        else if (distanceFromTarget > radiusMinMax.y)
        {
            newDir = flyingTarget.position - currentPosition;
        }
        else if (distanceFromTarget < radiusMinMax.x)
        {
            newDir = currentPosition - flyingTarget.position;
        }
        else
        {
            // 360-degree freedom of choice on the horizontal plane
            float angleXZ = Random.Range(-Mathf.PI, Mathf.PI);
            // Limited max steepness of ascent/descent in the vertical direction
            float angleY = Random.Range(-Mathf.PI / 48f, Mathf.PI / 48f);
            // Calculate direction
            newDir = Mathf.Sin(angleXZ) * Vector3.forward + Mathf.Cos(angleXZ) * Vector3.right + Mathf.Sin(angleY) * Vector3.up;
        }
        return newDir.normalized;
    }
}
