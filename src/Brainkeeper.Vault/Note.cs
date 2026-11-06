namespace Brainkeeper.Vault;

public enum NoteType { Idea, Concepto, Persona, Proyecto, Decision, Diario, Borrador }
public enum Layer { Inbox, Daily, Wiki, Core }
public enum Visibility { Public, Private }
public enum NoteStatus { Draft, Active, Stale, Archived }

/// <summary>Frontmatter común a todas las notas. Los campos propios de cada tipo viven en <see cref="Extra"/>.</summary>
public sealed record NoteFrontmatter(
    string Id,
    NoteType Type,
    string Title,
    DateOnly Created,
    DateOnly Updated,
    string Author,
    bool Reviewed,
    Layer Layer,
    Visibility Visibility,
    IReadOnlyList<string> Sources,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Links,
    NoteStatus Status,
    IReadOnlyDictionary<string, object?> Extra);

/// <summary>Una nota = frontmatter + cuerpo Markdown + ruta relativa al vault.</summary>
public sealed record Note(string RelativePath, NoteFrontmatter Frontmatter, string Body)
{
    public bool IsWikiPage => Frontmatter.Layer == Layer.Wiki;
    public bool HasMiPostura => Body.Contains("## Mi postura", StringComparison.Ordinal);
}
