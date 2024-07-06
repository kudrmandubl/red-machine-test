using System;
using Camera;
using UnityEngine;
using Utils.Singleton;


namespace Player.ActionHandlers
{
    public class ClickHandler : DontDestroyMonoBehaviourSingleton<ClickHandler>
    {
        [SerializeField] private float clickToDragDuration;

        public event Action<Vector3> PointerDownEvent;
        public event Action<Vector3> ClickEvent;
        public event Action<Vector3> PointerUpEvent;
        public event Action<Vector3> DragStartEvent;
        public event Action<Vector3> DragEvent;
        public event Action<Vector3> DragEndEvent;

        private Vector3 _pointerDownPosition;
        private Vector3 _pointerLastDragPosition;

        private bool _isClick;
        private bool _isDrag;
        private float _clickHoldDuration;


        private void Update()
        {
            if ((Application.isEditor && Input.GetMouseButtonDown(0))
                || (!Application.isEditor && Input.touchCount > 0))
            {
                _isClick = true;
                _clickHoldDuration = .0f;

                _pointerDownPosition = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(GetPointerPosition());
                
                PointerDownEvent?.Invoke(_pointerDownPosition);
                
                _pointerDownPosition = new Vector3(_pointerDownPosition.x, _pointerDownPosition.y, .0f);
            }
            else if ((Application.isEditor && Input.GetMouseButtonUp(0))
                || (!Application.isEditor && (_isClick || _isDrag) && Input.touchCount == 0))
            {
                var pointerUpPosition = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(GetPointerPosition());
                    
                if (_isDrag)
                {
                    DragEndEvent?.Invoke(pointerUpPosition);

                    _isDrag = false;
                }
                else
                {
                    ClickEvent?.Invoke(pointerUpPosition);
                }
                
                PointerUpEvent?.Invoke(pointerUpPosition);

                _isClick = false;
            }

            if (_isDrag)
            {
                var pointerPosition = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(GetPointerPosition());
                Vector3 deltaDrag = _pointerLastDragPosition - pointerPosition;
                deltaDrag.z = 0;
                _pointerLastDragPosition = pointerPosition;
                DragEvent?.Invoke(deltaDrag);
            }
        }

        private void LateUpdate()
        {
            if (!_isClick)
                return;

            _clickHoldDuration += Time.deltaTime;
            if (_clickHoldDuration >= clickToDragDuration)
            {
                DragStartEvent?.Invoke(_pointerDownPosition);

                _pointerLastDragPosition = _pointerDownPosition;
                _isClick = false;
                _isDrag = true;
            }
        }

        private Vector3 GetPointerPosition()
        {
            if (Application.isEditor || (!Application.isEditor && Input.touchCount == 0))
            {
                return Input.mousePosition;
            }
            return Input.touches[0].position;
        }

        public void SetDragEventHandlers(Action<Vector3> dragStartEvent, Action<Vector3> dragEndEvent)
        {
            ClearEvents();

            DragStartEvent = dragStartEvent;
            DragEndEvent = dragEndEvent;
        }

        public void SetDragEventHandlers(Action<Vector3> dragEvent)
        {
            ClearDragEvents();

            DragEvent = dragEvent;
        }


        public void ClearEvents()
        {
            DragStartEvent = null;
            DragEndEvent = null;
        }

        public void ClearDragEvents()
        {
            DragEvent = null;
        }
    }
}