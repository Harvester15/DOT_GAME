using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;

namespace dot_battle_signal_bus.scripts.Systems
{
    public partial class CameraManager : Node
    {

         public Camera2D MainCamera;
        [Export] public float MoveSpeed = 5f;
        [Export] public float ZoomSpeed = 0.1f;

        private Vector2 _targetPosition;
        private float _targetZoom = 1.0f;
        private BaseUnit _followTarget;

        private GridManager _gridManager;
        private GridPathfinding _gridPathfinding => GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");

        private static CameraManager _instance;
        private static Sprite2D _spriteMap;
        private bool _isDragging = false;
        private Rect2 _mapLimits;

        public static CameraManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CameraManager();
                }
                return _instance;
            }
        }

        public override void _Ready()
        {
            GD.Print("CameraManager ready");
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                QueueFree(); // Ensure only one instance exists
            }

            _gridManager = GetNode<GridManager>("/root/Main/GameWorld/Map");
            _spriteMap = GetNode<Sprite2D>("/root/Main/GameWorld/Map/Sprite2D");

            MainCamera = GetTree().Root.GetNode<Camera2D>("/root/Main/CameraRig/Camera2D");
            MainCamera.Enabled = true;
            MainCamera.MakeCurrent();
            var mapSize = _spriteMap.Texture.GetSize();
            MainCamera.GlobalPosition = new Vector2(_spriteMap.GlobalPosition.X + mapSize.X/2, _spriteMap.GlobalPosition.Y + mapSize.Y/2) ;
            //MainCamera.GlobalPosition = new Vector2(_gridManager.GridWidth, _gridManager.GridWidth);
            MainCamera.Zoom = new Vector2(1.9f, 1.9f);
            GD.Print("CameraManager MainCamera position set to: " + MainCamera.GlobalPosition);
            // Set initial position to center of the map or follow a default target
            _targetPosition = MainCamera.GlobalPosition; // Keep current position or set to map center
            _targetZoom = MainCamera.Zoom.X; // Sync with current zoom
            _mapLimits = new Rect2(_spriteMap.GlobalPosition - mapSize / 2, mapSize);
            GD.Print("CameraManager MainCamera: " + MainCamera.Name);

        }

        //add _input handling to zoom in and out 
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                //if mouse wheel is pressed, use mouse movement to move the camera
                if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
                {
                    //TODO add camera movement?
                    //_targetPosition = GetViewport().GetMousePosition();
                    //GD.Print("Mouse wheel down pressed, target position: " + _targetPosition);
                }


                if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
                {
                    _targetZoom = Mathf.Clamp(_targetZoom - ZoomSpeed, 0.02f, 3f);
                }
                if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
                {
                    _targetZoom = Mathf.Clamp(_targetZoom + ZoomSpeed, 0.02f, 3f);
                }
                if (mouseEvent.ButtonIndex == MouseButton.Middle)
                {
                    _targetPosition = _gridPathfinding.GetLocalMousePosition();
                }

                if (_isDragging && @event is InputEventMouseMotion mouseMotion)
                {
                    // Adjust sensitivity by current zoom so it feels 1:1 with the cursor
                    _targetPosition -= mouseMotion.Relative / MainCamera.Zoom;

                    // Clamp the target so you can't drag the "view" outside the map
                    _targetPosition.X = Mathf.Clamp(_targetPosition.X, _mapLimits.Position.X, _mapLimits.End.X);
                    _targetPosition.Y = Mathf.Clamp(_targetPosition.Y, _mapLimits.Position.Y, _mapLimits.End.Y);
                }

            }



            if (Input.IsActionJustPressed("camera"))
            {
                MainCamera.MakeCurrent();
                GD.Print("CameraManager MainCamera: " + MainCamera.Name);
            }
        }

        //make the _targetZoom work on the maincamera
        public override void _Process(double delta)
        {
            float weight = (float)delta * MoveSpeed;

            
            //why do we need this?
            //if (_followTarget != null)
            //{
            //    _targetPosition = _followTarget.GlobalPosition;
            //}
            //GD.Print($"CameraManager _Process  - {this}: TargetPosition {_targetPosition}, TargetZoom {_targetZoom}");
            MainCamera.GlobalPosition = _targetPosition;
            // Smoothly interpolate the camera's position towards the target position
            //MainCamera.Position = MainCamera.Position.Lerp(_targetPosition, MoveSpeed * (float)delta);
            // Smoothly interpolate the camera's zoom towards the target zoom
            //MainCamera.Zoom = MainCamera.Zoom.Lerp(new Vector2(_targetZoom, _targetZoom), MoveSpeed * (float)delta);
            MainCamera.Zoom = new Vector2(_targetZoom, _targetZoom);
        }

        public void FocusOnUnit(BaseUnit unit)
        {
            GD.Print("CameraManager FocusOnUnit: " + unit.UnitName);
            _followTarget = unit;
            _targetPosition = unit.GlobalPosition;
            MainCamera.Zoom = new Vector2(1.0f, 1.0f); // Reset zoom to default when focusing on a unit
            GD.Print($"CameraManager FocusOnUnit - {this}: TargetPosition {_targetPosition}, from unit {unit.Position}, and clicked {MainCamera.GetGlobalMousePosition()}");
        }



    }
}
