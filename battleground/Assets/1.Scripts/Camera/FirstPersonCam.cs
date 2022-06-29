using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//카메라 속성중 중요 속성 하나는 피폿 오프셋 벡터
//피봇 오프셋 벡터는 시선 이동에 사용
//사격 반동을 위한 기능
//FOV 변경 기능
public class FirstPersonCam : MonoBehaviour
{   
    public float horizontalAimingSpeed = 6.0f; //수평 회전 속도
    public float verticalAimingSpeed = 6.0f; //수직 회전 속도
    public float maxVerticalAngle = 90.0f; //카메라의 수직 최대 각도
    public float minVerticalAngle = -90.0f; //카메라의 수직 최소 각도
    public float recoilAngleBouce = 5.0f; //사격 반동 바운스 값
    private float angleH = 0.0f; //마우스 이동에 따른 카메라 수평이동 수치
    private float angleV = 0.0f; //마우스 이동에 따른 카메라 수직이동 수치   
    
    private float defaultFOV; //기본 시야값   
    private float targetMaxVerticleAngle; //카메라 수직 최대 각도
    private float recoilAngle = 0f;

    public float GetH
    {
        get
        {
            return angleH;
        }
    }      

    public void BounceVertical(float degree)
    {
        recoilAngle = degree;
    }        

    private void Update()
    {
        //마우스 이동 값
        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalAimingSpeed;
        angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalAimingSpeed;
        //수직 이동 제한
        angleV = Mathf.Clamp(angleV, minVerticalAngle, maxVerticalAngle);
        //수직 카메라 바운스
        angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);

        transform.eulerAngles = new Vector3( -angleV, angleH, 0);

        if (recoilAngle > 0.0f)
        {
            recoilAngle -= recoilAngleBouce * Time.deltaTime;
        }
        else if (recoilAngle < 0.0f)
        {
            recoilAngle += recoilAngleBouce * Time.deltaTime;
        }
    }   
}
