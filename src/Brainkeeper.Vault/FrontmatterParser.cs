using System.Globalization;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Brainkeeper.Vault;

/// <summary>Separa frontmatter YAML y cuerpo. No valida: eso es <see cref="NoteValidator"/>.</summary>
public static partial class FrontmatterParser
{
    [GeneratedRegex(@"\A---\r?\n(?<yaml>.*?)\r?\n---\r?\n(?<body>.*)\z", RegexOptions.Singleline)]
    private static partial Regex FrontmatterRegex();

    private static readonly IDeserializer Yaml = new DeserializerBuilder().Build();

    public static (Dictionary<string, object?> Frontmatter, string Body)? Split(string markdown)
    {
        var m = FrontmatterRegex().Match(markdown);
        if (!m.Success) return null;
        var raw = Yaml.Deserialize<Dictionary<object, object?>>(m.Groups["yaml"].Value) ?? new();
        var fm = raw.ToDictionary(kv => kv.Key.ToString()!, kv => Normalize(kv.Value));
        return (fm, m.Groups["body"].Value);
    }

    /// <summary>YamlDotNet devuelve todos los escalares como string; aquí se tipan bool/int/float para que JSON Schema los valide bien.</summary>
    private static object? Normalize(object? value) => value switch
    {
        null => null,
        string s when s is "true" or "false" => bool.Parse(s),
        string s when long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) => l,
        string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
        string s => s,
        IDictionary<object, object?> d => d.ToDictionary(kv => kv.Key.ToString()!, kv => Normalize(kv.Value)),
        IList<object?> l => l.Select(Normalize).ToList(),
        _ => value,
    };
}
