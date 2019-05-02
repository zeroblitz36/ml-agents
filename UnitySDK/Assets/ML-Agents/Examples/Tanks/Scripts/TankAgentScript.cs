using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;
using System;
using UnityEngine.AI;

public class TankAgentScript : Agent
{
    public int m_TankId = 0;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public float m_StartingHealth = 100f;
    public float m_CurrentHealth;
    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    public Camera cam;
    private NavMeshAgent navMeshAgent;
    private Vector3 navDestination;
    public GameObject directionSphere;
    private bool isDestinationSet = false;

    public Vector3 m_SpawnPoint;

    public TankAgentScript[] enemyTankAgents;

    private Rigidbody rBody;
    private RayPerception rayPer = null;
    public TankAcademy tankAcademy;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rBody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        rayPer = GetComponent<RayPerception>();
    }

    public override void AgentReset()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        this.transform.position = m_SpawnPoint;

        m_CurrentHealth = m_StartingHealth;

        SetHealthUI();
    }

    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        Monitor.Log("health", m_CurrentHealth/m_StartingHealth, transform);
    }

    public void TakeDamage(float amount, int bulletTankId)
    {
        if(amount < 0)
        {
            Debug.LogError("TakeDamage must not receive a negative value");
            return;
        }
        if(IsDead())
        {
            //nothing to do , the tank is already dead
            return;
        }
        float previousHealth = m_CurrentHealth;
        m_CurrentHealth -= amount;
        m_CurrentHealth = Mathf.Max(0, m_CurrentHealth);

        //AddReward((m_CurrentHealth - previousHealth) / m_StartingHealth);
        tankAcademy.EventTankTookDamage(m_TankId, bulletTankId, amount, m_StartingHealth);
        SetHealthUI();
        if(previousHealth > 0 && m_CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    public void HitPoint()
    {
        AddReward(1);
    }

    private void OnDeath()
    {
        Done();
        tankAcademy.EventTankDied(m_TankId);
    }

    public bool IsDead() {
        return m_CurrentHealth <= 0;
    }

    public override void CollectObservations()
    {
        //Debug.Log("tank " + m_TankId + " reward = " + GetCumulativeReward());
        //Agent position and rotation
        AddVectorObs(this.transform.position.x);
        //The Y position can be ignored, the Tank never flies
        AddVectorObs(this.transform.position.z);
        //The Tank can only rotate around the y axis
        AddVectorObs(this.transform.rotation.y);

        //How much health it has
        if(m_CurrentHealth > 0)
        {
            AddVectorObs(m_CurrentHealth / m_StartingHealth);
        }
        else
        {
            AddVectorObs(0);
        }

        //Add velocity
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        float rayDistance = 10f;
        float[] rayAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        //string[] detectableObjects = { "tank", "stage" };
        //if(rayPer == null)
        //{
        //    rayPer = gameObject.AddComponent(typeof(RayPerception)) as RayPerception;
        //}
        
        List<float> rayList = rayPer.Perceive(rayDistance, rayAngles,new string[]{ "tank", "stage","goal" }, 1.5f, 1.5f);

        AddVectorObs(tankAcademy.GetRelativeGoalSphereObservations(transform));
        /*
        float[] logRayListInfo = rayList.ToArray();
        for(int i = 0; i < logRayListInfo.Length; i++)
        {
            logRayListInfo[i] /= rayDistance;
        }
        */
        //Monitor.Log("rayList", rayList.ToArray(), transform,Monitor.DisplayType.INDEPENDENT);
        //AddVectorObs(rayList);

        /*
        if (m_TankId == 0)
        {
            String rayString = "";
            foreach (float x in enemyTankRayList)
            {
                rayString += x + " ";
            }
            Debug.Log("rays size = "+ enemyTankRayList.Count +" content = '" + rayString + "'");
        }
        */
        /*
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                navDestination = hit.point;
                //navMeshAgent.isStopped = false;
                isDestinationSet = true;
                navMeshAgent.SetDestination(navDestination);
            }
        }
        else if (isDestinationSet && navMeshAgent.remainingDistance < 2)
        {
            //navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            isDestinationSet = false;
        }
        */
        if (enemyTankAgents == null)
        {
            return;
        }
        for(int i = 0; i < enemyTankAgents.Length; i++)
        {
            TankAgentScript enemyTankAgent = enemyTankAgents[0];
            GameObject enemyGameObject = enemyTankAgent.gameObject;
            /*
            //Agent position and rotation
            AddVectorObs(enemyGameObject.transform.position.x);
            //The Y position can be ignored, the Tank never flies
            AddVectorObs(enemyGameObject.transform.position.z);
            //The Tank can only rotate around the y axis
            AddVectorObs(enemyGameObject.transform.rotation.y);
            */
            Vector3 relativeVector = enemyGameObject.transform.position - transform.position;
            //Relative enemy position
            AddVectorObs(relativeVector.x);
            AddVectorObs(relativeVector.z);
            //Where is the enemy pointing at ?
            AddVectorObs(enemyGameObject.transform.rotation.y);
            //Add velocity
            AddVectorObs(enemyTankAgent.rBody.velocity.x);
            AddVectorObs(enemyTankAgent.rBody.velocity.z);

            //Are you looking at the enemy ?
            float signedAngle = Vector3.SignedAngle(transform.forward, relativeVector, transform.up);
            //signedAngle /= 180;
            AddVectorObs(signedAngle);
            Monitor.Log("angleToEnemy_"+enemyTankAgent.m_TankId, signedAngle/180, transform);

            //How much health it has
            if (enemyTankAgent.m_CurrentHealth > 0)
            {
                AddVectorObs(enemyTankAgent.m_CurrentHealth / enemyTankAgent.m_StartingHealth);
            }
            else
            {
                AddVectorObs(0);
            }
        }
    }

    private float m_TimeOfLastShot = 0;
    public float m_TimeBetweenShots = 0.5f;
    public Rigidbody m_Shell;
    public Transform m_FireTransform;
    public float m_ShellSpeed = 20;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (IsDead())
        {
            return;
        }
        /*
        //Vector3 nextPos = navMeshAgent.nextPosition;
        //Debug.Log("tank #" + m_TankId + " nextPos = " + nextPos);
        if (isDestinationSet && navMeshAgent.path.corners.Length > 1)
        {
            directionSphere.SetActive(true);
            Vector3 v = navMeshAgent.path.corners[1] - transform.position;
            v = v.normalized * 3;
            directionSphere.transform.position = v + transform.position;

            float signedAngle = Vector3.SignedAngle(transform.forward, v, transform.up);
            float aux = m_TurnSpeed * Time.deltaTime;
            float aux2 = Mathf.Clamp(signedAngle / aux, -1, 1);
            if(Mathf.Abs(aux2) < 0.1f)
            {
                aux2 = 0;
            }
            vectorAction[1] = aux2;

            if(Mathf.Abs(signedAngle) < 10)
            {
                vectorAction[0] = 1;
            }
        }
        else
        {
            directionSphere.SetActive(false);
        }
        */

        float forwardMovement = vectorAction[0];
        float turnValue = vectorAction[1];
        bool tryingToShoot = vectorAction[2] > 0.5f;
        if(forwardMovement < 0)
        {
            turnValue *= -1;
        }
        forwardMovement = Mathf.Clamp(forwardMovement, -1, 1);
        turnValue = Mathf.Clamp(turnValue, -1, 1);

        //Vector3 movement = transform.forward * forwardMovement * m_Speed * Time.deltaTime;
        //rBody.MovePosition(rBody.position + movement);
        rBody.AddForce(transform.forward * forwardMovement * m_Speed, ForceMode.VelocityChange);
        float turn = turnValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rBody.MoveRotation(rBody.rotation * turnRotation);

        if(tryingToShoot && Time.time > m_TimeOfLastShot + m_TimeBetweenShots)
        {
            m_TimeOfLastShot = Time.time;
            //BANG BANG
            //Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
            //shellInstance.velocity = m_ShellSpeed * m_FireTransform.forward;
            TankShell.FireShell(m_Shell, 
                m_FireTransform.position, 
                m_FireTransform.rotation,
                m_ShellSpeed * m_FireTransform.forward,
                m_TankId);
        }
    }

    public override void AgentOnDone()
    {
        base.AgentOnDone();
    }
}
