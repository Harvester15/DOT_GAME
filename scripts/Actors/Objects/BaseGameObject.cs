using Godot;

//namespace dot_battle_signal_bus.scripts;

public abstract partial class BaseGameObject : Area2D
{

    public enum Team { Neutral, Player, Enemy, Ally }

    public string UnitName { get; set; } = "BaseObject";
    public Team UnitTeam { get; set; } = 0; // 0=neutral, 1=player, 2=enemy, etc.
    public TextureRect Portrait;
    public CollisionShape2D CollisionShape;
    public bool _hasBeenPlaced = false;

    // Every object in your game must define what happens when clicked
    public abstract void OnPlayerInteract(BaseGameObject gameObject);

    public TextureRect GetSprite()
    {
        return Portrait;
    }

}

