using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayer : Player
{
    protected override void Start()
    {
        playerType = PlayerType.Enemy; // EnemyPlayer�� ����
        base.Start(); // �θ� Ŭ������ Start() ȣ��

        Initialize();


        initialPosition = transform.position; // EnemyPlayer �ʱ� ��ġ ����
        initialRotation = transform.rotation; // EnemyPlayer �ʱ� ���� ����

    }
    protected override void Update()
    {
        // �θ� Ŭ������ Update ȣ��
        base.Update();

        // WŰ -> ������ �̵�
        inputValueY = Input.GetKey(KeyCode.W) ? 1f : 0f;
        // SŰ -> �ڷ� �̵�
        inputValueY += Input.GetKey(KeyCode.S) ? -1f : 0f;
        // AŰ -> �ð���� ȸ��
        rotationInput = Input.GetKey(KeyCode.A) ? 1f : 0f;
        // DŰ -> �ݽð���� ȸ��
        rotationInput += Input.GetKey(KeyCode.D) ? -1f : 0f;
    }
}
