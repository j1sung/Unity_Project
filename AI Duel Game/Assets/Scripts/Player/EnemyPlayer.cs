using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayer : Player
{
    protected override void Start()
    {
        playerType = PlayerType.Enemy; // EnemyPlayer로 설정
        base.Start(); // 부모 클래스의 Start() 호출

        Initialize();


        initialPosition = transform.position; // EnemyPlayer 초기 위치 저장
        initialRotation = transform.rotation; // EnemyPlayer 초기 방향 저장

    }
    protected override void Update()
    {
        // 부모 클래스의 Update 호출
        base.Update();

        // W키 -> 앞으로 이동
        inputValueY = Input.GetKey(KeyCode.W) ? 1f : 0f;
        // S키 -> 뒤로 이동
        inputValueY += Input.GetKey(KeyCode.S) ? -1f : 0f;
        // A키 -> 시계방향 회전
        rotationInput = Input.GetKey(KeyCode.A) ? 1f : 0f;
        // D키 -> 반시계방향 회전
        rotationInput += Input.GetKey(KeyCode.D) ? -1f : 0f;
    }
}
