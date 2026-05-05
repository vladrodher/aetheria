using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Herramienta de editor que genera todos los assets de la tabla periódica:
/// 118 ElementData, efectos base, reacciones de ejemplo, y el database central.
/// Menú: Aetheria → Generate Periodic Table
/// </summary>
public class PeriodicTableGenerator : EditorWindow
{
    private bool generateEffects = true;
    private bool generateElements = true;
    private bool generateReactions = true;
    private bool generateDatabase = true;
    private bool overwriteExisting;

    private const string RootPath = "Assets/Data";
    private const string ElementsPath = "Assets/Data/Elements";
    private const string EffectsPath = "Assets/Data/Effects";
    private const string ReactionsPath = "Assets/Data/Reactions";

    [MenuItem("Aetheria/Generate Periodic Table")]
    private static void ShowWindow()
    {
        GetWindow<PeriodicTableGenerator>("Periodic Table Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generador de Tabla Periódica", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        generateEffects = EditorGUILayout.Toggle("Generar Efectos", generateEffects);
        generateElements = EditorGUILayout.Toggle("Generar 118 Elementos", generateElements);
        generateReactions = EditorGUILayout.Toggle("Generar Reacciones", generateReactions);
        generateDatabase = EditorGUILayout.Toggle("Generar Database", generateDatabase);
        EditorGUILayout.Space();
        overwriteExisting = EditorGUILayout.Toggle("Sobreescribir existentes", overwriteExisting);
        EditorGUILayout.Space();

        if (GUILayout.Button("Generar Todo", GUILayout.Height(40)))
        {
            Generate();
        }
    }

    private void Generate()
    {
        EnsureFolders();

        Dictionary<string, ElementEffect> effects = null;
        ElementData[] elements = null;
        ElementReaction[] reactions = null;

        if (generateEffects)
            effects = CreateEffects();
        else
            effects = LoadExistingEffects();

        if (generateElements)
            elements = CreateAllElements(effects);
        else
            elements = LoadExistingElements();

        if (generateReactions && elements != null)
            reactions = CreateReactions(elements, effects);

        if (generateDatabase && elements != null)
            CreateDatabase(elements, reactions);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Aetheria] Tabla periódica generada exitosamente.");
    }

