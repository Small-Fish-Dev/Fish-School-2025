using Sandbox;

public enum TeamType
{
	[Icon( "🧑" )]
	[Description( "Players, turrets, and whatnots" )]
	Player,
	[Icon( "💩" )]
	[Description( "Anything green and mean" )]
	Snot
}

public sealed class UnitComponent : Component
{
	/// <summary>
	/// The name displayed for your unit
	/// </summary>
	[Property]
	[Category( "Info" )]
	public string Name { get; set; }

	/// <summary>
	/// Which team this unit will be in
	/// </summary>
	[Property]
	[Category( "Info" )]
	public TeamType Team { get; set; }

	[Property]
	[Category( "Health" )]
	[Range( 10f, 300f, 10f )]
	public float MaxHealth { get; set; } = 100f;

	/// <summary>
	/// How much health is regenerated per second
	/// </summary>
	[Property]
	[Category( "Health" )]
	[Range( 0f, 30f, 1f )]
	public float HealthRegeneration { get; set; } = 5f;

	[Property]
	[Category( "Component" )]
	public SkinnedModelRenderer ModelRenderer { get; set; }

	public bool Alive = true;

	private float _health;
	public float Health
	{
		get
		{
			return _health;
		}
		set
		{
			UpdateHealth( value );
		}
	}

	private TimeUntil _nextRegen;

	protected override void OnUpdate()
	{
		if ( !Alive ) return;
		if ( !ModelRenderer.IsValid() ) return;

		if ( Team == TeamType.Snot )
		{
			var remappedHealth = MathX.Remap( Health, 0f, MaxHealth, 0f, 100f );
			var currentHealth = ModelRenderer.GetFloat( "health" );
			var lerpedHealth = MathX.Lerp( currentHealth, remappedHealth, Time.Delta * 2f );
			ModelRenderer.Set( "health", lerpedHealth );
		}

		DebugOverlay.Text( WorldPosition + Vector3.Up * 80f, $"{Name} [{Health}/{MaxHealth}]" );
	}

	protected override void OnStart()
	{
		_health = MaxHealth;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Alive ) return;

		if ( _nextRegen )
		{
			Damage( -HealthRegeneration ); // Heal
			_nextRegen = 1f; // Reset timer to 1 second
		}
	}

	protected override void OnDestroy()
	{

	}

	[Button( "Hurt 10", "💥" )]
	[Category( "Health" )]
	public void HurtDebug()
	{
		Damage( 10f );
	}

	[Button( "Heal 10", "❤️" )]
	[Category( "Health" )]
	public void HealDebug()
	{
		Damage( -10f );
	}

	[Button( "Hurt a lot", "💥" )]
	[Category( "Health" )]
	public void HurtLotDebug()
	{
		Damage( 30f );
	}

	/// <summary>
	/// Positive = hurt, Negative = heal
	/// </summary>
	/// <param name="damage"></param>
	public void Damage( float damage )
	{
		if ( !Alive ) return;

		Health -= damage;

		if ( damage >= 0f )
			_nextRegen = 5f;
	}

	private void UpdateHealth( float newHealth )
	{
		var difference = newHealth - Health;
		_health = float.Clamp( newHealth, 0f, MaxHealth );

		if ( !ModelRenderer.IsValid() ) return;

		if ( difference < 0f )
		{
			var remappedDamage = MathX.Remap( -difference, 0f, MaxHealth, 0f, 100f );
			DamageAnimation( remappedDamage );
		}

		if ( Health <= 0f )
			Kill();
	}

	private async void DamageAnimation( float damage )
	{
		ModelRenderer.LocalScale *= 1.1f;
		ModelRenderer.Tint = Color.Red;

		await Task.DelaySeconds( damage / 100f );

		ModelRenderer.GameObject.LocalScale /= 1.1f;
		ModelRenderer.Tint = Color.White;
	}

	private void DeathAnimation()
	{
		ModelRenderer.Set( "dead", true ); // Set death animation for snot
	}

	public async void Kill()
	{
		Alive = false;
		DeathAnimation();

		var playerComponent = GetComponent<SnotPlayerComponent>();

		if ( playerComponent.IsValid() )
			playerComponent.Ragdoll();

		await Task.DelaySeconds( 1f );

		if ( playerComponent.IsValid() )
			playerComponent.Respawn();
		else
			GameObject.Destroy();
	}
}
