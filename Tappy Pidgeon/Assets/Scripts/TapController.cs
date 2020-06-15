using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TapController : MonoBehaviour
{
    public delegate void PlayerDelegate();
    public static event PlayerDelegate OnPlayerDied;
    public static event PlayerDelegate OnPlayerScored;

    public float tapForce = 10;
    public float tilt = 5;
    public Vector3 startPosition;

    public AudioSource tapAudio;
    public AudioSource scoreAudio;
    public AudioSource dieAudio;

    Rigidbody2D rigidBody;
    Quaternion downRotation;                            //rotation bird will be descending towards when not tapped
    Quaternion fowardRotation;                          //rotation the bird will snap to when tapped

    GameManager game;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0,0,0);       //Vector3 into a Quaternion, -90 on Z axis to face bird down
        fowardRotation = Quaternion.Euler(0, 0, 35);    //35 on Z axis to snap him back up at an angle 
        game = GameManager.Instance;
        rigidBody.simulated = false;
    }

    private void OnEnable()
    {
        GameManager.OnGameStarted += OnGameStarted;
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= OnGameStarted;
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameStarted()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.simulated = true;
    }

    void OnGameOverConfirmed()
    {
        transform.localPosition = startPosition;
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (game.GameOver) return;

        if (Input.GetMouseButtonDown(0))                //0 = left click, 1 = right click (0 translates to a tap on mobile devices)
        {
            tapAudio.Play();
            transform.rotation = fowardRotation;
            rigidBody.velocity = Vector3.zero;          //at each tap, remove velocity, because if velocity was too high, the tap would lose a lot of strength
            rigidBody.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
        }
        //transform.rotation IS a Quaternion and not a Vector3
        //Lerp means we're going from a source value to a target value over a certain amount of time
        //1st param: source value, 2nd param: target value, 3rd param: how fast we're gonna move towards that target
        transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tilt * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "ScoreZone")     //register a score event
        {
            OnPlayerScored();                           //event sent to GameManager
            scoreAudio.Play();
        }
        if (collision.gameObject.tag == "DeadZone")     //register a death event
        {
            rigidBody.simulated = false;                //stops applying physics to the bird, so it freezes him where he stands
            OnPlayerDied();                             //event sent to GameManager
            dieAudio.Play();
        }
    }
}
