using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitIngameUIController : MonoBehaviour
{
    public GameObject uiprefab;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(uiprefab, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
