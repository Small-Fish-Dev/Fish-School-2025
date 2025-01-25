using Sandbox;

public sealed class MultiplyComponent : Component
{
	/// <summary>
	/// How fast this unit multiplies itself
	/// </summary>
	[Property]
	public RangedFloat Cooldown { get; set; } = new RangedFloat( 2f, 3f );

	/// <summary>
	/// Which prefab we wnat to clone
	/// </summary>
	[Property]
	public PrefabScene PrefabToClone { get; set; }

	private TimeUntil _nextClone;

	protected override void OnStart()
	{
		ResetTimer();

		var foundObjects = Scene.FindInPhysics( new Sphere( WorldPosition, 100f ) );

		foreach ( var gameObject in foundObjects )
		{
			if ( gameObject.Components.TryGet<SnotPlayerComponent>( out var player ) )
			{
				player.UnitComponent.Damage( 50f );
				DestroyGameObject();
			}
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( _nextClone )
		{
			Multiply();
			ResetTimer();
		}
	}

	public void Multiply()
	{
		if ( !PrefabToClone.IsValid() ) return;

		var randomDirection = (Vector3)Game.Random.VectorInCircle().Normal;
		var startPos = WorldPosition + Vector3.Up * 20f;
		var endPos = startPos + randomDirection * 100f;
		var traceCheck = Scene.Trace.Ray( startPos, endPos )
			.Radius( 10f )
			.WithoutTags( "player" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !traceCheck.Hit )
		{
			var spawnPos = traceCheck.EndPosition + Vector3.Down * 20f;
			PrefabToClone.Clone( spawnPos );
		}
	}

	private void ResetTimer()
	{
		_nextClone = Cooldown.GetValue();
	}
}
