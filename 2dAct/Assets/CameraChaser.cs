using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChaser : MonoBehaviour
{
    [SerializeField] GameObject targetObj;
    [SerializeField] Vector3 margin;
    [SerializeField] float chaseSpeed;

    Vector3 beforePos;

    private void Start()
    {
        var pos = targetObj.transform.position + margin;
        pos = new Vector3(pos.x, pos.y, Camera.main.transform.position.z);
        Camera.main.transform.position = pos;
        beforePos = Camera.main.transform.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 goalPos = targetObj.transform.position + margin;
        goalPos.z = Camera.main.transform.position.z;
        Camera.main.transform.position = Vector3.Lerp(beforePos, goalPos, chaseSpeed);
        beforePos = Camera.main.transform.position;
        beforePos.z = Camera.main.transform.position.z;
    }
}
