using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// //feel shot decision.. 사격을 위해 결정할 멤버변수들
///cover decision.. 커버를 위해 결정할 멤버변수들
///repeat decision.. 반복을 위해 결정할 멤버변수들
///patrol decision.. 정찰을 위해 결정할 멤버변수들
///attack decision.. 공격을 위해 결정할 멤버변수들
/// </summary>
[Serializable]
public class EnemyVariables
{
    public bool feelAlert; //위험느꼇나
    public bool hearAlert; //소리를 들었나
    public bool advanceCoverDecision; //타겟과 가까운 좋은 엄페물이 있는가
    public int waitRounds; //교전중 플레이어가 몇발 쏘고나면 공격
    public bool repeatShot; //반복적으로 공격할것인가
    public float waitInCoverTime; //엄폐를 하고나서 얼마나 기다릴것인가
    public float coverTime; //이번 교전에서 얼마나 숨어있는중인가
    public float patrolTimer; //얼마나 정찰중인가
    public float shotTimer; //총쏘는 딜레이
    public float startShootTimer; 
    public float currentShots; //현재 발사한 총알 갯수
    public float shotsInRounds; //교전중 얼마나 총알을 썼는가
    public float blindEngageTimer; //플레이어가 시야에서 사라졌을 경우 얼마나 플레이어를 찾는가
}
