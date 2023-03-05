using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bridges.Scripts.Gameplay
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private ObstacleConfig config;

        private float _speed;
        private bool _moving;
        private List<EObstacleType> _types = new();
        private List<EObstacleType> _typesLeft = new();
        private Vector2 _screenBounds;
        private float _edgePercent;
        private float _height;
        private ObstacleConfigData _config;

        private void Start()
        {
            foreach (EObstacleType e in Enum.GetValues(typeof(EObstacleType)))
            {
                _types.Add(e);
            }

            _typesLeft = new(_types);
            _height = spriteRenderer.bounds.size.y;
            NextIcon();
        }

        public void Init(Vector2 screenBounds, float edgePercent)
        {
            _screenBounds = screenBounds;
            _edgePercent = edgePercent;
        }

        public void SetColorSprite()
        {
            if (_config != null)
            {
                iconRenderer.sprite = _config.color;
            }
        }

        public void StartMoving(float speed)
        {
            //if obstacle is up then set speed to negative to move down
            if (transform.position.y > 1)
                _speed = -speed;
            else
                _speed = speed;

            _moving = true;
        }

        //stop block from moving
        public void StopMoving()
        {
            _moving = false;
        }

        //if moving enabled move block
        private void Update()
        {
            if (_moving)
            {
                var y = transform.position.y;
                var speed = _speed * (y < -_screenBounds.y * _edgePercent || y > _screenBounds.y * _edgePercent
                    ? 4f
                    : 1f);
                
                transform.position += (Vector3.up * (speed * Time.deltaTime)); //move only on the y axis
                
                if (_speed < 0 && transform.position.y < -_screenBounds.y + _height / 2)
                {
                    _speed = -1 * _speed;
                    NextIcon();
                }
                if (_speed > 0 && transform.position.y > _screenBounds.y - _height / 2)
                {
                    _speed = -1 * _speed;
                    NextIcon();
                }
            }
        }

        //if block is passed the screen than trigger game over
        private void OnBecameInvisible()
        {
            if (_moving)
            {
                // if(_speed)
                if (_speed < 0 && transform.position.y < 0)
                    GameOver();
                else if (_speed > 0 && transform.position.y > 0)
                    GameOver();
            }
        }

        private void NextIcon()
        {
            if (_typesLeft.Count == 0)
            {
                _typesLeft.AddRange(_types);
            }

            var icon = _typesLeft[Random.Range(0, _typesLeft.Count - 1)];
            _typesLeft.Remove(icon);
            _config = config.GetFor(icon);
            if (_config != null)
            {
                iconRenderer.sprite = _config.line;
            }
        }

        //game over call
        private void GameOver()
        {
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }
    }
}
