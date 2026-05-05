/// <summary>
/// Categoría del elemento en la tabla periódica.
/// Determina el color de fondo en la UI y comportamientos compartidos por grupo.
/// </summary>
public enum ElementCategory
{
    AlkaliMetal,
    AlkalineEarthMetal,
    TransitionMetal,
    PostTransitionMetal,
    Metalloid,
    NonMetal,
    Halogen,
    NobleGas,
    Lanthanide,
    Actinide,
    Unknown
}

/// <summary>
/// Estado de fase del elemento. Afecta el tipo de proyectil base y VFX.
/// </summary>
public enum ElementPhase
{
    Solid,
    Liquid,
    Gas,
    Plasma
}

/// <summary>
/// Tipo de daño elemental. Usado por el sistema de combate
/// para calcular resistencias y reacciones.
/// </summary>
public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Electric,
    Toxic,
    Radiant,
    Corrosive,
    Explosive
}
