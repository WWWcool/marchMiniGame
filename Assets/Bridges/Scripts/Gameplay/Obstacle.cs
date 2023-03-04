using UnityEngine;

public class Obstacle : MonoBehaviour
{
    float speed;
    bool moving;

    public void StartMoving(float _speed)
    {
        //if obstacle is up then set speed to negative to move down
        if (transform.position.y > 1)
            speed = -_speed;
        else
            speed = _speed;

        moving = true;
    }

    //stop block from moving
    public void StopMoving()
    {
        moving = false;
    }

    //if moving enabled move block
    void Update()
    {
        if (moving)
        {
            transform.position = transform.position + (Vector3.up * (speed * Time.deltaTime)); //move only on the y axis
        }
    }

    //if block is passed the screen than trigger game over
    void OnBecameInvisible()
    {
        if (moving)
        {
            if (speed < 0 && transform.position.y < 0)
                GameOver();
            else if (speed > 0 && transform.position.y > 0)
                GameOver();
        }
    }

    //game over call
    void GameOver()
    {
        GameManager.Instance.GameOver();
        Destroy(gameObject);
    }
}