    // ── Carpetas ─────────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(ElementsPath))
            AssetDatabase.CreateFolder(RootPath, "Elements");
        if (!AssetDatabase.IsValidFolder(EffectsPath))
            AssetDatabase.CreateFolder(RootPath, "Effects");
        if (!AssetDatabase.IsValidFolder(ReactionsPath))
            AssetDatabase.CreateFolder(RootPath, "Reactions");
    }

    // ── Efectos ──────────────────────────────────────────────────────

    private Dictionary<string, ElementEffect> CreateEffects()
    {
        var map = new Dictionary<string, ElementEffect>();

        map["Burn"]     = CreateEffect("Burn",     "Daño de fuego continuo",       DamageType.Fire,      8f,  3f, 1f,   new Color(1f, 0.3f, 0f));
        map["Freeze"]   = CreateEffect("Freeze",   "Ralentiza al objetivo",        DamageType.Ice,       2f,  4f, 0.4f, new Color(0.5f, 0.8f, 1f));
        map["Shock"]    = CreateEffect("Shock",    "Descarga eléctrica en cadena",  DamageType.Electric, 12f,  1f, 1f,   new Color(1f, 1f, 0.3f));
        map["Poison"]   = CreateEffect("Poison",   "Veneno que se acumula",        DamageType.Toxic,     5f,  6f, 0.9f, new Color(0.4f, 0.9f, 0.2f));
        map["Corrode"]  = CreateEffect("Corrode",  "Corroe defensas del objetivo", DamageType.Corrosive, 4f,  5f, 1f,   new Color(0.6f, 0.8f, 0.1f));
        map["Radiate"]  = CreateEffect("Radiate",  "Radiación persistente",        DamageType.Radiant,   6f,  8f, 0.85f,new Color(0.8f, 0.4f, 1f));
        map["Explode"]  = CreateEffect("Explode",  "Explosión en área",            DamageType.Explosive,25f,  0f, 1f,   new Color(1f, 0.5f, 0f));

        return map;
    }

    private ElementEffect CreateEffect(string effectName, string desc, DamageType type,
        float dps, float duration, float slow, Color color)
    {
        string path = $"{EffectsPath}/{effectName}.asset";
        if (!overwriteExisting && AssetDatabase.LoadAssetAtPath<ElementEffect>(path) != null)
            return AssetDatabase.LoadAssetAtPath<ElementEffect>(path);

        var asset = ScriptableObject.CreateInstance<ElementEffect>();
        var so = new SerializedObject(asset);
        so.FindProperty("effectName").stringValue = effectName;
        so.FindProperty("description").stringValue = desc;
        so.FindProperty("damageType").enumValueIndex = (int)type;
        so.FindProperty("isDebuff").boolValue = true;
        so.FindProperty("damagePerSecond").floatValue = dps;
        so.FindProperty("duration").floatValue = duration;
        so.FindProperty("slowMultiplier").floatValue = slow;
        so.FindProperty("maxStacks").intValue = type == DamageType.Toxic ? 5 : 1;
        so.FindProperty("refreshDurationOnReapply").boolValue = true;
        so.FindProperty("tintColor").colorValue = color;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private Dictionary<string, ElementEffect> LoadExistingEffects()
    {
        var map = new Dictionary<string, ElementEffect>();
        var guids = AssetDatabase.FindAssets("t:ElementEffect", new[] { EffectsPath });
        foreach (var guid in guids)
        {
            var effect = AssetDatabase.LoadAssetAtPath<ElementEffect>(AssetDatabase.GUIDToAssetPath(guid));
            if (effect != null) map[effect.EffectName] = effect;
        }
        return map;
    }

    // ── Elementos ────────────────────────────────────────────────────

    private ElementData[] CreateAllElements(Dictionary<string, ElementEffect> effects)
    {
        var rawData = GetRawElementData();
        var result = new List<ElementData>(118);

        for (int i = 0; i < rawData.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Generando elementos",
                $"Creando elemento {i + 1}/118...", (float)i / rawData.Length);

            var parts = rawData[i].Split('|');
            int atomicNum    = int.Parse(parts[0]);
            string symbol    = parts[1];
            string elemName  = parts[2];
            var category     = ParseCategory(parts[3]);
            var phase        = ParsePhase(parts[4]);
            int period       = int.Parse(parts[5]);
            int group        = int.Parse(parts[6]);

            var element = CreateElement(
                atomicNum, symbol, elemName, category, phase,
                period, group, effects);

            result.Add(element);
        }

        EditorUtility.ClearProgressBar();
        return result.ToArray();
    }

    private ElementData CreateElement(int atomicNum, string symbol, string elemName,
        ElementCategory category, ElementPhase phase, int period, int group,
        Dictionary<string, ElementEffect> effects)
    {
        string path = $"{ElementsPath}/{atomicNum:D3}_{symbol}_{elemName}.asset";
        if (!overwriteExisting && AssetDatabase.LoadAssetAtPath<ElementData>(path) != null)
            return AssetDatabase.LoadAssetAtPath<ElementData>(path);

        var asset = ScriptableObject.CreateInstance<ElementData>();
        var so = new SerializedObject(asset);

        // Identidad
        so.FindProperty("atomicNumber").intValue = atomicNum;
        so.FindProperty("symbol").stringValue = symbol;
        so.FindProperty("elementName").stringValue = elemName;
        so.FindProperty("description").stringValue = $"Elemento #{atomicNum}: {elemName} ({symbol})";
        so.FindProperty("category").enumValueIndex = (int)category;
        so.FindProperty("phase").enumValueIndex = (int)phase;
        so.FindProperty("period").intValue = period;
        so.FindProperty("group").intValue = group;

        // Visuales según categoría
        var colors = GetCategoryColors(category);
        so.FindProperty("primaryColor").colorValue = colors.primary;
        so.FindProperty("secondaryColor").colorValue = colors.secondary;

        // Stats de combate basados en categoría
        var stats = GetCategoryStats(category);
        so.FindProperty("damageType").enumValueIndex = (int)stats.damageType;
        so.FindProperty("baseDamage").floatValue = stats.baseDamage;
        so.FindProperty("projectileSpeed").floatValue = stats.projectileSpeed;
        so.FindProperty("fireRate").floatValue = stats.fireRate;
        so.FindProperty("areaOfEffect").floatValue = stats.aoe;

        // Modificadores
        so.FindProperty("damageMultiplier").floatValue = stats.damageMult;
        so.FindProperty("speedMultiplier").floatValue = stats.speedMult;
        so.FindProperty("rateMultiplier").floatValue = stats.rateMult;

        // Rareza basada en periodo
        int rarity = Mathf.Clamp(period <= 3 ? 1 : period <= 5 ? 2 : period <= 6 ? 3 : 4, 1, 5);
        so.FindProperty("rarity").intValue = rarity;
        so.FindProperty("discoveredByDefault").boolValue = period <= 2;

        // Efectos on-hit según categoría
        AssignEffects(so, category, effects);

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private void AssignEffects(SerializedObject so, ElementCategory category,
        Dictionary<string, ElementEffect> effects)
    {
        var prop = so.FindProperty("onHitEffects");
        string effectKey = category switch
        {
            ElementCategory.AlkaliMetal          => "Explode",
            ElementCategory.AlkalineEarthMetal    => null,
            ElementCategory.TransitionMetal       => null,
            ElementCategory.PostTransitionMetal   => "Corrode",
            ElementCategory.Metalloid             => "Shock",
            ElementCategory.NonMetal              => "Burn",
            ElementCategory.Halogen               => "Poison",
            ElementCategory.NobleGas              => "Radiate",
            ElementCategory.Lanthanide            => "Radiate",
            ElementCategory.Actinide              => "Radiate",
            _                                     => null
        };

        if (effectKey != null && effects.TryGetValue(effectKey, out var effect))
        {
            prop.arraySize = 1;
            prop.GetArrayElementAtIndex(0).objectReferenceValue = effect;
        }
        else
        {
            prop.arraySize = 0;
        }
    }

    // ── Reacciones ───────────────────────────────────────────────────

    private ElementReaction[] CreateReactions(ElementData[] elements,
        Dictionary<string, ElementEffect> effects)
    {
        var bySymbol = new Dictionary<string, ElementData>();
        foreach (var el in elements)
            bySymbol[el.Symbol] = el;

        var reactionDefs = new[]
        {
            new ReactionDef("H",  "O",  "Steam",           "Nube de vapor que ralentiza en área",       1.5f, 2f, "Freeze",  new Color(0.8f, 0.9f, 1f)),
            new ReactionDef("Na", "Cl", "Salt Burst",       "Explosión cristalina de sal",              2f,   1f, "Explode", new Color(1f, 1f, 0.9f)),
            new ReactionDef("Fe", "O",  "Rust",             "Óxido corrosivo que debilita defensas",    1.2f, 1.5f,"Corrode",new Color(0.7f, 0.3f, 0.1f)),
            new ReactionDef("Li", "O",  "Lithium Fire",     "Fuego de litio, intenso e inextinguible",  2.5f, 1f, "Burn",    new Color(1f, 0.2f, 0.3f)),
            new ReactionDef("N",  "O",  "Toxic Gas",        "Nube de gas tóxico en área",               1.3f, 2.5f,"Poison", new Color(0.5f, 0.7f, 0.1f)),
            new ReactionDef("P",  "O",  "Phosphor Flame",   "Fuego blanco persistente",                 1.8f, 1f, "Burn",    new Color(1f, 1f, 0.8f)),
            new ReactionDef("K",  "O",  "Potassium Flare",  "Llamarada violeta explosiva",              2.2f, 1.5f,"Explode",new Color(0.7f, 0.3f, 1f)),
            new ReactionDef("He", "Ne", "Noble Resonance",  "Pulso de energía radiante pura",           3f,   3f, "Radiate", new Color(0.6f, 1f, 1f)),
            new ReactionDef("S",  "O",  "Sulfuric Cloud",   "Ácido sulfúrico en área",                  1.4f, 2f, "Corrode", new Color(0.9f, 0.9f, 0.2f)),
            new ReactionDef("U",  "Pu", "Nuclear Chain",    "Reacción nuclear en cadena masiva",        5f,   5f, "Radiate", new Color(0.3f, 1f, 0.3f)),
            new ReactionDef("Si", "O",  "Glass Shards",     "Fragmentos cortantes de cristal",          1.6f, 1f, null,      new Color(0.8f, 0.9f, 1f)),
            new ReactionDef("Cu", "S",  "Verdigris",        "Pátina tóxica que corroe y envenena",      1.3f, 1.5f,"Poison", new Color(0.2f, 0.8f, 0.5f)),
        };

        var result = new List<ElementReaction>();
        foreach (var def in reactionDefs)
        {
            if (!bySymbol.TryGetValue(def.symbolA, out var elA)) continue;
            if (!bySymbol.TryGetValue(def.symbolB, out var elB)) continue;

            var reaction = CreateReaction(elA, elB, def, effects);
            if (reaction != null) result.Add(reaction);
        }

        return result.ToArray();
    }

    private ElementReaction CreateReaction(ElementData elA, ElementData elB,
        ReactionDef def, Dictionary<string, ElementEffect> effects)
    {
        string safeName = def.name.Replace(" ", "");
        string path = $"{ReactionsPath}/{elA.Symbol}_{elB.Symbol}_{safeName}.asset";
        if (!overwriteExisting && AssetDatabase.LoadAssetAtPath<ElementReaction>(path) != null)
            return AssetDatabase.LoadAssetAtPath<ElementReaction>(path);

        var asset = ScriptableObject.CreateInstance<ElementReaction>();
        var so = new SerializedObject(asset);

        so.FindProperty("elementA").objectReferenceValue = elA;
        so.FindProperty("elementB").objectReferenceValue = elB;
        so.FindProperty("reactionName").stringValue = def.name;
        so.FindProperty("description").stringValue = def.description;
        so.FindProperty("damageMultiplier").floatValue = def.damageMult;
        so.FindProperty("areaMultiplier").floatValue = def.areaMult;
        so.FindProperty("reactionColor").colorValue = def.color;
        so.FindProperty("cooldown").floatValue = 0.5f;
        so.FindProperty("triggerRadius").floatValue = 2f + def.areaMult;

        var effectsProp = so.FindProperty("resultEffects");
        if (def.effectKey != null && effects.TryGetValue(def.effectKey, out var effect))
        {
            effectsProp.arraySize = 1;
            effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = effect;
        }
        else
        {
            effectsProp.arraySize = 0;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    // ── Database ─────────────────────────────────────────────────────

    private void CreateDatabase(ElementData[] elements, ElementReaction[] reactions)
    {
        string path = $"{RootPath}/PeriodicTable.asset";

        var asset = AssetDatabase.LoadAssetAtPath<PeriodicTableDatabase>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<PeriodicTableDatabase>();
            AssetDatabase.CreateAsset(asset, path);
        }

        var so = new SerializedObject(asset);

        var elemProp = so.FindProperty("elements");
        elemProp.arraySize = elements.Length;
        for (int i = 0; i < elements.Length; i++)
            elemProp.GetArrayElementAtIndex(i).objectReferenceValue = elements[i];

        if (reactions != null)
        {
            var rxProp = so.FindProperty("reactions");
            rxProp.arraySize = reactions.Length;
            for (int i = 0; i < reactions.Length; i++)
                rxProp.GetArrayElementAtIndex(i).objectReferenceValue = reactions[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    // ── Carga de assets existentes ───────────────────────────────────

    private ElementData[] LoadExistingElements()
    {
        var guids = AssetDatabase.FindAssets("t:ElementData", new[] { ElementsPath });
        var list = new List<ElementData>();
        foreach (var guid in guids)
        {
            var el = AssetDatabase.LoadAssetAtPath<ElementData>(AssetDatabase.GUIDToAssetPath(guid));
            if (el != null) list.Add(el);
        }
        list.Sort((a, b) => a.AtomicNumber.CompareTo(b.AtomicNumber));
        return list.ToArray();
    }

    // ── Stats por categoría ──────────────────────────────────────────

    private struct CategoryColors
    {
        public Color primary;
        public Color secondary;
    }

    private static CategoryColors GetCategoryColors(ElementCategory cat)
    {
        return cat switch
        {
            ElementCategory.AlkaliMetal        => new CategoryColors { primary = new Color(1f, 0.40f, 0.40f), secondary = new Color(0.8f, 0.2f, 0.2f) },
            ElementCategory.AlkalineEarthMetal => new CategoryColors { primary = new Color(1f, 0.87f, 0.68f), secondary = new Color(0.85f, 0.65f, 0.4f) },
            ElementCategory.TransitionMetal    => new CategoryColors { primary = new Color(1f, 0.75f, 0.75f), secondary = new Color(0.8f, 0.55f, 0.55f) },
            ElementCategory.PostTransitionMetal=> new CategoryColors { primary = new Color(0.78f, 0.78f, 0.78f), secondary = new Color(0.55f, 0.55f, 0.55f) },
            ElementCategory.Metalloid          => new CategoryColors { primary = new Color(0.80f, 0.80f, 0.60f), secondary = new Color(0.6f, 0.6f, 0.35f) },
            ElementCategory.NonMetal           => new CategoryColors { primary = new Color(0.63f, 1f, 0.63f), secondary = new Color(0.3f, 0.8f, 0.3f) },
            ElementCategory.Halogen            => new CategoryColors { primary = new Color(1f, 1f, 0.60f), secondary = new Color(0.85f, 0.85f, 0.3f) },
            ElementCategory.NobleGas           => new CategoryColors { primary = new Color(0.75f, 1f, 1f), secondary = new Color(0.4f, 0.8f, 0.8f) },
            ElementCategory.Lanthanide         => new CategoryColors { primary = new Color(1f, 0.75f, 1f), secondary = new Color(0.8f, 0.4f, 0.8f) },
            ElementCategory.Actinide           => new CategoryColors { primary = new Color(1f, 0.60f, 0.80f), secondary = new Color(0.8f, 0.3f, 0.55f) },
            _                                  => new CategoryColors { primary = Color.gray, secondary = Color.gray },
        };
    }

    private struct CategoryStats
    {
        public DamageType damageType;
        public float baseDamage, projectileSpeed, fireRate, aoe;
        public float damageMult, speedMult, rateMult;
    }

    private static CategoryStats GetCategoryStats(ElementCategory cat)
    {
        return cat switch
        {
            ElementCategory.AlkaliMetal => new CategoryStats
            {
                damageType = DamageType.Explosive, baseDamage = 15f, projectileSpeed = 15f,
                fireRate = 3f, aoe = 3f, damageMult = 1.4f, speedMult = 0.8f, rateMult = 0.7f
            },
            ElementCategory.AlkalineEarthMetal => new CategoryStats
            {
                damageType = DamageType.Physical, baseDamage = 12f, projectileSpeed = 18f,
                fireRate = 4f, aoe = 1f, damageMult = 1.2f, speedMult = 0.9f, rateMult = 1f
            },
            ElementCategory.TransitionMetal => new CategoryStats
            {
                damageType = DamageType.Physical, baseDamage = 10f, projectileSpeed = 20f,
                fireRate = 5f, aoe = 0f, damageMult = 1f, speedMult = 1f, rateMult = 1f
            },
            ElementCategory.PostTransitionMetal => new CategoryStats
            {
                damageType = DamageType.Corrosive, baseDamage = 8f, projectileSpeed = 22f,
                fireRate = 6f, aoe = 1f, damageMult = 0.9f, speedMult = 1.1f, rateMult = 1.1f
            },
            ElementCategory.Metalloid => new CategoryStats
            {
                damageType = DamageType.Electric, baseDamage = 9f, projectileSpeed = 25f,
                fireRate = 7f, aoe = 1.5f, damageMult = 1f, speedMult = 1.2f, rateMult = 1.1f
            },
            ElementCategory.NonMetal => new CategoryStats
            {
                damageType = DamageType.Fire, baseDamage = 8f, projectileSpeed = 28f,
                fireRate = 8f, aoe = 0.5f, damageMult = 1.1f, speedMult = 1.3f, rateMult = 1.2f
            },
            ElementCategory.Halogen => new CategoryStats
            {
                damageType = DamageType.Toxic, baseDamage = 6f, projectileSpeed = 30f,
                fireRate = 9f, aoe = 2f, damageMult = 0.8f, speedMult = 1.4f, rateMult = 1.3f
            },
            ElementCategory.NobleGas => new CategoryStats
            {
                damageType = DamageType.Radiant, baseDamage = 12f, projectileSpeed = 35f,
                fireRate = 4f, aoe = 2.5f, damageMult = 1.3f, speedMult = 1.5f, rateMult = 0.8f
            },
            ElementCategory.Lanthanide => new CategoryStats
            {
                damageType = DamageType.Radiant, baseDamage = 14f, projectileSpeed = 22f,
                fireRate = 3f, aoe = 2f, damageMult = 1.4f, speedMult = 1f, rateMult = 0.9f
            },
            ElementCategory.Actinide => new CategoryStats
            {
                damageType = DamageType.Explosive, baseDamage = 18f, projectileSpeed = 15f,
                fireRate = 2f, aoe = 4f, damageMult = 1.8f, speedMult = 0.7f, rateMult = 0.6f
            },
            _ => new CategoryStats
            {
                damageType = DamageType.Physical, baseDamage = 5f, projectileSpeed = 20f,
                fireRate = 5f, aoe = 0f, damageMult = 1f, speedMult = 1f, rateMult = 1f
            },
        };
    }

    // ── Parseo de enums ──────────────────────────────────────────────

    private static ElementCategory ParseCategory(string abbr)
    {
        return abbr switch
        {
            "AM"  => ElementCategory.AlkaliMetal,
            "AEM" => ElementCategory.AlkalineEarthMetal,
            "TM"  => ElementCategory.TransitionMetal,
            "PTM" => ElementCategory.PostTransitionMetal,
            "ML"  => ElementCategory.Metalloid,
            "NM"  => ElementCategory.NonMetal,
            "HL"  => ElementCategory.Halogen,
            "NG"  => ElementCategory.NobleGas,
            "LN"  => ElementCategory.Lanthanide,
            "AC"  => ElementCategory.Actinide,
            _     => ElementCategory.Unknown
        };
    }

    private static ElementPhase ParsePhase(string abbr)
    {
        return abbr switch
        {
            "S" => ElementPhase.Solid,
            "L" => ElementPhase.Liquid,
            "G" => ElementPhase.Gas,
            "P" => ElementPhase.Plasma,
            _   => ElementPhase.Solid
        };
    }

    // ── Struct auxiliar para reacciones ───────────────────────────────

    private struct ReactionDef
    {
        public string symbolA, symbolB, name, description, effectKey;
        public float damageMult, areaMult;
        public Color color;

        public ReactionDef(string a, string b, string n, string desc,
            float dmg, float area, string fx, Color c)
        {
            symbolA = a; symbolB = b; name = n; description = desc;
            damageMult = dmg; areaMult = area; effectKey = fx; color = c;
        }
    }

    // ── Los 118 elementos de la tabla periódica ──────────────────────
    // Formato: "atómico|símbolo|nombre|categoría|fase|periodo|grupo"

    private static string[] GetRawElementData()
    {
        return new[]
        {
            // ── Periodo 1 ──
            "1|H|Hydrogen|NM|G|1|1",
            "2|He|Helium|NG|G|1|18",
            // ── Periodo 2 ──
            "3|Li|Lithium|AM|S|2|1",
            "4|Be|Beryllium|AEM|S|2|2",
            "5|B|Boron|ML|S|2|13",
            "6|C|Carbon|NM|S|2|14",
            "7|N|Nitrogen|NM|G|2|15",
            "8|O|Oxygen|NM|G|2|16",
            "9|F|Fluorine|HL|G|2|17",
            "10|Ne|Neon|NG|G|2|18",
            // ── Periodo 3 ──
            "11|Na|Sodium|AM|S|3|1",
            "12|Mg|Magnesium|AEM|S|3|2",
            "13|Al|Aluminum|PTM|S|3|13",
            "14|Si|Silicon|ML|S|3|14",
            "15|P|Phosphorus|NM|S|3|15",
            "16|S|Sulfur|NM|S|3|16",
            "17|Cl|Chlorine|HL|G|3|17",
            "18|Ar|Argon|NG|G|3|18",
            // ── Periodo 4 ──
            "19|K|Potassium|AM|S|4|1",
            "20|Ca|Calcium|AEM|S|4|2",
            "21|Sc|Scandium|TM|S|4|3",
            "22|Ti|Titanium|TM|S|4|4",
            "23|V|Vanadium|TM|S|4|5",
            "24|Cr|Chromium|TM|S|4|6",
            "25|Mn|Manganese|TM|S|4|7",
            "26|Fe|Iron|TM|S|4|8",
            "27|Co|Cobalt|TM|S|4|9",
            "28|Ni|Nickel|TM|S|4|10",
            "29|Cu|Copper|TM|S|4|11",
            "30|Zn|Zinc|TM|S|4|12",
            "31|Ga|Gallium|PTM|S|4|13",
            "32|Ge|Germanium|ML|S|4|14",
            "33|As|Arsenic|ML|S|4|15",
            "34|Se|Selenium|NM|S|4|16",
            "35|Br|Bromine|HL|L|4|17",
            "36|Kr|Krypton|NG|G|4|18",
            // ── Periodo 5 ──
            "37|Rb|Rubidium|AM|S|5|1",
            "38|Sr|Strontium|AEM|S|5|2",
            "39|Y|Yttrium|TM|S|5|3",
            "40|Zr|Zirconium|TM|S|5|4",
            "41|Nb|Niobium|TM|S|5|5",
            "42|Mo|Molybdenum|TM|S|5|6",
            "43|Tc|Technetium|TM|S|5|7",
            "44|Ru|Ruthenium|TM|S|5|8",
            "45|Rh|Rhodium|TM|S|5|9",
            "46|Pd|Palladium|TM|S|5|10",
            "47|Ag|Silver|TM|S|5|11",
            "48|Cd|Cadmium|TM|S|5|12",
            "49|In|Indium|PTM|S|5|13",
            "50|Sn|Tin|PTM|S|5|14",
            "51|Sb|Antimony|ML|S|5|15",
            "52|Te|Tellurium|ML|S|5|16",
            "53|I|Iodine|HL|S|5|17",
            "54|Xe|Xenon|NG|G|5|18",
            // ── Periodo 6 ──
            "55|Cs|Cesium|AM|S|6|1",
            "56|Ba|Barium|AEM|S|6|2",
            // Lantánidos (57–71)
            "57|La|Lanthanum|LN|S|6|3",
            "58|Ce|Cerium|LN|S|6|3",
            "59|Pr|Praseodymium|LN|S|6|3",
            "60|Nd|Neodymium|LN|S|6|3",
            "61|Pm|Promethium|LN|S|6|3",
            "62|Sm|Samarium|LN|S|6|3",
            "63|Eu|Europium|LN|S|6|3",
            "64|Gd|Gadolinium|LN|S|6|3",
            "65|Tb|Terbium|LN|S|6|3",
            "66|Dy|Dysprosium|LN|S|6|3",
            "67|Ho|Holmium|LN|S|6|3",
            "68|Er|Erbium|LN|S|6|3",
            "69|Tm|Thulium|LN|S|6|3",
            "70|Yb|Ytterbium|LN|S|6|3",
            "71|Lu|Lutetium|LN|S|6|3",
            // Periodo 6 continúa
            "72|Hf|Hafnium|TM|S|6|4",
            "73|Ta|Tantalum|TM|S|6|5",
            "74|W|Tungsten|TM|S|6|6",
            "75|Re|Rhenium|TM|S|6|7",
            "76|Os|Osmium|TM|S|6|8",
            "77|Ir|Iridium|TM|S|6|9",
            "78|Pt|Platinum|TM|S|6|10",
            "79|Au|Gold|TM|S|6|11",
            "80|Hg|Mercury|TM|L|6|12",
            "81|Tl|Thallium|PTM|S|6|13",
            "82|Pb|Lead|PTM|S|6|14",
            "83|Bi|Bismuth|PTM|S|6|15",
            "84|Po|Polonium|ML|S|6|16",
            "85|At|Astatine|HL|S|6|17",
            "86|Rn|Radon|NG|G|6|18",
            // ── Periodo 7 ──
            "87|Fr|Francium|AM|S|7|1",
            "88|Ra|Radium|AEM|S|7|2",
            // Actínidos (89–103)
            "89|Ac|Actinium|AC|S|7|3",
            "90|Th|Thorium|AC|S|7|3",
            "91|Pa|Protactinium|AC|S|7|3",
            "92|U|Uranium|AC|S|7|3",
            "93|Np|Neptunium|AC|S|7|3",
            "94|Pu|Plutonium|AC|S|7|3",
            "95|Am|Americium|AC|S|7|3",
            "96|Cm|Curium|AC|S|7|3",
            "97|Bk|Berkelium|AC|S|7|3",
            "98|Cf|Californium|AC|S|7|3",
            "99|Es|Einsteinium|AC|S|7|3",
            "100|Fm|Fermium|AC|S|7|3",
            "101|Md|Mendelevium|AC|S|7|3",
            "102|No|Nobelium|AC|S|7|3",
            "103|Lr|Lawrencium|AC|S|7|3",
            // Periodo 7 continúa
            "104|Rf|Rutherfordium|TM|S|7|4",
            "105|Db|Dubnium|TM|S|7|5",
            "106|Sg|Seaborgium|TM|S|7|6",
            "107|Bh|Bohrium|TM|S|7|7",
            "108|Hs|Hassium|TM|S|7|8",
            "109|Mt|Meitnerium|TM|S|7|9",
            "110|Ds|Darmstadtium|TM|S|7|10",
            "111|Rg|Roentgenium|TM|S|7|11",
            "112|Cn|Copernicium|TM|S|7|12",
            "113|Nh|Nihonium|PTM|S|7|13",
            "114|Fl|Flerovium|PTM|S|7|14",
            "115|Mc|Moscovium|PTM|S|7|15",
            "116|Lv|Livermorium|PTM|S|7|16",
            "117|Ts|Tennessine|HL|S|7|17",
            "118|Og|Oganesson|NG|G|7|18",
        };
    }
}
