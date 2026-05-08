using UnityEngine;

public class EleanorController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Gets horizontal input (arrow keys or WASD)
        float move = Input.GetAxis("Horizontal");

        // Flips sprite direction based on movement
        if (move > 0) transform.localScale = new Vector3(1, 1, 1);
        if (move < 0) transform.localScale = new Vector3(-1, 1, 1);

        // Switches animation
        animator.SetBool("IsWalking", move != 0);
    }
}
