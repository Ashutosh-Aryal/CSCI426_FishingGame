using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Behavior : MonoBehaviour
{
    private const float ACCELERATION_INTERVAL = 0.05f;
    private const float MIN_CUBE_MOVEMENT_TIME = 0.50f;
    private const float MAX_CUBE_MOVEMENT_TIME = 1.0f;
    private const float MIN_TIME_TO_MOVE_CUBE_AGAIN = 1.5f;
    private const float MAX_TIME_TO_MOVE_CUBE_AGAIN = 4.5f;

    private const KeyCode UP_KEY = KeyCode.W;
    private const KeyCode RIGHT_KEY = KeyCode.D;
    private const KeyCode LEFT_KEY = KeyCode.A;

    private static Vector2[] s_MovementDirections = new[] { Vector2.up, Vector2.left, Vector2.right };
    private static KeyCode[] s_MovementKeys = new[] { UP_KEY, LEFT_KEY, RIGHT_KEY };

    private static float[] ORG_FORCE_MAGNITUDE_SCALAR = new[] { 1.0f, 0.7f, 0.7f };

    private static float[] s_ForceMagnitudeScalar = new[] { 1.0f, 0.7f, 0.7f };
    private static float[] s_PrevTime = new[] { 0.0f, 0.0f, 0.0f };

    private static GameObject s_ReelingSliderObject;
    private static GameObject s_FishingPopUpPanel;
    private static GameObject s_PlayerObject;
    private static GameObject s_HollowCubeObject;
    private static Vector3 s_BottomLeftLimits;
    private static Vector3 s_TopRightLimits;

    private static Vector3 s_InitialHollowCubePosition;
    private static Vector3 s_HollowCubeStartPosition = Vector3.zero;
    private static Vector3? s_HollowCubeDestinationPosition = null;

    private static Slider s_ReelingSlider;
    private static Slider s_ProgressSlider;
    private static Rigidbody2D s_PlayerRigidbody2D;
    private static Vector3 s_PlayerStartPosition;

    private static Vector2 s_MovementForce;

    private static float s_TimerToMoveCube = 0.0f;
    private static float s_CubeMovementTimer = 0.0f;
    private static float s_TimeCubeMovementShouldTake = 0.0f;

    public AudioSource m_FishCaughtSound;

    public AudioSource m_FishOnLineSound;

    public AudioSource m_FishLostSound;

    private static bool s_ShouldIncrementSliderValue = true;
    private static UI_Behavior s_Instance;
    
    public static float s_SliderMaxValue = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        s_FishingPopUpPanel = gameObject.transform.parent.gameObject;
        s_ReelingSliderObject = s_FishingPopUpPanel.transform.parent.gameObject.transform.GetChild(1).gameObject;
        s_ProgressSlider = s_FishingPopUpPanel.transform.parent.gameObject.transform.GetChild(2).gameObject.GetComponent<Slider>();
        
        s_PlayerObject = s_FishingPopUpPanel.transform.GetChild(0).gameObject;
        s_HollowCubeObject = s_FishingPopUpPanel.transform.GetChild(1).gameObject;
        s_TopRightLimits = s_FishingPopUpPanel.transform.GetChild(2).position;
        s_BottomLeftLimits = s_FishingPopUpPanel.transform.GetChild(3).position;

        s_ReelingSlider = s_ReelingSliderObject.GetComponent<Slider>();
        s_ReelingSlider.minValue = 0.0f;
        s_ReelingSlider.maxValue = s_SliderMaxValue;

        s_PlayerStartPosition = s_PlayerObject.transform.position;
        s_PlayerRigidbody2D = s_PlayerObject.GetComponent<Rigidbody2D>();
        s_PlayerRigidbody2D.freezeRotation = true;

        s_InitialHollowCubePosition = s_HollowCubeObject.transform.position;

        s_ProgressSlider.minValue = 0.0f;
        s_ProgressSlider.maxValue = 100.0f;

        DisplayUI(false);

        s_Instance = this;
    }

    private static Vector3 GetRandomLocationForCube()
    {
        if(s_TimeCubeMovementShouldTake == 0.0f) {
            return Vector3.zero;
        }

        float xLocation = Mathf.Lerp(s_BottomLeftLimits.x, s_TopRightLimits.x, Random.Range(0.0f, 1.0f));
        float yLocation = Mathf.Lerp(s_BottomLeftLimits.y, s_TopRightLimits.y, Random.Range(0.0f, 1.0f));
        float zLocation = s_BottomLeftLimits.z;
        return new Vector3(xLocation, yLocation, zLocation);
    }

    public static Slider GetSlider()
    {
        return s_ReelingSlider;
    }

    public static void DisplayUI(bool shouldDisplay)
    {
        if(!shouldDisplay) {
            s_ReelingSlider.value = 0.0f;
            s_PlayerObject.transform.position = s_PlayerStartPosition;
            s_PlayerRigidbody2D.gravityScale = 0.0f;
        } else {
            s_Instance.m_FishOnLineSound.PlayOneShot(s_Instance.m_FishOnLineSound.clip);
            s_PlayerRigidbody2D.gravityScale = 12.0f;
            s_ReelingSlider.maxValue = s_SliderMaxValue;
            s_ReelingSlider.value = s_ReelingSlider.maxValue / 4.0f;
            s_HollowCubeObject.transform.position = s_InitialHollowCubePosition;
            s_PlayerObject.transform.position = s_InitialHollowCubePosition;
            s_TimerToMoveCube = Random.Range(MIN_CUBE_MOVEMENT_TIME, MAX_CUBE_MOVEMENT_TIME);
        }

        s_ReelingSliderObject.SetActive(shouldDisplay);
        s_FishingPopUpPanel.SetActive(shouldDisplay);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!s_FishingPopUpPanel.activeInHierarchy) {
            return;
        }

        UpdatePlayerMovement();
        UpdateHollowCube();
        UpdateSlider();
    }

    private void UpdatePlayerMovement()
    {
        s_MovementForce = Vector2.zero;

        for (int x = 0; x < 3; x++)
        {
            UpdateMovementSpeed(s_MovementKeys[x], x);
        }

        s_PlayerRigidbody2D.AddForce(s_MovementForce, ForceMode2D.Impulse);
    }

    private void UpdateMovementSpeed(KeyCode key, int index)
    {
        bool isLeadingEdge = Input.GetKeyDown(key);
        bool isHolding = Input.GetKey(key);
        bool shouldAccelerate = Time.time - s_PrevTime[index] >= (ACCELERATION_INTERVAL) && isHolding && s_ForceMagnitudeScalar[index] <= ((index == 0)? 5.0f : 1.8f);
        if(!isHolding) {
            s_ForceMagnitudeScalar[index] = ORG_FORCE_MAGNITUDE_SCALAR[index]; return;
        }
         
        if (isLeadingEdge) {
            s_PrevTime[index] = Time.time;
        }
        else if (shouldAccelerate) {

            if (index == 0)
            {
                s_ForceMagnitudeScalar[index] += ACCELERATION_INTERVAL * 5.0f;
            }
            else
            {
                s_ForceMagnitudeScalar[index] += ACCELERATION_INTERVAL;
            }
            //s_PrevTime[index] = Time.time;
        }

        s_MovementForce += s_MovementDirections[index] * s_ForceMagnitudeScalar[index];
    }

    private void UpdateHollowCube()
    {
        if(!s_HollowCubeDestinationPosition.HasValue)
        {
            s_TimerToMoveCube -= Time.fixedDeltaTime;

            if(s_TimerToMoveCube <= 0.0f)
            {
                s_TimeCubeMovementShouldTake = Random.Range(MIN_CUBE_MOVEMENT_TIME, MAX_CUBE_MOVEMENT_TIME);
                s_CubeMovementTimer = 0.0f;
                s_HollowCubeStartPosition = gameObject.transform.position;
                s_HollowCubeDestinationPosition = GetRandomLocationForCube();
            }

        } else
        {
            s_CubeMovementTimer += Time.fixedDeltaTime;
            s_HollowCubeObject.transform.position = Vector3.Lerp(s_HollowCubeStartPosition, s_HollowCubeDestinationPosition.Value, s_CubeMovementTimer / s_TimeCubeMovementShouldTake);
            
            if((s_HollowCubeObject.transform.position - s_HollowCubeDestinationPosition.Value).magnitude <= 0.01f) 
            {
                s_HollowCubeDestinationPosition = null;
                s_TimerToMoveCube = Random.Range(MIN_TIME_TO_MOVE_CUBE_AGAIN, MAX_TIME_TO_MOVE_CUBE_AGAIN);
            }
        }
    }

    private void UpdateSlider()
    {

        if (s_ShouldIncrementSliderValue)
        {
            s_ReelingSlider.value += Time.fixedDeltaTime / 3.0f;
        }
        else
        {
            s_ReelingSlider.value -= Time.fixedDeltaTime / 5.0f;
        }


        if (s_ReelingSlider.value >= s_ReelingSlider.maxValue) {

            m_FishCaughtSound.PlayOneShot(m_FishCaughtSound.clip);
            DisplayUI(false);
            MovementBehavior.s_IsReeling = false;
            Destroy(MovementBehavior.s_ReeledFish);
            MovementBehavior.s_ReeledFish = null;
            s_ProgressSlider.value += 2.0f;
        
        } else if(s_ReelingSlider.value <= s_ReelingSlider.minValue) {

            m_FishLostSound.PlayOneShot(m_FishLostSound.clip);
            DisplayUI(false);
            MovementBehavior.s_IsReeling = false;
            MovementBehavior.s_ReeledFish.GetComponent<FishScript>().isCaught = false;
            MovementBehavior.s_ReeledFish = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            s_ShouldIncrementSliderValue = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            s_ShouldIncrementSliderValue = false;
        }
    }
}
