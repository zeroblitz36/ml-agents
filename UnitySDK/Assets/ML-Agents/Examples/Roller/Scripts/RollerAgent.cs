using MLAgents;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RollerAgent : Agent
{
    Rigidbody rBody;
    public TankArena tankArena;
    public GameObject target1;
    public GameObject target2;
    public GameObject target3;
    private Rigidbody target1RigidBody;
    private Rigidbody target2RigidBody;
    private Rigidbody target3RigidBody;
    public float speed;

    BrainEventManager brainEventManager = null;

    private const bool giveGradualRewardForEachSphere = false;

    /*
     * If currentMission is
     * 1 : drop off target1
     * 2 : drop off target2
     * 3 : drop off target3
     */
    private int currentMission = 1;

    private static int atomicCounter = 0;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rBody = GetComponent<Rigidbody>();

        if(brain is PlayerBrain)
        {
            //int c = Interlocked.Increment(ref atomicCounter);
            //brainEventManager = new BrainEventManager("BrainEventChain_"+c);
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);

        //Target1 fell off
        if (target1.activeSelf && target1.transform.position.y + 1 < tankArena.transform.position.y)
        {
            LogEvent("Target1_fell");
            if (currentMission == 1)
            {
                currentMission++;
                LogEvent("CurrentMission="+currentMission);
                target1.SetActive(false);
                if (giveGradualRewardForEachSphere)
                {
                    AddReward(1 / 3f);
                }
            }
            else
            {
                EndLogEvent(false);
                Done();
            }
            return;
        }
        //Target2 fell off
        if (target2.activeSelf && target2.transform.position.y + 1 < tankArena.transform.position.y)
        {
            LogEvent("Target2_fell");
            if (currentMission == 2)
            {
                currentMission++;
                LogEvent("CurrentMission=" + currentMission);
                target2.SetActive(false);
                if (giveGradualRewardForEachSphere)
                {
                    AddReward(1 / 3f);
                }
            }
            else
            {
                EndLogEvent(false);
                Done();
            }
            return;
        }
        //Target3 fell off
        if (target3.activeSelf && target3.transform.position.y + 1 < tankArena.transform.position.y)
        {
            LogEvent("Target3_fell");
            if (currentMission == 3)
            {
                target3.SetActive(false);
                if (giveGradualRewardForEachSphere)
                {
                    AddReward(1 / 3f);
                }
                else
                {
                    AddReward(1);
                }
                EndLogEvent(true);
                Done();
            }
            else
            {
                EndLogEvent(false);
                Done();
            }
            return;
        }

        //Agent fell off platform
        if (transform.position.y < tankArena.transform.position.y + 0.45f)
        {
            //SetReward(-1);
            EndLogEvent(false);
            Done();
            return;
        }

        //AddReward(-1f / agentParameters.maxStep);
    }

    public override void AgentReset()
    { 
        if (!target1RigidBody)
        {
            target1RigidBody = target1.GetComponent<Rigidbody>();
        }
        if (!target2RigidBody)
        {
            target2RigidBody = target2.GetComponent<Rigidbody>();
        }
        if (!target3RigidBody)
        {
            target3RigidBody = target3.GetComponent<Rigidbody>();
        }

        if (transform.position.y < tankArena.transform.position.y + 0.45f)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(0, tankArena.transform.position.y + 0.5f, 0);
        }

        currentMission = 1;

        ResetAllTargets();

        StartLogEvent();
    }

    public override void CollectObservations()
    {
        AddVectorObs(transform.position.x);
        AddVectorObs(transform.position.z);

        float distanceFromCenter = Vector3.Distance(
                transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(distanceFromCenter);

        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        //Current mission
        AddVectorObs(currentMission);

        //Target1
        Vector3 relativePosition = target1.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        float distanceToTarget = Vector3.Distance(transform.position, target1.transform.position);
        AddVectorObs(distanceToTarget);

        float targetDistanceFromCenter = Vector3.Distance(
                target1.transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);

        AddVectorObs(target1RigidBody.velocity.x);
        AddVectorObs(target1RigidBody.velocity.z);

        //Target2
        relativePosition = target2.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        distanceToTarget = Vector3.Distance(transform.position, target2.transform.position);
        AddVectorObs(distanceToTarget);

        targetDistanceFromCenter = Vector3.Distance(
                target2.transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);

        AddVectorObs(target2RigidBody.velocity.x);
        AddVectorObs(target2RigidBody.velocity.z);

        //Target3
        relativePosition = target3.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        distanceToTarget = Vector3.Distance(transform.position, target3.transform.position);
        AddVectorObs(distanceToTarget);

        targetDistanceFromCenter = Vector3.Distance(
                target3.transform.position,
                new Vector3(0, tankArena.transform.position.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);

        AddVectorObs(target3RigidBody.velocity.x);
        AddVectorObs(target3RigidBody.velocity.z);
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

    private void ResetAllTargets()
    {
        target1RigidBody.velocity = Vector3.zero;
        target1RigidBody.angularVelocity = Vector3.zero;
        target2RigidBody.velocity = Vector3.zero;
        target2RigidBody.angularVelocity = Vector3.zero;
        target3RigidBody.velocity = Vector3.zero;
        target3RigidBody.angularVelocity = Vector3.zero;

        target1.SetActive(true);
        target2.SetActive(true);
        target3.SetActive(true);

        float spawnHeight = tankArena.transform.position.y + 0.5f;
        float arenaRadius = 4.5f;

        Vector3 targetPosition;
        float distanceToAgent;
        float distanceToTarget1;
        float distanceToTarget2;
        do
        {
            Vector2 r = Random.insideUnitCircle * arenaRadius;
            targetPosition = new Vector3(
                r.x,
                spawnHeight,
                r.y
            );
            distanceToAgent = Vector3.Distance(transform.position, targetPosition);
        } while (distanceToAgent < 1.1f);

        target1.transform.position = targetPosition;

        do
        {
            Vector2 r = Random.insideUnitCircle * arenaRadius;
            targetPosition = new Vector3(
                r.x,
                spawnHeight,
                r.y
            );
            distanceToAgent = Vector3.Distance(transform.position, targetPosition);
            distanceToTarget1 = Vector3.Distance(target1.transform.position, targetPosition);
        } while (distanceToAgent < 1.1f || distanceToTarget1 < 1.1f);
        target2.transform.position = targetPosition;

        do
        {
            Vector2 r = Random.insideUnitCircle * arenaRadius;
            targetPosition = new Vector3(
                r.x,
                spawnHeight,
                r.y
            );
            distanceToAgent = Vector3.Distance(transform.position, targetPosition);
            distanceToTarget1 = Vector3.Distance(target1.transform.position, targetPosition);
            distanceToTarget2 = Vector3.Distance(target2.transform.position, targetPosition);
        } while (distanceToAgent < 1.1f || distanceToTarget1 < 1.1f || distanceToTarget2 < 1.1f);
        target3.transform.position = targetPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject otherGameObject = collision.gameObject;
        if(otherGameObject == target1)
        {
            LogEvent("CollideWithTarget1");
            return;
        }
        else if(otherGameObject == target2)
        {
            LogEvent("CollideWithTarget2");
            return;
        }else if(otherGameObject == target3)
        {
            LogEvent("CollideWithTarget3");
            return;
        }
    }

    private void StartLogEvent()
    {
        if (brainEventManager == null)
        {
            return;
        }
        
        brainEventManager.StartRecordingNewEvents();
    }

    private void LogEvent(string s)
    {
        if(brainEventManager == null)
        {
            return;
        }

        brainEventManager.RecordEvent(s);
    }

    private void EndLogEvent(bool successfull)
    {
        if (brainEventManager == null)
        {
            return;
        }
        if (successfull)
        {
            brainEventManager.RecordGoalSucces();
        }
        else
        {
            brainEventManager.RecordGoalFailed();
        }
    }
}
