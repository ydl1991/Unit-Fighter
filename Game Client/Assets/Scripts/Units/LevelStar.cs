using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStar : MonoBehaviour
{
    public Material[] m_materialByLevel;
    private Unit m_owner;
    private MeshRenderer m_renderer;

    // Start is called before the first frame update
    void Start()
    {
        m_renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        RotateToCamera();
        UpdateMaterial();
    }

    public void SetOwner(Unit unit)
    {
        m_owner = unit;
    }

    private void RotateToCamera()
    {
        Vector3 targetVec = Camera.main.transform.position - transform.position;
        float newXAngle = Mathf.Atan2(targetVec.z, targetVec.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(newXAngle, 0, 0);
    }

    private void UpdateMaterial()
    {
        if (m_renderer.material != m_materialByLevel[(int)m_owner.unitLevel])
            m_renderer.material = m_materialByLevel[(int)m_owner.unitLevel];
    }
}
