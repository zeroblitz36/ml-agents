using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class TankAcademy : Academy
{
    public Transform[] m_SpawnPoints;
    public int m_NumberOfAgents;
    public bool m_isPlayerPresent = true;
    public GameObject m_TankAgentPrefab;
    public Brain m_PlayerBrain;
    public Brain m_LearningBrain;

    public CameraControl m_CameraControl;
    protected GameObject[] m_Agents;
    public override void AcademyReset()
    {
        base.AcademyReset();
    }

    public override void InitializeAcademy()
    {
        if(m_NumberOfAgents > m_SpawnPoints.Length)
        {
            Debug.LogError("There are not enough spawn points for all the agents");
        }
        if(m_NumberOfAgents <= 0)
        {
            Debug.LogError("The number of agents must be a positive number");
        }

        m_Agents = new GameObject[m_NumberOfAgents];

        if (m_isPlayerPresent)
        {
            m_Agents[0] = CreateTankAgent(m_TankAgentPrefab, m_PlayerBrain, m_SpawnPoints[0].position, Quaternion.identity);
        }
        else
        {
            m_Agents[0] = CreateTankAgent(m_TankAgentPrefab, m_LearningBrain, m_SpawnPoints[0].position, Quaternion.identity);
        }

        for(int i = 1; i < m_NumberOfAgents; i++)
        {
            m_Agents[i] = CreateTankAgent(m_TankAgentPrefab, m_LearningBrain, m_SpawnPoints[i].position, Quaternion.identity);
        }

        Transform[] agentTransforms = new Transform[m_NumberOfAgents];

        for(int i = 0; i < m_NumberOfAgents; i++)
        {
            agentTransforms[i] = m_Agents[i].transform;
        }

        m_CameraControl.m_Targets = agentTransforms;
    }

    private GameObject CreateTankAgent(GameObject tankAgentPrefab, Brain brain, Vector3 position, Quaternion orientation)
    {
        GameObject agentObj = Instantiate(tankAgentPrefab, position, orientation);
        Agent agent = agentObj.GetComponent<Agent>();
        agent.GiveBrain(brain);
        agent.AgentReset();
        return agentObj;
    }
}
