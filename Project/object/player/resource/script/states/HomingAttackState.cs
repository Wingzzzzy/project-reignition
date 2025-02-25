using Godot;
using Project.Core;

namespace Project.Gameplay;

public partial class HomingAttackState : PlayerState
{
	[Export]
	private PlayerState landState;
	[Export]
	private PlayerState stompState;
	[Export]
	private PlayerState jumpDashState;

	[Export]
	private float normalStrikeSpeed;
	[Export]
	private float perfectStrikeSpeed;
	[Export]
	private float homingAttackAcceleration;

	private bool IsPerfectHomingAttack { get; set; }

	public override void EnterState()
	{
		Player.VerticalSpeed = 0;
		Player.IsMovingBackward = false;
		Player.IsHomingAttacking = true;
		Player.ChangeHitbox("spin");
		Player.AttackState = PlayerController.AttackStates.Weak;

		IsPerfectHomingAttack = Player.Lockon.IsMonitoringPerfectHomingAttack;
		if (IsPerfectHomingAttack)
		{
			Player.Lockon.PlayPerfectStrike();
			Player.AttackState = PlayerController.AttackStates.Strong;
		}

		Player.Effect.StartSpinFX();
		Player.Effect.PlayActionSFX(Player.Effect.JumpDashSfx);
		Player.Effect.StartTrailFX();
		Player.Effect.StartSpinFX();
		Player.Effect.PlayVoice("grunt");

		Player.Animator.StartSpin(2.0f);
		Player.ChangeHitbox("spin");

		if (SaveManager.ActiveSkillRing.IsSkillEquipped(SkillKey.CrestFire))
			Player.Skills.ActivateFireCrest();
	}

	public override void ExitState()
	{
		Player.IsHomingAttacking = false;
		Player.AttackState = PlayerController.AttackStates.None;
		Player.ChangeHitbox("RESET");
		Player.Lockon.CallDeferred(PlayerLockonController.MethodName.ResetLockonTarget);
		Player.Effect.StopSpinFX();
		Player.Effect.StopTrailFX();
		Player.Animator.ResetState();

		if (!SaveManager.ActiveSkillRing.IsSkillEquipped(SkillKey.CrestFire))
			return;

		if (Player.IsBouncing)
		{
			Player.Skills.CallDeferred(PlayerSkillController.MethodName.ActivateFireCrestBurst);
		}
		else
		{
			Player.Lockon.ResetLockonTarget();
			Player.Skills.DeactivateFireCrest();
		}
	}

	public override PlayerState ProcessPhysics()
	{
		if (Player.Lockon.Target == null) // Target disappeared. Transition to jumpdash
		{
			Player.MovementAngle = Player.PathFollower.ForwardAngle;
			Player.ChangeHitbox("RESET");
			return jumpDashState;
		}

		if (IsPerfectHomingAttack)
			Player.MoveSpeed = Mathf.MoveToward(Player.MoveSpeed, perfectStrikeSpeed, homingAttackAcceleration * 2.0f * PhysicsManager.physicsDelta);
		else
			Player.MoveSpeed = Mathf.MoveToward(Player.MoveSpeed, normalStrikeSpeed, homingAttackAcceleration * PhysicsManager.physicsDelta);
		Player.Velocity = Player.Lockon.HomingAttackDirection.Normalized() * Player.MoveSpeed;
		Player.MovementAngle = ExtensionMethods.CalculateForwardAngle(Player.Lockon.HomingAttackDirection);
		Player.MoveAndSlide();
		Player.CheckGround();
		Player.CheckWall();
		Player.UpdateUpDirection(true);
		Player.PathFollower.Resync();

		bool isColliding = Player.GetSlideCollisionCount() != 0;
		if (isColliding && ProcessEnvironmentCollision())
		{
			Player.StartBounce();
			return null;
		}

		if (Player.Controller.IsActionBufferActive)
		{
			Player.Controller.ResetActionBuffer();
			return stompState;
		}

		return null;
	}

	private bool ProcessEnvironmentCollision()
	{
		// Check from the floor
		Vector3 castOffset = Vector3.Up * Player.CollisionSize.Y * .5f;
		Vector3 castPosition = Player.GlobalPosition + castOffset;
		if (Player.VerticalSpeed < 0)
			castPosition += Player.UpDirection * Player.VerticalSpeed * PhysicsManager.physicsDelta;
		Vector3 castVector = Player.Lockon.Target.GlobalPosition - castPosition;
		RaycastHit hit = Player.CastRay(castPosition, castVector, Runtime.Instance.environmentMask);
		DebugManager.DrawRay(castPosition, castVector, Colors.Magenta);
		return hit && hit.collidedObject.IsInGroup("wall") && !hit.collidedObject.IsInGroup("level wall");
	}
}