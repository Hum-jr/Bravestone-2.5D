using Godot;

namespace Platformer.Scripts;

public partial class Player : CharacterBody3D 
{
    public const float InitialSpeed = 2.0f;
    public const float MaxSpeed = 6.0f;
    public const float Acceleration = 5.0f;
    public const float MaxAcceleration = 6.0f;
    public const float JumpVelocity = 7.0f;
    public Vector3 gravity = new Vector3(0, -12.0f, 0);

    private float currentSpeed;
    private int jumpCount = 0;
    private const int MaxJumps = 2;
    
    // Reference to the animation player
    private AnimationPlayer animPlayer;

    public override void _Ready()
    {
        currentSpeed = InitialSpeed;
        
        // Cache the animation player for better performance
        animPlayer = GetNode<AnimationPlayer>("player/AnimationPlayer");
        
        // Ensure idle animation plays on start
        PlayAnimation("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        
        velocity.X = 0;
        
        // Apply gravity
        if (!IsOnFloor())
        {
            velocity += gravity * (float)delta;
            
            // Play air animation when not on floor
            if (animPlayer.CurrentAnimation != "jump")
            {
                PlayAnimation("jump");
            }
        }
        
        

        // Reset jump count when on the floor
        if (IsOnFloor())
        {
            jumpCount = 0;
        }

        // Handle forward and backward movement with acceleration
        float inputDirection = Input.GetActionStrength("back") - Input.GetActionStrength("foward");
        
        if (inputDirection != 0)
        {
            Node3D player = GetNode<Node3D>("player");
            float accelerationThisFrame = Acceleration * (float)delta;
            accelerationThisFrame = Mathf.Min(accelerationThisFrame, MaxAcceleration * (float)delta);
            
            currentSpeed += accelerationThisFrame;
            currentSpeed = Mathf.Clamp(currentSpeed, InitialSpeed, MaxSpeed);
            velocity.Z = inputDirection * currentSpeed;
            
            // Rotate player based on movement direction
            if (inputDirection < 0) 
                player.Rotation = new Vector3(Rotation.X, Mathf.DegToRad(180), Rotation.Z);
            else if (inputDirection > 0) 
                player.Rotation = new Vector3(Rotation.X, 0, Rotation.Z);
            
            // Play run animation only when on floor
            if (IsOnFloor())
                PlayAnimation("run");
        }
        else
        {
            // Decelerate to stop
            if (IsOnFloor())
            {
                PlayAnimation("rest");
            }
            
            currentSpeed = InitialSpeed;
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, 0.5f);
        }
        
        // Handle jumping
        if (Input.IsActionJustPressed("jump") && jumpCount < MaxJumps)
        {
            // Play jump animation
            PlayAnimation("jump");
            
            velocity.Y = JumpVelocity;
            jumpCount++;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    // Helper method to play animations safely
    private void PlayAnimation(string animationName)
    {
        if (animPlayer == null) return;
        
        // Prevent replaying the same animation
        if (animPlayer.CurrentAnimation != animationName)
        {
            animPlayer.Play(animationName);
        }
    }
}