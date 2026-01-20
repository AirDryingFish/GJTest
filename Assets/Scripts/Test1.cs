using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    private float time = 0.0f;
    private bool moveRight = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        

        Debug.Log(time);
        if (time <= 1.0f)
        {
            if (moveRight)
            {
                this.transform.position += new Vector3(1, 0, 0) * Time.deltaTime;
            }
            else
            {
                this.transform.position -= new Vector3(1, 0, 0) * Time.deltaTime;
            }
        }
        else {
            time = 0.0f;
            moveRight = !moveRight;
        }

    }
}
