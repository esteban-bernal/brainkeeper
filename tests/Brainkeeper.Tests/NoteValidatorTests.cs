using Brainkeeper.Vault;
using FluentAssertions;

namespace Brainkeeper.Tests;

/// <summary>Los 7 ejemplos válidos pasan; los 7 inválidos fallan. Mismo contrato que schemas/validate.py.</summary>
public sealed class NoteValidatorTests
{
    private static readonly string Root = FindRepoRoot();
    private static readonly NoteValidator Validator = new(Path.Combine(Root, "schemas"));

    public static IEnumerable<object[]> Examples(string suffix) =>
        Directory.GetFiles(Path.Combine(Root, "schemas", "examples"), $"*.{suffix}.md").Select(f => new object[] { f });

    [Theory, MemberData(nameof(Examples), "ok")]
    public void Valid_examples_pass(string file) =>
        Validator.Validate(File.ReadAllText(file)).Should().BeEmpty(because: Path.GetFileName(file));

    [Theory, MemberData(nameof(Examples), "bad")]
    public void Invalid_examples_fail(string file) =>
        Validator.Validate(File.ReadAllText(file)).Should().NotBeEmpty(because: Path.GetFileName(file));

    [Fact]
    public void Missing_frontmatter_is_rejected() =>
        Validator.Validate("# sin frontmatter").Should().ContainSingle(e => e.Message == "sin frontmatter");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "schemas", "examples"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("raíz del repo (schemas/examples) no encontrada");
    }
}
