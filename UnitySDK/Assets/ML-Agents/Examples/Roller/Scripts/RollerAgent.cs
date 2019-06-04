using MLAgents;
using System.Collections.Generic;
using UnityEngine;

public class RollerAgent : Agent
{
    Rigidbody rBody;
    public Vector3 arenaPosition;
    public GameObject target1;
    public GameObject target2;
    public GameObject target3;
    private Rigidbody target1RigidBody;
    private Rigidbody target2RigidBody;
    private Rigidbody target3RigidBody;
    public float speed;

    private RollerArenaAcademy academy;

    private const bool giveGradualRewardForEachSphere = true;
    private const bool eachMissionIsItsOwnBoolean = false;
    private const bool rewardOnTouch = false;
    private int numberOfTargets = 3;
    private bool m_UseCurriculumLearning = true;

    /*
     * If currentMission is
     * 1 : drop off target1
     * 2 : drop off target2
     * 3 : drop off target3
     */
    private int currentMission = 1;

    public override void InitializeAgent()
    {
        academy = FindObjectOfType<RollerArenaAcademy>();
        if (numberOfTargets < 1)
        {
            numberOfTargets = 1;
        }else if(numberOfTargets > 3)
        {
            numberOfTargets = 3;
        }
        base.InitializeAgent();
        rBody = GetComponent<Rigidbody>();
    }

    private void Goal1Achieved()
    {
        if (currentMission == 1)
        {
            target1.SetActive(false);
            if (giveGradualRewardForEachSphere)
            {
                AddReward(1f / numberOfTargets);
            }
            if(currentMission == numberOfTargets)
            {
                if (!giveGradualRewardForEachSphere)
                {
                    AddReward(1);
                }
                Done();
            }
            currentMission++;
        }
        else
        {
            Done();
        }
    }

    private void Goal2Achieved()
    {
        if (currentMission == 2)
        {
            target2.SetActive(false);
            if (giveGradualRewardForEachSphere)
            {
                AddReward(1f / numberOfTargets);
            }
            if (currentMission == numberOfTargets)
            {
                if (!giveGradualRewardForEachSphere)
                {
                    AddReward(1);
                }
                Done();
            }
            currentMission++;
        }
        else
        {
            Done();
        }
    }

    private void Goal3Achieved()
    {
        if (currentMission == 3)
        {
            target3.SetActive(false);
            if (giveGradualRewardForEachSphere)
            {
                AddReward(1f / numberOfTargets);
            }
            if (currentMission == numberOfTargets)
            {
                if (!giveGradualRewardForEachSphere)
                {
                    AddReward(1);
                }
                Done();
            }
            currentMission++;
        }
        else
        {
            Done();
        }
    }
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        Vector3 controlSignal = new Vector3(vectorAction[0], 0, vectorAction[1]);
        rBody.AddForce(controlSignal * speed);

