using MLAgents;
using System.Collections.Generic;
using UnityEngine;

public class TankAcademy : Academy
{
    public Transform[] m_SpawnPoints;
    public int m_NumberOfAgents;
    public bool m_isPlayerPresent = true;
    public GameObject m_TankAgentPrefab;
    public Brain m_PlayerBrain;
    public Brain m_LearningBrain;
    public GameObject m_goalSphere;
    public CameraControl m_CameraControl;
    public Camera m_mainCamera;
    protected GameObject[] m_Agents;
    private float timeOfEpisodeStart;

    public override void InitializeAcademy()
    {
        m_Agents = new GameObject[m_NumberOfAgents];
        m_CameraControl.m_Targets = new Transform[m_NumberOfAgents + 1];
        Monitor.SetActive(true);
    }

    public override void AcademyStep()
    {
        
    }

    internal void EventGoalSphereHitByBullet(int bulletTankId)
    {
        ResetGoalSphere();

        for (int i = 0; i < m_Agents.Length; i++)
        {
            TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
            if (bulletTankId == t.m_TankId)
            {
                t.HitPoint();
            }
        }
    }

    private void ResetGoalSphere()
    {
        int d = 30;
        Vector3 newPos = new Vector3(Random.Range(-d, d), 1.5f, Random.Range(-d, d));
        m_goalSphere.transform.position = newPos;
        Vector2 v = Random.insideUnitCircle.normalized * 10;
        Vector3 newVel = new Vector3(0, 0, 0);
        m_goalSphere.GetComponent<Rigidbody>().velocity = newVel;

        TankAgentScript t = m_Agents[0].GetComponent<TankAgentScript>();
        Vector3 relativeVector = m_goalSphere.transform.position - t.transform.position;
        float signedAngle = GetSignedAngleBetweenTankAndGoalSphere(t.transform);
        Rigidbody rBody = t.GetComponent<Rigidbody>();
        rBody.rotation *= Quaternion.Euler(0, -signedAngle, 0);
    }

    public float GetSignedAngleBetweenTankAndGoalSphere(Transform t)
    {
        Vector3 relativeVector = m_goalSphere.transform.position - t.position;
        relativeVector.y = 0;
        Vector3 forwardTank = t.forward;
        forwardTank.y = 0;
        return Vector3.SignedAngle(relativeVector, forwardTank, Vector3.up);
    }

    List<float> goalSphereObservationList = new List<float>();
    public List<float> GetRelativeGoalSphereObservations(Transform t)
    {
        goalSphereObservationList.Clear();
        Vector3 relativeVector = m_goalSphere.transform.position - t.position;
        goalSphereObservationList.Add(relativeVector.x);
        goalSphereObservationList.Add(relativeVector.z);
        goalSphereObservationList.Add(Vector3.Distance(m_goalSphere.transform.position, t.position));
        relativeVector.y = 0;
        Vector3 forwardTank = t.forward;
        forwardTank.y = 0;
        float signedAngle = GetSignedAngleBetweenTankAndGoalSphere(t);
        goalSphereObservationList.Add(signedAngle);

        return goalSphereObservationList;
    }
    
    public float GetDistanceToGoalSphere(Vector3 pos)
    {
        return Vector3.Distance(m_goalSphere.transform.position, pos);
    }
    

    public override void AcademyReset()
    {
        if (m_NumberOfAgents > m_SpawnPoints.Length)
        {
            Debug.LogError("There are not enough spawn points for all the agents");
        }
        if (m_NumberOfAgents <= 0)
        {
            Debug.LogError("The number of agents must be a positive number");
        }

        for (int i = 0; i < m_Agents.Length; i++)
        {
            if (m_Agents[i] != null)
            {
                TankAgentScript currentTankAgent = m_Agents[i].GetComponent<TankAgentScript>();
                currentTankAgent.Done();
                Destroy(m_Agents[i]);
            }
        }
        
        if (m_isPlayerPresent)
        {
            m_Agents[0] = CreateTankAgent(m_TankAgentPrefab, m_PlayerBrain, m_SpawnPoints[0].position, Quaternion.identity);
        }
        else
        {
            m_Agents[0] = CreateTankAgent(m_TankAgentPrefab, m_LearningBrain, m_SpawnPoints[0].position, Quaternion.identity);
        }

        for (int i = 1; i < m_NumberOfAgents; i++)
        {
            m_Agents[i] = CreateTankAgent(m_TankAgentPrefab, m_LearningBrain, m_SpawnPoints[i].position, Quaternion.identity);
        }

        for (int i = 0; i < m_NumberOfAgents; i++)
        {
            m_CameraControl.m_Targets[i] = m_Agents[i].transform;
            TankAgentScript currentTankAgent = m_Agents[i].GetComponent<TankAgentScript>();
            currentTankAgent.m_TankId = i;
            currentTankAgent.cam = m_mainCamera;
            currentTankAgent.tankAcademy = this;
            currentTankAgent.enemyTankAgents = new TankAgentScript[m_NumberOfAgents - 1];

            int k = 0;
            for (int j = 0; j < m_NumberOfAgents; j++)
            {
                if (i == j) continue;
                TankAgentScript otherTankAgent = m_Agents[j].GetComponent<TankAgentScript>();
                currentTankAgent.enemyTankAgents[k] = otherTankAgent;
                ++k;
            }
        }

        m_CameraControl.m_Targets[m_NumberOfAgents] = m_goalSphere.transform;

        TankShell.DisableAllShells();

        ResetGoalSphere();
    }

    private GameObject CreateTankAgent(GameObject tankAgentPrefab, Brain brain, Vector3 position, Quaternion orientation)
    {
        GameObject agentObj = Instantiate(tankAgentPrefab, position, orientation);
        Agent agent = agentObj.GetComponent<Agent>();
        agent.GiveBrain(brain);
        TankAgentScript tankAgentScript = agentObj.GetComponent<TankAgentScript>();
        tankAgentScript.m_SpawnPoint = position;
        agent.AgentReset();
        return agentObj;
    }


}
