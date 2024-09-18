using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothnessAnimator : MonoBehaviour
{
    public AnimationCurve Smoothness;

    Material _material;
    float _curTime = 0;
    float _timeFrom = 0.0f;
    float _timeTo = 1.0f;
    float _period = 1.0f;

    void OnEnable()
    {
        var mr = GetComponent<MeshRenderer>();
        _material = new Material(mr.sharedMaterial);
        mr.sharedMaterial = _material;

        _curTime = 0.0f;

        var keys = Smoothness.keys;
        var lastIdx = keys.Length - 1;
        _timeFrom = keys[0].time;
        _timeTo = keys[lastIdx].time;
        _period = _timeTo - _timeFrom;
        _curTime = _timeFrom;
    }

    // Update is called once per frame
    void Update()
    {
        _curTime += Time.deltaTime;
        while (_curTime > _timeTo)
        {
            _curTime -= _period;
        }

        var smoothness = Smoothness.Evaluate(_curTime);
        _material.SetFloat("_Smoothness", smoothness);

    }
}
