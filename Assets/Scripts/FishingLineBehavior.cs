using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingLineBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject m_StartingPoint;

    [SerializeField]
    private GameObject m_EndingPoint;

    public static Vector3 s_StartingPosition;    

    private LineRenderer myLineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        myLineRenderer = gameObject.GetComponent<LineRenderer>();
        myLineRenderer.positionCount = 2;
        myLineRenderer.widthMultiplier = 0.1f;

        s_StartingPosition = m_StartingPoint.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        myLineRenderer.SetPosition(0, m_StartingPoint.transform.position);
        myLineRenderer.SetPosition(1, m_EndingPoint.transform.position);
    }
}
