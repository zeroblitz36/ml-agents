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
        //Monitor.Log()
    }

    public override void AcademyStep()
    {
        if (startedStageCooldown && Time.time - timeWhenLastTankAlive > 1)
        {
            Done();
            return;
        }
        if(!startedStageCooldown && Time.time - timeOfEpisodeStart > 120)
        {
            int bestTankId = 0;
            for (int i = 1; i < m_Agents.Length; i++)
            {
                TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
                TankAgentScript bestTank = m_Agents[bestTankId].GetComponent<TankAgentScript>();
                if (t.m_CurrentHealth > bestTank.m_CurrentHealth)
                {
                    bestTankId = t.m_TankId;
                }
            }
            for(int i = 0; i < m_Agents.Length; i++)
            {
                TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
                if(t.m_TankId == bestTankId)
                {
                    //t.AddReward(10);
                }
                t.Done();
            }
            startedStageCooldown = true;
            timeWhenLastTankAlive = Time.time;
            //Done();
            //return;
        }
    }

    private bool startedStageCooldown = false;
    private float timeWhenLastTankAlive;
    public void EventTankDied(int tankId)
    {
        int aliveCount = 0;
        for (int i = 0; i < m_Agents.Length; i++)
        {
            TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
            if (!t.IsDead())
            {
                aliveCount++;
            }
        }
        if(aliveCount <= 0)
        {
            //all tanks have died
            Done();
            return;
        }
        if (aliveCount <= 1)
        {
            for (int i = 0; i < m_Agents.Length; i++)
            {
                TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
                if (!t.IsDead())
                {
                    //t.AddReward(10);
                    //t.Done();
                    //break;
                }
            }
            //Done();
            if (!startedStageCooldown)
            {
                startedStageCooldown = true;
                timeWhenLastTankAlive = Time.time;
                //Debug.Log("timeWhenLastTankAlive = " + timeWhenLastTankAlive);
            }
            return;
        }
    }

    public void EventTankTookDamage(int tankId, int bulletTankId, float damage, float startingHealth)
    {
        for(int i = 0; i < m_Agents.Length; i++)
        {
            TankAgentScript t = m_Agents[i].GetComponent<TankAgentScript>();
            float reward = damage / startingHealth;
            if(tankId == t.m_TankId)
            {
                //t.AddReward(-reward);
            }else if(bulletTankId == t.m_TankId)
            {
                t.AddReward(reward);
            }
            /*
            if (t.m_TankId == tankId)
            {
                reward *= -1;
            }
            if(t.m_TankId == bulletTankId)
            {
                reward *= 2;
            }
            t.AddReward(reward);
            */
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
            m_Agents[i] = CreateTankAgent(m_TankAgentPrefab, m_LearningBrain, m_SpawnPoints[i].position, Quaternion.identity);
            //m_Agents[i] = CreateTankAgent(m_TankAgentPrefab, m_PlayerBrain, m_SpawnPoints[i].position, Quaternion.identity);
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

        startedStageCooldown = false;
        timeOfEpisodeStart = Time.time;
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
