using System;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothSpeed = 50f;
    [SerializeField] private float _followX = -2.5f;
    [SerializeField] private float _limitX = 197f;

    private float _camX;
    private float _leftEdge;

    void Start() {
        _camX = transform.position.x;
        _leftEdge = transform.position.x - 7.5f;
    }

    void LateUpdate() {

        // check if flagpole conflicts
        switch (StateManager.CurrentState) {
            case StateManager.GameState.Won:
            case StateManager.GameState.Play:
                break;
            default:
                return;
        }

        _leftEdge = transform.position.x - 7.5f;
        if (_target == null) return;

        Rigidbody2D rb = _target.GetComponent<Rigidbody2D>();

        if (_target.position.x < _leftEdge)
        {
            Vector2 pos = rb.position;
            pos.x = _leftEdge;
            rb.position = pos;

            if (rb.linearVelocity.x < 0)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        float thresholdWorldX = _camX + _followX;
        
        if (_target.position.x > thresholdWorldX) {
            float newCamX = _target.position.x - _followX;
            newCamX = Mathf.Clamp(newCamX, 0f, _limitX);
            _camX = Mathf.Lerp(_camX, newCamX, _smoothSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(_camX, transform.position.y, transform.position.z);      
    }

    public void SetCamX(float x) {
        _camX = x;
    }

    public void SetLeftEdge(float x) {
        _leftEdge = x;
    }
}
