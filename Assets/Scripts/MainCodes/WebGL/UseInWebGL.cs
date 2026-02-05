using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseInWebGL : MonoBehaviour
{
    [SerializeField]
    private bool useInWebGL = true;

    private void Awake() {
        #if UNITY_WEBGL
        gameObject.SetActive(useInWebGL);
        #endif
    }
}
