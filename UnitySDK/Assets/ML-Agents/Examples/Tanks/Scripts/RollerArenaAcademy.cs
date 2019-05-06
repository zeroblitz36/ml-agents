using MLAgents;
using UnityEngine;

public class RollerArenaAcademy : Academy
{
    public GameObject m_RollerArenaPrefab;
    public int m_NumberOfArenas;

    public override void InitializeAcademy()
    {
        base.InitializeAcademy();

        //skip the first arena
        for(int i = 1; i < m_NumberOfArenas; i++)
        {
            GameObject o = Instantiate(m_RollerArenaPrefab, new Vector3(0, -4 * i, 0), Quaternion.identity);
        }
    }

    public override void AcademyReset()
    {

    }

    public override void AcademyStep()
    {

    }
}
