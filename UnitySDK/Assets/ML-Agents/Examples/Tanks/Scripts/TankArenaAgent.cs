using MLAgents;
using UnityEngine;

public class TankArenaAgent : Agent
{
    public int m_TankId;
    public float m_Speed;
    public float m_TurnSpeed;

    private Rigidbody rBody = null;
    private RayPerception rayPer = null;
    public TankArena tankArena;
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rBody = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float forwardMovement = vectorAction[0];
        float turnValue = vectorAction[1];
        if (forwardMovement < 0)
        {
            turnValue *= -1;
        }
        forwardMovement = Mathf.Clamp(forwardMovement, -1, 1);
        turnValue = Mathf.Clamp(turnValue, -1, 1);

        Vector3 vel = transform.forward * forwardMovement * m_Speed;
        //Debug.Log("Vel = " + vel);
        rBody.AddForce(vel, ForceMode.VelocityChange);

        if (rBody.velocity.sqrMagnitude > 25f) // slow it down
        {
            rBody.velocity *= 0.95f;
        }

        float turn = turnValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rBody.MoveRotation(rBody.rotation * turnRotation);

        AddReward(-1f / agentParameters.maxStep);
        if(GetCumulativeReward() < -1)
        {
            SetReward(-1);
            Done();
            return;
        }
        if(transform.position.y < tankArena.transform.position.y - 0.5f)
        {
            SetReward(-1);
            Done();
            return;
        }

        if(tankArena.GetCurrentPointSphere().transform.position.y < tankArena.transform.position.y)
        {
            AddReward(2);
            Done();
            return;
        }
    }

    public override void AgentReset()
    {
        Vector3 pos = new Vector3();
        pos.x = Random.Range(-20, 20);
        pos.y = tankArena.transform.position.y;
        pos.z = Random.Range(-20, 20);
        transform.position = pos;
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        GameObject spherePoint = tankArena.GetCurrentPointSphere();
        pos.x = Random.Range(-20, 20);
        pos.y = tankArena.transform.position.y + 1;
        pos.z = Random.Range(-20, 20);
        spherePoint.transform.position = pos;
        spherePoint.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public override void CollectObservations()
    {
        /*
        //position
        AddVectorObs(transform.position.x);
        AddVectorObs(transform.position.z);
        //distance from center 
        AddVectorObs(Vector3.Distance(transform.position, tankArena.transform.position));
        //velocity
        Vector3 vel = transform.InverseTransformDirection(rBody.velocity);
        AddVectorObs(vel.x);
        AddVectorObs(vel.z);


        GameObject pointSpere = tankArena.GetCurrentPointSphere();
        Vector3 relativeVector = pointSpere.transform.position - transform.position;
        //sphere position
        AddVectorObs(relativeVector.x);
        AddVectorObs(relativeVector.z);
        //distance to sphere
        AddVectorObs(Vector3.Distance(transform.position, pointSpere.transform.position));
        Rigidbody r = pointSpere.GetComponent<Rigidbody>();
        vel = pointSpere.transform.InverseTransformDirection(r.velocity);
        //sphere velocity
        AddVectorObs(vel.x);
        AddVectorObs(vel.z);
        */

        float rayDistance = 50f;
        float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };
        string[] detectableObjects = { "goal"};
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 1f, 1f));
        Vector3 localVelocity = transform.InverseTransformDirection(rBody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.z);
    }
}
