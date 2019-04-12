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

    public override void InitializeAcademy()
    {
        m_Agents = new GameObject[m_NumberOfAgents];
        m_CameraControl.m_Targets = new Transform[m_NumberOfAgents];
    }


    public override void AcademyStep()
    {
        if(m_Agents.Length <= 1)
        {
            //somebody intentionally wanted only one tank agent, so skip the rest of the logic
            return;
        }
        int aliveCount = 0;
        for (int i = 0; i < m_Agents.Length; i++)
        {
            TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
            if (!t.IsDead())
            {
                aliveCount++;
            }
        }
        if(aliveCount <= 1)
        {
            Done();
        }
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
            //m_Agents[i] = CreateTankAgent(m_TankAgentPrefab, m_LearningBrain, m_SpawnPoints[i].position, Quaternion.identity);
            m_Agents[i] = CreateTankAgent(m_TankAgentPrefab, m_PlayerBrain, m_SpawnPoints[i].position, Quaternion.identity);
        }

        for (int i = 0; i < m_NumberOfAgents; i++)
        {
            m_CameraControl.m_Targets[i] = m_Agents[i].transform;
            m_Agents[i].GetComponent<TankAgentScript>().m_TankId = i;
        }
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
