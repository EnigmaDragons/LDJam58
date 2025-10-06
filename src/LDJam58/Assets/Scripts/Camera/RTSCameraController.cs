using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// RTS-style top-down camera controller supporting:
    /// - WASD/arrow movement
    /// - Screen-edge scrolling
    /// - Middle-mouse drag panning
    /// - Mouse wheel zoom (orthographic or perspective)
    /// - Optional yaw rotation (Q/E)
    /// - Spacebar to instantly reset to starting position (preserves zoom)
    /// - Smoothing and world-bounds clamping
    /// Attach to a Camera. For top-down, set a tilt (e.g., 60°) and position above ground.
    /// </summary>
    public class RtsCameraController : OnMessage<LockCameraMovement, UnlockCameraMovement, RoomOpened>
    {
        private bool _movementEnabled = true;
        
        [Header("Movement (Planar)")]
        [SerializeField] private float _moveSpeed = 20f;
        [SerializeField] private float _mouseMoveSpeed = 1f;
        [SerializeField] private bool _useEdgeScroll = true;
        [SerializeField, Range(0f, 0.5f)] private float _edgeThicknessPercent = 0.04f; // % of screen

        [Header("Rotation")]
        [SerializeField] private bool _allowRotation = true;
        [SerializeField] private float _rotationSpeed = 90f; // degrees per second, Q/E

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 200f; // wheel delta applied to distance/size
        [SerializeField] private float _minZoom = 10f;     // for perspective: min distance; for ortho: min size
        [SerializeField] private float _maxZoom = 120f;    // for perspective: max distance; for ortho: max size

        [Header("Smoothing")]
        [SerializeField] private float _positionSmoothing = 0.08f; // 0: instant

        [Header("Bounds Offsets")]
        [SerializeField] private float rightReduction = 0;
        [SerializeField] private float leftReduction = 0;
        [SerializeField] private float topReduction = 0;
        [SerializeField] private float bottomReduction = 0;
        [SerializeField] private float xBoundsOffset = 0;
        [SerializeField] private float zBoundsOffset = 0;
        
        private bool _clampToBounds = false;
        private Vector2 _xBounds = new Vector2(-200f, 200f);
        private Vector2 _zBounds = new Vector2(-200f, 200f);

        private Camera _cachedCamera;
        private Vector3 _desiredPosition;
        private float _startZoom;
        private float _desiredZoom; // perspective: desired height; ortho: size
        private bool _hasMoveInputThisFrame;
        private bool _hasZoomInputThisFrame;
        private Vector3 _startingPosition;
        

        private void Awake()
        {
            _cachedCamera = GetComponent<Camera>();
            if (_cachedCamera == null)
                _cachedCamera = Camera.main;

            _startingPosition = transform.position;
            _desiredPosition = transform.position;

            if (_cachedCamera != null && _cachedCamera.orthographic)
            {
                _desiredZoom = Mathf.Clamp(_cachedCamera.orthographicSize, _minZoom, _maxZoom);
            }
            else
            {
                // For perspective, treat desired zoom as desired height
                _desiredZoom = Mathf.Clamp(transform.position.y, _minZoom, _maxZoom);
                _startZoom = _desiredZoom;
            }
        }

        private void Update()
        {
            if (!_movementEnabled)
                return;

            HandleResetInput();
            HandleMovementInput();
            HandleRotationInput();
            HandleZoomInput();
            ApplyDesiredTransform();
        }

        private void HandleResetInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetToStartingPosition();
            }
        }

        private void HandleMovementInput()
        {
            Vector3 inputMove = Vector3.zero;
            _hasMoveInputThisFrame = false;

            // WASD / Arrow keys: move in camera's local XZ plane (ignoring pitch)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            // Derive planar axes from yaw so forward/back still works when looking straight down
            GetPlanarAxes(out Vector3 forward, out Vector3 right);
            inputMove += (forward * vertical + right * horizontal);

            // Edge scrolling (sole input for movement)
            if (_useEdgeScroll)
            {
                Vector2 edgeMove = GetEdgeScrollDirection2D();
                // Use proportional magnitude based on proximity to edges
                inputMove += (forward * edgeMove.y + right * edgeMove.x);
            }

            // Mark if there is any movement input
            _hasMoveInputThisFrame = inputMove.sqrMagnitude > 1e-6f;

            // Apply movement solely when there is input; otherwise freeze desired XZ to prevent drift
            if (_hasMoveInputThisFrame)
            {
                float speed = _moveSpeed;
                _desiredPosition += inputMove * speed * Time.unscaledDeltaTime;
            }
            else
            {
                _desiredPosition.x = transform.position.x;
                _desiredPosition.z = transform.position.z;
            }

            if (_clampToBounds)
            {
                var mult = transform.position.y / _startZoom;
                var adjustedRightReduction = rightReduction * mult;
                var adjustedLeftReduction = leftReduction * mult;
                var adjustedTopReduction = topReduction * mult;
                var adjustedBottomReduction = bottomReduction * mult;
                
                //the below lines work with extremely specific ranges and cannot be trusted
                var zRotationRatio = (90 - transform.rotation.eulerAngles.x) / 45;
                var multChangeAmount = Math.Abs(mult - 1);
                var adjustedZMultChangeAmount = multChangeAmount * zRotationRatio;
                var adjustedOffsetZMult = mult > 1 ? 1 + adjustedZMultChangeAmount : 1 - adjustedZMultChangeAmount;
                var adjustedXBoundsOffset = xBoundsOffset;
                var adjustedZBoundsOffset = zBoundsOffset * adjustedOffsetZMult;

                var minX = _xBounds.x + adjustedXBoundsOffset + adjustedRightReduction;
                var maxX = _xBounds.y + adjustedXBoundsOffset - adjustedLeftReduction;
                if (minX > maxX)
                {
                    var average = (minX + maxX) / 2;
                    minX = average;
                    maxX = average;
                }
                var minY = _zBounds.x + adjustedZBoundsOffset + adjustedBottomReduction;
                var maxY = _zBounds.y + adjustedZBoundsOffset - adjustedTopReduction;
                if (minY > maxY)
                {
                    var average = (minY + maxY) / 2;
                    minY = average;
                    maxY = average;
                }

                _desiredPosition.x = Mathf.Clamp(_desiredPosition.x, minX, maxX);
                _desiredPosition.z = Mathf.Clamp(_desiredPosition.z, minY, maxY);
            }
        }

        private void HandleRotationInput()
        {
            if (!_allowRotation)
                return;

            float rotate = 0f;
            if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
            if (Input.GetKey(KeyCode.E)) rotate += 1f;
            if (Mathf.Abs(rotate) > 0.01f)
            {
                transform.Rotate(Vector3.up, rotate * _rotationSpeed * Time.unscaledDeltaTime, Space.World);
            }
        }

        private void HandleZoomInput()
        {
            float wheel = Input.mouseScrollDelta.y;
            _hasZoomInputThisFrame = Mathf.Abs(wheel) >= 0.001f;
            if (!_hasZoomInputThisFrame)
                return;

            // Use fixed increment regardless of current zoom level
            float zoomIncrement = wheel * _zoomSpeed;
            _desiredZoom = Mathf.Clamp(_desiredZoom - zoomIncrement, _minZoom, _maxZoom);
        }

        private void ApplyDesiredTransform()
        {
            // Zoom application (orthographic and perspective) - no smoothing
            if (_cachedCamera != null && _cachedCamera.orthographic)
            {
                _cachedCamera.orthographicSize = _desiredZoom;
            }
            else
            {
                // For perspective: only adjust Y while zoom input is active; otherwise freeze Y
                _desiredPosition.y = _hasZoomInputThisFrame ? _desiredZoom : transform.position.y;
            }

            // Position smoothing (freeze when neutral: no movement, no zoom)
            bool neutral = !_hasMoveInputThisFrame && !_hasZoomInputThisFrame;
            if (neutral || _positionSmoothing <= 0.0001f)
            {
                transform.position = _desiredPosition;
            }
            else
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    _desiredPosition,
                    1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(0.0001f, _positionSmoothing)));
            }
        }

        private void ResetToStartingPosition()
        {
            // Reset position to starting position while preserving zoom
            var currentZoom = _desiredZoom;
            _desiredPosition = _startingPosition;
            _desiredPosition.y = currentZoom; // Preserve zoom level
            transform.position = _desiredPosition; // Instantly apply without smoothing
        }

    private void GetPlanarAxes(out Vector3 forward, out Vector3 right)
    {
        float yawRad = transform.eulerAngles.y * Mathf.Deg2Rad;
        forward = new Vector3(Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
        right = new Vector3(Mathf.Cos(yawRad), 0f, -Mathf.Sin(yawRad));
    }

        private Vector2 GetEdgeScrollDirection2D()
        {
            if (!Application.isFocused || !MouseScreenCheck())
                return Vector2.zero;

            Vector2 dir = Vector2.zero;
            Vector3 mouse = Input.mousePosition;
            float w = Screen.width;
            float h = Screen.height;
            float pct = Mathf.Clamp01(_edgeThicknessPercent);
            float tX = Mathf.Max(1f, w * pct);
            float tY = Mathf.Max(1f, h * pct);

            if (mouse.x <= tX) dir.x = -_mouseMoveSpeed; // -1..0
            else if (mouse.x >= w - tX) dir.x = _mouseMoveSpeed; // 0..1

            if (mouse.y <= tY) dir.y = -_mouseMoveSpeed; // -1..0 (down)
            else if (mouse.y >= h - tY) dir.y = _mouseMoveSpeed; // 0..1 (up)

            return dir;
        }

        public bool MouseScreenCheck()
        {
#if UNITY_EDITOR
            if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 ||
                Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 ||
                Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1)
            {
                return false;
            }
#else
        if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) {
        return false;
        }
#endif
            else
            {
                return true;
            }
        }

        // Public API
        public void SetBounds(Vector2 xRange, Vector2 zRange)
        {
            _xBounds = xRange;
            _zBounds = zRange;
            _clampToBounds = true;
        }

        protected override void Execute(LockCameraMovement msg)
        {
            _movementEnabled = false;
            Debug.Log("LockCameraMovement");
        }

        protected override void Execute(UnlockCameraMovement msg)
        {
            _movementEnabled = true;
            Debug.Log("UnlockCameraMovement");
        }

        private List<Bounds> _roomBounds = new List<Bounds>();

        protected override void Execute(RoomOpened msg)
        {
            _roomBounds.Add(msg.Bounds);
            var bound = _roomBounds[0];
            foreach (var room in _roomBounds.Skip(1))
                bound.Encapsulate(room);
            SetBounds(new Vector2(bound.min.x, bound.max.x), new Vector2(bound.min.z, bound.max.z));
        }
    }
}


