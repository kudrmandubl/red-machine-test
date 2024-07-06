using Connection;
using Events;
using Player.ActionHandlers;
using UnityEngine;
using Utils.Scenes;
using Utils.Singleton;

namespace Camera
{
    public class CameraMover : DontDestroyMonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 0.8f;
        [SerializeField] private Vector3 _extraBorder = new Vector3(1f, 1f, 0f);

        private ClickHandler _clickHandler;
        private Transform _cameraTransform;
        private Vector3 _startCameraPosition;

        private Vector3 _minPosition;
        private Vector3 _maxPosition;

        private bool _canMove = true;

        protected override void Awake()
        {
            base.Awake();

            _clickHandler = ClickHandler.Instance;
            _clickHandler.SetDragEventHandlers(MoveCamera);

            _cameraTransform = transform;
            _startCameraPosition = _cameraTransform.position;

            EventsController.Subscribe<EventModels.Game.NodeTapped>(this, SetMoveImpossible);
            EventsController.Subscribe<EventModels.Game.PlayerFingerRemoved>(this, SetMovePossible);

            ScenesChanger.SceneLoadedEvent += Reinit;
            ColorConnectionManager.OnNodesInited += SetBorders;
        }

        private void MoveCamera(Vector3 deltaDrag)
        {
            if (!_canMove)
            {
                return;
            }

            Vector3 targetPosition = _cameraTransform.position + deltaDrag;
            targetPosition.x = Mathf.Clamp(targetPosition.x, _minPosition.x, _maxPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, _minPosition.y, _maxPosition.y);
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, targetPosition, Time.deltaTime * _moveSpeed);
        }

        private void SetMoveImpossible(EventModels.Game.NodeTapped data)
        {
            _canMove = false;
        }

        private void SetMovePossible(EventModels.Game.PlayerFingerRemoved data)
        {
            _canMove = true;
        }

        private void Reinit()
        {
            _cameraTransform.position = _startCameraPosition;
        }

        private void SetBorders(ColorNode[] nodes)
        {
            _minPosition = nodes[0].transform.position;
            _maxPosition = _minPosition;
            for (int i = 1; i < nodes.Length; i++)
            {
                Vector3 nodePosition = nodes[i].transform.position;
                if(nodePosition.x < _minPosition.x)
                {
                    _minPosition.x = nodePosition.x;
                }
                if (nodePosition.x > _maxPosition.x)
                {
                    _maxPosition.x = nodePosition.x;
                }

                if (nodePosition.y < _minPosition.y)
                {
                    _minPosition.y = nodePosition.y;
                }
                if (nodePosition.y > _maxPosition.y)
                {
                    _maxPosition.y = nodePosition.y;
                }
            }
            _minPosition -= _extraBorder;
            _maxPosition += _extraBorder;
        }

        private void OnDestroy()
        {
            _clickHandler.ClearDragEvents();

            EventsController.Unsubscribe<EventModels.Game.NodeTapped>(SetMoveImpossible);
            EventsController.Unsubscribe<EventModels.Game.PlayerFingerRemoved>(SetMovePossible);

            ScenesChanger.SceneLoadedEvent -= Reinit;
            ColorConnectionManager.OnNodesInited -= SetBorders;
        }
    }
}
