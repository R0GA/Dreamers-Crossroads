using UnityEngine;

public class BallRandom : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Animator animator = GetComponent<Animator>();

        float offset = Random.Range(0f, 10f);
        animator.Play("Ball_Gyrate", 0, offset);
    }
}