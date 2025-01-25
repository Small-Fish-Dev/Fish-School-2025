using Sandbox;

public sealed class SnotPlayerComponent : Component
{

	[Property]
	[Category( "Components" )]
	public PlayerController Controller { get; set; }

	[Property]
	[Category( "Components" )]
	public SkinnedModelRenderer ModelRenderer { get; set; }

	[Property]
	[Category( "Components" )]
	public UnitComponent UnitComponent { get; set; }

	[Property]
	[Category( "Stats" )]
	[Range( 50f, 200f, 10f )]
	public float PunchRange { get; set; } = 100f;

	[Property]
	[Category( "Stats" )]
	[Range( 5f, 50f, 5f )]
	public float PunchDamage { get; set; } = 10f;

	[Property]
	[Category( "Stats" )]
	[Range( 0f, 2f, 0.1f )]
	public float PunchCooldown { get; set; } = 0.5f;

	public TimeUntil NextPunch;
	private ModelPhysics _ragdoll;
	private Vector3 _spawnPosition;
	private TimeUntil _resetPose;

	protected override void OnStart()
	{
		_spawnPosition = WorldPosition;
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Down( "attack1" ) && NextPunch )
		{
			Punch();
			NextPunch = PunchCooldown;
		}

		if ( _resetPose )
			ModelRenderer.Set( "holdtype", 0 );
	}

	public void Punch()
	{
		ModelRenderer.Set( "holdtype", 5 );
		ModelRenderer.Set( "b_attack", true );
		_resetPose = 3f;

		var punchDirection = Controller.EyeAngles.Forward;
		var punchStart = Controller.EyePosition;
		var punchEnd = punchStart + punchDirection * PunchRange;

		var punchTrace = Scene.Trace.Ray( punchStart, punchEnd )
			.Radius( 20f )
			.WithoutTags( "player" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !punchTrace.Hit ) return;
		if ( !punchTrace.GameObject.Components.TryGet<UnitComponent>( out var unit ) ) return;
		if ( unit.Team == UnitComponent.Team ) return;

		unit.Damage( PunchDamage );
	}

	[Button]
	public void Ragdoll()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( _ragdoll.IsValid() ) return;

		_ragdoll = AddComponent<ModelPhysics>();
		_ragdoll.Renderer = ModelRenderer;
		_ragdoll.Model = ModelRenderer.Model;

		Controller.UseInputControls = false;
	}

	[Button]
	public void Unragdoll()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( !_ragdoll.IsValid() ) return;

		_ragdoll.Destroy();
		Controller.UseInputControls = true;
	}

	public void Respawn()
	{
		Unragdoll();
		UnitComponent.Alive = true;
		UnitComponent.Health = UnitComponent.MaxHealth;
		WorldPosition = _spawnPosition;
		ModelRenderer.Tint = ModelRenderer.Tint.WithAlpha( 1f );
	}
}
