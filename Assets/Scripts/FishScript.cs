using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    private const float TIME_PER_SPEED_INCREMENT = 0.01f;
    private const float MIN_TIME_TO_CATCH = 1.0f;
    private static float s_MaxSpeed = 0.0f;

    public int myRandValue;
    public float catchTime; 
    public Vector2 swimVector;
    public float swimSpeed;
    public bool isCaught = false;
    public Vector3 caughtPosition = Vector3.zero;

    private const int X_BOUNDARY = 20;
    public AudioClip sound;

    private void Awake()
    {
        if (swimSpeed >= s_MaxSpeed)
        {
            s_MaxSpeed = swimSpeed;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        catchTime = MIN_TIME_TO_CATCH + (s_MaxSpeed - swimSpeed) * TIME_PER_SPEED_INCREMENT;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(gameObject.transform.position.x) > X_BOUNDARY ) {
            Destroy(gameObject); return;
        }
        else if (isCaught) {
            GetComponent<AudioSource> ().Play ();
            return;
        }

        gameObject.GetComponent<Rigidbody2D>().AddForce(swimVector * swimSpeed * Time.deltaTime);
    }
}
