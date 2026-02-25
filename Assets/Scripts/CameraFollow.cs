using System;
using UnityEditor.ShaderGraph.Internal;
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

        _leftEdge = transform.position.x - 7.5f;
        if (_target == null) return;

        if (_target.position.x < _leftEdge) {
            _target.position = new Vector3(_leftEdge, _target.position.y, _target.position.z);
        }

        float thresholdWorldX = _camX + _followX;
        
        if (_target.position.x > thresholdWorldX) {
            float newCamX = _target.position.x - _followX;
            newCamX = Mathf.Clamp(newCamX, 0f, _limitX);
            _camX = Mathf.Lerp(_camX, newCamX, _smoothSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(_camX, transform.position.y, transform.position.z);      
    }
}