        if (target1.activeSelf && numberOfTargets >= 1 && target1.transform.position.y + 1 < arenaPosition.y){
            Goal1Achieved();return;
        }
        if (target2.activeSelf && numberOfTargets >= 2 && target2.transform.position.y + 1 < arenaPosition.y){
            Goal2Achieved();return;
        }
        if (target3.activeSelf && numberOfTargets >= 3 && target3.transform.position.y + 1 < arenaPosition.y){
            Goal3Achieved();return;
        }
        if (transform.position.y < arenaPosition.y + 0.45f){
            Done();
        }
    }

    public override void AgentReset()
    {
        if (m_UseCurriculumLearning)
        {
            numberOfTargets = (int)(academy.resetParameters["sphere_count"]);
        }
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

        if (transform.position.y < arenaPosition.y + 0.45f)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(0, arenaPosition.y + 0.5f, 0);
        }

        currentMission = 1;

        ResetAllTargets();
    }

    public override void CollectObservations()
    {
        if(agentParameters.agentCameras.Count > 0)
        {
            return;
        }

        AddVectorObs(transform.position.x);
        AddVectorObs(transform.position.z);

        float distanceFromCenter = Vector3.Distance(
                transform.position,
                new Vector3(0, arenaPosition.y + 0.5f, 0)
            );
        AddVectorObs(distanceFromCenter);

        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        //Current mission
        if (eachMissionIsItsOwnBoolean)
        {
            for(int i = 1; i <= 3; i++)
            {
                AddVectorObs(currentMission == i);
            }
        }
        else
        {
            AddVectorObs(currentMission);
        }

        //Target1
        Vector3 relativePosition = target1.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        float distanceToTarget = Vector3.Distance(transform.position, target1.transform.position);
        AddVectorObs(distanceToTarget);

        float targetDistanceFromCenter = Vector3.Distance(
                target1.transform.position,
                new Vector3(0, arenaPosition.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);

        AddVectorObs(target1RigidBody.velocity.x);
        AddVectorObs(target1RigidBody.velocity.z);

        //Target2
        if (numberOfTargets < 2 && !m_UseCurriculumLearning) return;
        relativePosition = target2.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        distanceToTarget = Vector3.Distance(transform.position, target2.transform.position);
        AddVectorObs(distanceToTarget);

        targetDistanceFromCenter = Vector3.Distance(
                target2.transform.position,
                new Vector3(0, arenaPosition.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);

        AddVectorObs(target2RigidBody.velocity.x);
        AddVectorObs(target2RigidBody.velocity.z);
        
        //Target3
        if (numberOfTargets < 3 && !m_UseCurriculumLearning) return;
        relativePosition = target3.transform.position - transform.position;
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        distanceToTarget = Vector3.Distance(transform.position, target3.transform.position);
        AddVectorObs(distanceToTarget);

        targetDistanceFromCenter = Vector3.Distance(
                target3.transform.position,
                new Vector3(0, arenaPosition.y + 0.5f, 0)
            );
        AddVectorObs(targetDistanceFromCenter);

        AddVectorObs(target3RigidBody.velocity.x);
        AddVectorObs(target3RigidBody.velocity.z);
    }
    
    private void ResetAllTargets()
    {
        target1RigidBody.velocity = Vector3.zero;
        target1RigidBody.angularVelocity = Vector3.zero;
        target2RigidBody.velocity = Vector3.zero;
        target2RigidBody.angularVelocity = Vector3.zero;
        target3RigidBody.velocity = Vector3.zero;
        target3RigidBody.angularVelocity = Vector3.zero;
        
        target1.SetActive(numberOfTargets >= 1);
        target2.SetActive(numberOfTargets >= 2);
        target3.SetActive(numberOfTargets >= 3);

        float spawnHeight = arenaPosition.y + 0.5f;
        float arenaRadius = 4.5f;

        Vector3 targetPosition;
        Vector2 r;
        float distanceToAgent;
        float distanceToTarget1;
        float distanceToTarget2;
        do
        {
            r = Random.insideUnitCircle * arenaRadius;
            targetPosition = new Vector3(r.x, spawnHeight, r.y);
            distanceToAgent = Vector3.Distance(transform.position, targetPosition);
        } while (distanceToAgent < 1.1f);

        target1.transform.position = targetPosition;

        if (numberOfTargets < 2) return;
        do
        {
            r = Random.insideUnitCircle * arenaRadius;
            targetPosition = new Vector3(r.x, spawnHeight, r.y);
            distanceToAgent = Vector3.Distance(transform.position, targetPosition);
            distanceToTarget1 = Vector3.Distance(target1.transform.position, targetPosition);
        } while (distanceToAgent < 1.1f || distanceToTarget1 < 1.1f);
        target2.transform.position = targetPosition;

        if (numberOfTargets < 3) return;
        do
        {
            r = Random.insideUnitCircle * arenaRadius;
            targetPosition = new Vector3(r.x, spawnHeight, r.y);
            distanceToAgent = Vector3.Distance(transform.position, targetPosition);
            distanceToTarget1 = Vector3.Distance(target1.transform.position, targetPosition);
            distanceToTarget2 = Vector3.Distance(target2.transform.position, targetPosition);
        } while (distanceToAgent < 1.1f || distanceToTarget1 < 1.1f || distanceToTarget2 < 1.1f);
        target3.transform.position = targetPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!rewardOnTouch)
        {
            return;
        }
        GameObject otherGameObject = collision.gameObject;
        
        if(otherGameObject == target1)
        {
            Goal1Achieved();
        }
        if(otherGameObject == target2)
        {
            Goal2Achieved();
        }
        if (otherGameObject == target3)
        {
            Goal3Achieved();
        }
    }
}
