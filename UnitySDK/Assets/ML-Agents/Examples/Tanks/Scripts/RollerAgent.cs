using MLAgents;
using UnityEngine;

public class RollerAgent : Agent
{
    Rigidbody rBody;
    public TankArena tankArena;
    public Transform target;
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

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if(distanceToTarget < 1.5f)
        {
            SetReward(1);
            Done();
            return;
        }

        // Fell off platform
        if (transform.position.y < tankArena.transform.position.y)
        {
            SetReward(-1);
            Done();
            return;
        }
    }

    public override void AgentReset()
    {
        if(transform.position.y < tankArena.transform.position.y)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(0, tankArena.transform.position.y + 0.5f, 0);
        }

        Vector3 targetPosition;
        float distanceToTarget;
        do
        {
            targetPosition = new Vector3(
                Random.Range(-4, 4),
                tankArena.transform.position.y + 0.5f,
                Random.Range(-4, 4)
            );
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        } while (distanceToTarget < 2f);

        target.position = targetPosition;
    }

    public override void CollectObservations()
    {
        Vector3 relativePosition = target.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        float distanceFromCenter = Vector3.Distance(
                transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(distanceFromCenter);

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        AddVectorObs(distanceToTarget);

        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);
    }
}
