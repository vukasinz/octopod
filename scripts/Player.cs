using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]public  float Speed = 250.0f;
	[Export] public float Energy = 1000.0f;
	public  float JumpVelocity = -300.0f;
	
	private AnimationTree _animationTree;
	private Sprite2D _player;
	private ProgressBar _energyBar;
	private Timer _dashTimer;
	private Timer _jumpTimer;
	private Timer _hitTimer;
	public bool _jumping = false;
	public bool _dashing = false;
	public bool _hitting = false;
	public bool _canDash = true;
	public bool _gravityDisabled = false;
	public double _dashCooldown = 1f;
	private Vector2 dashVelocity = Vector2.Zero;
	

	public override void _Ready()
	{
		_player = GetNode<Sprite2D>("sprite2d");
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true; ;
		_dashTimer = GetNode<Timer>("sprite2d/dashTimer");
		_dashTimer.Connect("timeout", new Callable(this, nameof(OnDashTimerTimeout)));
		_jumpTimer = GetNode<Timer>("sprite2d/jumpTimer");
		_jumpTimer.Connect("timeout", new Callable(this, nameof(OnJumpTimerTimeout)));
		_hitTimer = GetNode<Timer>("sprite2d/hitTimer");
		_hitTimer.Connect("timeout", new Callable(this, nameof(OnHitTimerTimeout)));
		_energyBar = GetNode<ProgressBar>("../UI/ProgressBar");

	}

	private void OnHitTimerTimeout()
	{
		_hitting = false;
		_animationTree.CallDeferred("set","parameters/conditions/hit", false);
		_animationTree.CallDeferred("set","parameters/conditions/not_hit", true);

	}
	private void OnJumpTimerTimeout()
	{
		_jumping = false;
		_animationTree.CallDeferred("set","parameters/conditions/jump", false);	}
	private void OnDashTimerTimeout()
	{
		_gravityDisabled = false;
		_dashing = false;
		_animationTree.CallDeferred("set","parameters/conditions/dash", false);
		
	}
	public void _useDash(Vector2 direction)
	{
		if(Energy >= 30.0f)
		{
			_gravityDisabled = true;
			Energy -= 30.0f;
			_dashTimer.Start(0.37f);
			_canDash = false;
			_animationTree.CallDeferred("set","parameters/conditions/dash", true);
			_dashCooldown = 1;
			_dashing = true;
		}
	}

	public void _useHitting()
	{
		_hitting = true;
		_hitTimer.Start(0.3f);
		_animationTree.CallDeferred("set","parameters/conditions/hit", true);
				_animationTree.CallDeferred("set","parameters/conditions/not_hit", false);

		/*	var result = (GD.Randf() < 0.5f) ? 1 : -1;
				_animationTree.Set("parameters/hits/blend_position",result);*/
	}
	public override void _PhysicsProcess(double delta)
	{
		if(Energy >= 100f)
			Energy = 100f;
		
		_energyBar.Value = Energy;
		Vector2 velocity = Velocity;
		
		if (!IsOnFloor()) {
			// Apply gravity, adjust based on _gravityDisabled
			float gravity_scale = _gravityDisabled ? 0.5f : 1.0f;
			velocity += GetGravity() * gravity_scale * (float)delta;
		}
		//jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			_animationTree.CallDeferred("set","parameters/conditions/jump", true);
			_jumpTimer.Start(0.23f);
		}
		//hit
		if (Input.IsActionJustPressed("hit"))
		{
			_useHitting();
		}
		//dash
		if (!_canDash)
			_dashCooldown -= delta;
		if (_dashCooldown <= 0)
			_canDash = true;
		if (_dashing)
			Speed = 500f;
		else
		{
			Energy += 5*(float)delta;
			Speed = 250f;
		}

		//input and move
		 Vector2 direction = Input.GetVector("move_left", "move_right", "move_down", "move_up");
		 if (IsOnFloor())
			 _animationTree.CallDeferred("set","parameters/move/blend_position", direction.X);
		 else
			 _animationTree.CallDeferred("set","parameters/move/blend_position", 0);

		 if (Input.IsActionJustPressed("move_dash") && _dashing == false && _canDash == true)
			_useDash(direction);
	
		if (direction.X == -1)
			_player.FlipH = true;
		else if(direction.X == 1)
			_player.FlipH = false;
		if (velocity.X == 0 && _dashing == true)
		{
			if (_player.FlipH == true)
				velocity.X = -Speed;
			else
				velocity.X = Speed;
		}
		else
			velocity.X = direction.X * Speed;
		Velocity = velocity;
		MoveAndSlide();
	}
}
