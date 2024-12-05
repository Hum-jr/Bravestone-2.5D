using Godot;
using System;
using System.Diagnostics;

public partial class Enemy : CharacterBody3D
{
    [Export]
    public float Speed = 2.0f;

    [Export]
    public float DetectionRange = 10.0f;

    [Export]
    public float Gravity = -30.0f;

    private RayCast3D leftWallRaycast;
    private RayCast3D rightWallRaycast;
    private RayCast3D leftFloorRaycast;
    private RayCast3D rightFloorRaycast;
    private bool movingRight = true;
    private Vector3 velocity;

    public override void _Ready()
    {
        // Setup wall detection raycasts
        leftWallRaycast = new RayCast3D
        {
            TargetPosition = new Vector3(0, 0, -1.0f),
            CollisionMask = 1
        };
        AddChild(leftWallRaycast);

        rightWallRaycast = new RayCast3D
        {
            TargetPosition = new Vector3(0, 0, 1.0f),
            CollisionMask = 1
        };
        AddChild(rightWallRaycast);

        // Setup floor detection raycasts
        leftFloorRaycast = new RayCast3D
        {
            TargetPosition = new Vector3(0, -1.5f, -0.5f),
            CollisionMask = 1
        };
        AddChild(leftFloorRaycast);

        rightFloorRaycast = new RayCast3D
        {
            TargetPosition = new Vector3(0, -1.5f, 0.5f),
            CollisionMask = 1
        };
        AddChild(rightFloorRaycast);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement(delta);
        Velocity = velocity;
        MoveAndSlide();
    }

    private void HandleMovement(double delta)
    {
        // Apply gravity
        if (!IsOnFloor())
            velocity.Y += Gravity * (float)delta;
        else
            velocity.Y = 0;

        // Update all raycasts
        leftWallRaycast.ForceRaycastUpdate();
        rightWallRaycast.ForceRaycastUpdate();
        leftFloorRaycast.ForceRaycastUpdate();
        rightFloorRaycast.ForceRaycastUpdate();

        bool leftWall = leftWallRaycast.IsColliding();
        bool rightWall = rightWallRaycast.IsColliding();
        bool leftFloor = leftFloorRaycast.IsColliding();
        bool rightFloor = rightFloorRaycast.IsColliding();

        // Check for holes based on movement direction
        bool holeAhead = movingRight ? !rightFloor : !leftFloor;

        // Change direction if wall is detected or there's a hole ahead
        if ((movingRight && (rightWall || holeAhead)) || 
            (!movingRight && (leftWall || holeAhead)))
        {
            movingRight = !movingRight;
            GD.Print($"Changing direction! Moving right: {movingRight} (Wall: {(rightWall || leftWall)}, Hole: {holeAhead})");
        }

        // Apply horizontal movement based on direction
        velocity.Z = movingRight ? Speed : -Speed;
    }

    // Debug method - call this from the editor to check raycast states

}