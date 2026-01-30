using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yzz
{
    
    public class changeScene : MonoBehaviour
    {
        public Material ma;
        float count;
        public bool SetWhiteBool = false;
        public bool SetBlackBool = false;
        public float ChangeTime = 0.3f;
        // Use this for initialization
        void Start()
        {
            if(ma==null)
            {
                ma = new Material(Shader.Find("MyShader/BlackScreen"));
            }      
        }


        // Update is called once per frame
        void Update()
        {
            if (SetWhiteBool)
            {
                SetWhite();
            }
            else if (SetBlackBool)
            {
                SetBlack();
            }    
        
        }


        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //拷贝源纹理到目的渲染纹理。这主要是用于实现图像效果。  
            //Blit设置dest到激活的渲染纹理，在材质上设置source作为  
            //_MainTex属性，并且绘制一个全屏方块。  
            Graphics.Blit(source, destination, ma);
        }


        void SetWhite()
        {
            count = ma.GetFloat("_Float1") + ChangeTime * Time.deltaTime;
            ma.SetFloat("_Float1", count);
            if (ma.GetFloat("_Float1") >= 1)
            {
                SetWhiteBool = false;
            }
        }


        void SetBlack()
        {
            count = ma.GetFloat("_Float1") - ChangeTime * Time.deltaTime;
            ma.SetFloat("_Float1", count);
            if (ma.GetFloat("_Float1") <= 0)
            {
                SetBlackBool = false;
            }
        }
    }
    
}

