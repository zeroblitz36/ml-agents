using MLAgents;
using System.Collections.Generic;
using UnityEngine;

public class RollerAgent : Agent
{
    Rigidbody rBody;
    public TankArena tankArena;
    public GameObject target;
    private Rigidbody targetRigidBody;
    public float speed;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rBody = GetComponent<Rigidbody>();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);

        //Target fell off
        if (target.transform.position.y + 1 < tankArena.transform.position.y)
        {
            SetReward(1);
            Done();
            return;
        }

        //Agent fell off platform
        if (transform.position.y - 0.45f < tankArena.transform.position.y)
        {
            SetReward(-1);
            Done();
            return;
        }

        //AddReward(-1f / agentParameters.maxStep);
    }

    public override void AgentReset()
    { 
        if (!targetRigidBody)
        {
            targetRigidBody = target.GetComponent<Rigidbody>();
        }

        if (transform.position.y < tankArena.transform.position.y)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(0, tankArena.transform.position.y + 0.5f, 0);
        }

        Vector3 targetPosition;
        float distanceToTarget;
        do
        {
            Vector2 r = Random.insideUnitCircle * 4.5f;
            targetPosition = new Vector3(
                r.x,
                tankArena.transform.position.y + 0.5f,
                r.y
            );
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        } while (distanceToTarget < 2f);

        target.transform.position = targetPosition;
        targetRigidBody.velocity = Vector3.zero;
        targetRigidBody.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations()
    {
        AddVectorObs(transform.position.x);
        AddVectorObs(transform.position.z);

        Vector3 relativePosition = target.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        AddVectorObs(transform.position.x);
        AddVectorObs(transform.position.z);

        float distanceFromCenter = Vector3.Distance(
                transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(distanceFromCenter);

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        AddVectorObs(distanceToTarget);

        float targetDistanceFromCenter = Vector3.Distance(
                target.transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);
        /*
        Vector3 localVelocity = transform.InverseTransformDirection(rBody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.z);
        */
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        AddVectorObs(targetRigidBody.velocity.x);
        AddVectorObs(targetRigidBody.velocity.z);

        /*
        float rayDistance = 10f;
        int n = 36;
        float[] rayAngles = new float[n];
        for(int i = 0; i < n; i++)
        {
            rayAngles[i] = i * (360 / n);
        }
        List<float> rays = Perceive(rayDistance, rayAngles, 1<<16);
        AddVectorObs(rays);
        */
    }

    List<float> perceptionBuffer = new List<float>();
    Vector3 endPosition;
    RaycastHit hit;

    public List<float> Perceive(float rayDistance,
            float[] rayAngles, int layerMask)
    {
        perceptionBuffer.Clear();
        // For each ray sublist stores categorial information on detected object
        // along with object distance.
        foreach (float angle in rayAngles)
        {
            Vector3 v = new Vector3(
                Mathf.Cos(DegreeToRadian(angle)),
                0,
                Mathf.Sin(DegreeToRadian(angle))
            );
            Debug.Log("angle = " + angle + " v = " + v);
            endPosition = transform.position + v * rayDistance;
            
            if(Physics.Linecast(transform.position, endPosition, out hit, layerMask))
            {
                perceptionBuffer.Add(hit.distance / rayDistance);
            }
            else
            {
                perceptionBuffer.Add(1);
            }

            if (Application.isEditor)
            {
                Debug.DrawRay(transform.position,
                    Vector3.Lerp(Vector3.zero,v * rayDistance, perceptionBuffer[perceptionBuffer.Count-1]),
                    Color.black, 
                    0.01f, true);
            }
        }

        return perceptionBuffer;
    }

    public static Vector3 PolarToCartesian(float radius, float angle)
    {
        float x = radius * Mathf.Cos(DegreeToRadian(angle));
        float z = radius * Mathf.Sin(DegreeToRadian(angle));
        return new Vector3(x, 0f, z);
    }

    public static float DegreeToRadian(float degree)
    {
        return degree * Mathf.PI / 180f;
    }
}
