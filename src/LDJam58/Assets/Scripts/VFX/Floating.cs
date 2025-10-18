using System;
using UnityEngine;

namespace VFX
{
    public class Floating : MonoBehaviour
    {
        [SerializeField] private float seconds;
        [SerializeField] private float minY;
        [SerializeField] private float maxY;
        [SerializeField] private AnimationCurve curve;

        private float _startingY;
        private float _t;

        private void Start() => _startingY = transform.localPosition.y;
        
        public void Update()
        {
            _t += Time.deltaTime;
            if (_t > seconds)
                _t -= seconds;
            float percent = _t / seconds;
            float point = curve.Evaluate(percent);
            float adjustment = point * (maxY - minY) + minY;
            float newY = _startingY + adjustment;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }
}