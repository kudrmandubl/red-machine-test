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

        private ClickHandler _clickHandler;
        private Transform _cameraTransform;
        private Vector3 _startCameraPosition;

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
        }

        private void MoveCamera(Vector3 deltaDrag)
        {
            if (!_canMove)
            {
                return;
            }

            float sign = -1;
            if (Application.isEditor)
            {
                sign = 1;
            }
            _cameraTransform.position += deltaDrag * sign * _moveSpeed;
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

        private void OnDestroy()
        {
            _clickHandler.ClearDragEvents();

            EventsController.Unsubscribe<EventModels.Game.NodeTapped>(SetMoveImpossible);
            EventsController.Unsubscribe<EventModels.Game.PlayerFingerRemoved>(SetMovePossible);

            ScenesChanger.SceneLoadedEvent -= Reinit;
        }
    }
}
