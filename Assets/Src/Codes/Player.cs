using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
	public Vector2 targetVec;
    public float speed;
    public string deviceId;
	public bool gottedResponse;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    TextMeshPro myText;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        myText = GetComponentInChildren<TextMeshPro>();
		gottedResponse = true;
    }

    public void InitCoordinate(float x, float y)
    {
        transform.position = new Vector2(x, y);
    }

    void OnEnable() {

        if (deviceId.Length > 5) {
            myText.text = deviceId[..5];
        } else {
            myText.text = deviceId;
        }
        myText.GetComponent<MeshRenderer>().sortingOrder = 6;
        
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive) {
            return;
        }
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

		Vector2 nextVec = inputVec * speed * Time.deltaTime;

		Vector2 sendVec;
		if ((int)inputVec.x == 0 && (int)inputVec.y == 0)
		{
			sendVec.x = 0;
			sendVec.y = 0;
		}
		else
		{
			sendVec.x = rigid.position.x + nextVec.x;
			sendVec.y = rigid.position.y + nextVec.y;
		}

        // 위치 이동 패킷 전송 -> 서버로
		if (gottedResponse)
		{
			NetworkManager.instance.SendLocationUpdatePacket(sendVec.x, sendVec.y);
			gottedResponse = false;
		}
    }


    void FixedUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }
        // 힘을 준다.
        // rigid.AddForce(inputVec);

        // 속도 제어
        // rigid.velocity = inputVec;

        // 위치 이동
		/*
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
		*/
		rigid.MovePosition(targetVec);
    }

    // Update가 끝난이후 적용
    void LateUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (!GameManager.instance.isLive) {
            return;
        }
    }
}
