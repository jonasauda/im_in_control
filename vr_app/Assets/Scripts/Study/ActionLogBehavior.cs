using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLogBehavior : MonoBehaviour
{
    public static long STOP_ACTION_TIME_THRESHOLD = 1000;

    private MovementDetector movementDetector;
    private RotationDetector rotationDetector;

    private EventLogger eventLogger;

    private string hrri;

    private enum State { INACTIVE, ACTIVE }
    private State state = State.INACTIVE;

    private long? startActionTimestamp;

    void Start()
    {
        eventLogger = GameObject.FindGameObjectWithTag("GameManager").GetComponent<EventLogger>();
        hrri = Utils.GetHrri(gameObject);

        movementDetector = new MovementDetector();
        movementDetector.Start(transform);
        rotationDetector = new RotationDetector();
        rotationDetector.Start(transform);
    }

    void Update()
    {
        long now = Utils.GetTime();

        movementDetector.Update(transform);
        rotationDetector.Update(transform);

        if (state == State.ACTIVE && (!movementDetector.IsActive() && !rotationDetector.IsActive()))
        {
            long endActionTimestamp = now - STOP_ACTION_TIME_THRESHOLD;
            string eventValue = hrri + " " + startActionTimestamp + " " + endActionTimestamp;
            eventLogger.AddLogEvent(EventLogger.LogEvent.MovementAction, eventValue);

            startActionTimestamp = null;
            state = State.INACTIVE;
        }

        if (state == State.INACTIVE && (movementDetector.IsActive() || rotationDetector.IsActive()))
        {
            startActionTimestamp = now;
            state = State.ACTIVE;
        }
    }

    private class MovementDetector
    {
        private static float START_MOVEMENT_DISTANCE_THRESHOLD = 0.05f;
        private static float STOP_MOVEMENT_DISTANCE_THRESHOLD = 0.01f;

        private Vector3 lastRestingPosition;
        private Vector3 lastPosition;
        private long? lastMovingTimestamp;

        private bool active = false;

        public void Start(Transform transform)
        {
            lastRestingPosition = transform.position;
        }

        public void Update(Transform transform)
        {
            long now = Utils.GetTime();

            if (lastMovingTimestamp.HasValue)
            {
                // object is moving

                if (now - lastMovingTimestamp.Value > STOP_ACTION_TIME_THRESHOLD)
                {
                    // movement complete/stopped

                    active = false;

                    lastMovingTimestamp = null;
                    lastPosition = Vector3.zero;
                    lastRestingPosition = transform.position;
                }
                else if ((lastPosition - transform.position).magnitude > STOP_MOVEMENT_DISTANCE_THRESHOLD)
                {
                    // still moving
                    lastMovingTimestamp = now;
                    lastPosition = transform.position;
                }

            }
            else
            {
                // object is not moving
                if ((transform.position - lastRestingPosition).magnitude > START_MOVEMENT_DISTANCE_THRESHOLD)
                {
                    // TODO: if hand is in vicinity

                    active = true;

                    lastMovingTimestamp = now;
                    lastPosition = transform.position;
                }
            }
        }

        public bool IsActive()
        {
            return active;
        }
    }

    private class RotationDetector
    {
        private static float START_ROTATION_DEGREE_THRESHOLD = 10f;
        private static float STOP_ROTATION_DEGREE_THRESHOLD = 5f;

        private float lastRestingRotation;
        private float lastRotation;
        private long? lastRotatingTimestamp;

        private bool active = false;

        public void Start(Transform transform)
        {
            float currentRotatation = GetCurrentRotation(transform);
            lastRestingRotation = currentRotatation;
        }

        public void Update(Transform transform)
        {
            long now = Utils.GetTime();

            float currentRotatation = GetCurrentRotation(transform);

            if (lastRotatingTimestamp.HasValue)
            {
                // object is moving
                if (now - lastRotatingTimestamp.Value > STOP_ACTION_TIME_THRESHOLD)
                {
                    // movement complete/stopped

                    active = false;

                    lastRotatingTimestamp = null;
                    lastRotation = 0f;
                    lastRestingRotation = currentRotatation;
                }
                else if (Mathf.Abs(lastRotation - currentRotatation) > START_ROTATION_DEGREE_THRESHOLD)
                {
                    // still moving
                    lastRotatingTimestamp = now;
                    lastRotation = currentRotatation;
                }

            }
            else
            {
                // object is not moving
                if (Mathf.Abs(currentRotatation - lastRestingRotation) > START_ROTATION_DEGREE_THRESHOLD)
                {
                    // TODO: if hand is in vicinity

                    active = true;

                    lastRotatingTimestamp = now;
                    lastRotation = currentRotatation;
                }
            }
        }

        public bool IsActive()
        {
            return active;
        }

        private static float GetCurrentRotation(Transform transform)
        {
            float currentRotatation = transform.rotation.eulerAngles.y;
            if (currentRotatation > 180) currentRotatation -= 360f;
            return currentRotatation;
        }
    }
}
