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

	private ModelPhysics _ragdoll;
	private Vector3 _spawnPosition;

	protected override void OnStart()
	{
		_spawnPosition = WorldPosition;
	}

	protected override void OnUpdate()
	{

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
	}
}
