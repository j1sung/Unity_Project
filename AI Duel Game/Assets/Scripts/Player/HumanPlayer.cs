using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class HumanPlayer : Player
{
    protected override void Start() // �⺻ ����
    {
        playerType = PlayerType.Human; // HumanPlayer�� ����
        base.Start();  // �θ� Ŭ������ Start() ȣ��

        Initialize();

        initialPosition = transform.position; // HumanPlayer �ʱ� ��ġ ����
        initialRotation = transform.rotation; // HumanPlayer �ʱ� ���� ����
      
    }


   
    protected override void Update() // ���� 60fps(1�� 60�� ��) -> 60���� ������ -> 1���� �����Ӹ��� �ݺ�
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
