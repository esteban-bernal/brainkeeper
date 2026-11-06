using System.Text.Json;
using Json.Schema;

namespace Brainkeeper.Vault;

public sealed record ValidationError(string Path, string Message);

/// <summary>
/// Hook pre-write: valida una nota contra schemas/&lt;type&gt;.schema.json y las reglas que el esquema no expresa
/// (página de wiki sin "## Mi postura"). Si devuelve errores, la nota no se escribe.
/// </summary>
public sealed class NoteValidator
{
    private readonly Dictionary<string, JsonSchema> _schemas;

    public NoteValidator(string schemasDirectory)
    {
        _schemas = Directory.GetFiles(schemasDirectory, "*.schema.json")
            .ToDictionary(
                f => Path.GetFileName(f).Replace(".schema.json", "", StringComparison.Ordinal),
                f => JsonSchema.FromFile(f));
    }

    public IReadOnlyList<ValidationError> Validate(string markdown)
    {
        var split = FrontmatterParser.Split(markdown);
        if (split is null) return [new("(raíz)", "sin frontmatter")];

        var (fm, body) = split.Value;
        if (fm.GetValueOrDefault("type") is not string type || !_schemas.TryGetValue(type, out var schema))
            return [new("type", $"type desconocido: {fm.GetValueOrDefault("type")}")];

        var element = JsonSerializer.SerializeToElement(fm);
        var result = schema.Evaluate(element, new EvaluationOptions { OutputFormat = OutputFormat.List });
        var errors = new List<ValidationError>();
        if (!result.IsValid)
            Collect(result, errors);

        if (fm.GetValueOrDefault("layer") is "wiki" && !body.Contains("## Mi postura", StringComparison.Ordinal))
            errors.Add(new("body", "página de wiki sin sección '## Mi postura'"));

        if (errors.Count == 0 && !result.IsValid)
            errors.Add(new("(raíz)", "frontmatter inválido contra el esquema"));

        return errors;
    }

    private static void Collect(EvaluationResults node, List<ValidationError> into)
    {
        if (node.Errors is not null)
            foreach (var kv in node.Errors)
                into.Add(new(node.InstanceLocation.ToString(), $"{kv.Key}: {kv.Value}"));
        if (node.Details is not null)
            foreach (var child in node.Details)
                Collect(child, into);
    }
}
