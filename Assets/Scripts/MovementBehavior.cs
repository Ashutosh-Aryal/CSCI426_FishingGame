using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementBehavior : MonoBehaviour
{
    private const KeyCode MOVE_UP_KEY = KeyCode.W;
    private const KeyCode MOVE_DOWN_KEY = KeyCode.S;

    private const float MOVEMENT_SPEED = 2.5f;
    private const int FISH_LAYER = 8;

    public static bool s_IsReeling = false;

    private Rigidbody2D myRigidBody2D;
    public static GameObject s_ReeledFish = null;
    private Vector3 m_CaughtPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    { 
        myRigidBody2D = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {

        if (s_IsReeling) {
            SetPosition(); return;
        }

        if(Input.GetKey(MOVE_DOWN_KEY)) {
            myRigidBody2D.velocity = Vector2.down * MOVEMENT_SPEED;
        } else if(Input.GetKey(MOVE_UP_KEY)) {
            myRigidBody2D.velocity = Vector2.up * MOVEMENT_SPEED;
        } else {
            myRigidBody2D.velocity = Vector2.zero;
        }
    }

    private void SetPosition()
    {
        Slider slider = UI_Behavior.GetSlider();
        Vector3 newPosition = Vector3.Lerp(m_CaughtPosition, FishingLineBehavior.s_StartingPosition, slider.value / slider.maxValue);
        gameObject.transform.position = newPosition;
        s_ReeledFish.transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == FISH_LAYER && !s_IsReeling)
        {
            s_IsReeling = true;
            s_ReeledFish = collision.gameObject;
            FishScript colliderFishScript = collision.gameObject.GetComponent<FishScript>();
            colliderFishScript.isCaught = true;
            m_CaughtPosition = gameObject.transform.position;
            colliderFishScript.caughtPosition = m_CaughtPosition;
            UI_Behavior.s_SliderMaxValue = colliderFishScript.catchTime;
            UI_Behavior.DisplayUI(true);
        }
    }
}
