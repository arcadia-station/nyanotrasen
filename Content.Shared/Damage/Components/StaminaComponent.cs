using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Add to an entity to paralyze it whenever it reaches critical amounts of Stamina DamageType.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed class StaminaComponent : Component
{
    /// <summary>
    /// Have we reached peak stamina damage and been paralyzed?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("critical")]
    public bool Critical;

    /// <summary>
    /// How much stamina reduces per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("decay")]
    public float Decay = 5f;

    /// <summary>
    /// How much time after receiving damage until stamina starts decreasing.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("cooldown")]
    public float DecayCooldown = 5f;

    /// <summary>
    /// How much stamina damage this entity has taken.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("staminaDamage")]
    public float StaminaDamage;

    /// <summary>
    /// How much stamina damage is required to entire stam crit.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("excess")]
    public float CritThreshold = 100f;

    /// <summary>
    /// What percentage of the CritThreshold is the maximum amount of stamina
    /// damage that can be reached by exhausting yourself with melee weapon
    /// attacks?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("meleeCostLimitFactor")]
    public float MeleeCostLimitFactor = 0.95f;

    /// <summary>
    /// To avoid continuously updating our data we track the last time we updated so we can extrapolate our current stamina.
    /// </summary>
    [DataField("lastUpdate", customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
