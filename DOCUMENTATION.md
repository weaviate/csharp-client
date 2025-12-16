# API Documentation Guide

This document explains how to generate and maintain API documentation for the Weaviate C# Client.

## Overview

The Weaviate C# Client uses XMLDoc comments in the source code to generate API documentation in two formats:
- **HTML** - Professional, searchable documentation website using DocFX
- **Markdown** - Simple, portable markdown files

## Quick Start

### Generate HTML Documentation

```bash
# macOS/Linux
./scripts/generate-docs-html.sh

# Windows
.\scripts\generate-docs-html.ps1
```

Output: `_site/` directory
View: `docfx serve _site` then open http://localhost:8080

### Generate Markdown Documentation

```bash
# macOS/Linux
./scripts/generate-docs-markdown.sh

# Direct Python call
python3 scripts/generate-docs-markdown.py src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.xml docs/api-markdown
```

Output: `docs/api-markdown/` directory
View: `docs/api-markdown/index.md`

## Documentation Status

### Current Coverage

As of this implementation:
- **Total public types:** 384
- **Documented types:** ~36% (including recently added documentation)
- **XML generation:** Enabled in project file

### Recently Documented (High Priority)

✅ **Model Classes:**
- `Vector`, `VectorSingle<T>`, `VectorMulti<T>` - Vector data types
- `Vectors` - Vector collection
- `Filter`, `PropertyFilter`, `ReferenceFilter` - Filtering system
- `Sort` - Sort specifications
- `BatchInsertRequest`, `BatchInsertResponse` - Batch operations

✅ **Infrastructure:**
- XML documentation file generation enabled in `Weaviate.Client.csproj`
- DocFX configuration (`docfx.json`)
- Documentation generation scripts

### To Be Documented

The following high-priority areas still need XMLDoc comments:

**Configuration Classes:**
- `VectorConfig`, `VectorConfigList`
- `InvertedIndexConfig`
- `ShardingConfig`
- `ReplicationConfig`
- `MultiTenancyConfig`
- `BM25Config`, `StopwordConfig`

**Model Classes:**
- `Property`, `Reference` - Collection schema
- `Collection`, `CollectionConfig` - Collection configuration
- `VectorIndex.*` - HNSW, Flat, Dynamic configurations
- `Vectorizer.*` - All vectorizer variants
- RBAC models in `Rbac.cs`

**Serialization:**
- `IPropertyConverter`, `PropertyConverterBase`
- `PropertyBag`, `PropertyConverterRegistry`
- Individual property converters

## Project Configuration

### XML Documentation Settings

Location: `src/Weaviate.Client/Weaviate.Client.csproj`

```xml
<PropertyGroup>
  <!-- Enable XML documentation generation -->
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\Weaviate.Client.xml</DocumentationFile>
  <!-- Suppress warnings for missing XML comments (remove this line once all public APIs are documented) -->
  <NoWarn>$(NoWarn);CS1591</NoWarn>
</PropertyGroup>
```

**Note:** The `CS1591` warning suppression should be removed once all public APIs are documented.

## Writing XMLDoc Comments

### Basic Structure

```csharp
/// <summary>
/// Brief description of the type or member.
/// </summary>
/// <remarks>
/// Detailed explanation, usage notes, or important information.
/// Can include multiple paragraphs.
/// </remarks>
/// <param name="paramName">Description of the parameter.</param>
/// <returns>Description of the return value.</returns>
/// <exception cref="ExceptionType">When this exception is thrown.</exception>
/// <example>
/// <code>
/// var example = new MyClass();
/// example.DoSomething();
/// </code>
/// </example>
public class MyClass
{
    // Implementation
}
```

### Key Tags

- `<summary>` - Brief description (required for all public APIs)
- `<remarks>` - Additional details, usage notes
- `<param>` - Parameter descriptions
- `<typeparam>` - Generic type parameter descriptions
- `<returns>` - Return value description
- `<exception>` - Exceptions that may be thrown
- `<example>` - Code examples
- `<code>` - Code blocks within examples
- `<see cref=""/>` - Cross-reference to other types/members
- `<seealso cref=""/>` - Related types/members

### Best Practices

1. **Be Concise** - Summaries should be one or two sentences
2. **Be Specific** - Describe what the member does, not how it works
3. **Include Examples** - Especially for complex APIs
4. **Cross-Reference** - Use `<see cref=""/>` to link related types
5. **Document Parameters** - Explain purpose, valid values, constraints
6. **Document Returns** - Describe what is returned and when
7. **Note Exceptions** - Document all exceptions that can be thrown

### Example from Vector.cs

```csharp
/// <summary>
/// Represents a vector for use with Weaviate, supporting both single and multi-vector configurations.
/// Vectors can contain numeric values of various types (double, float, int, etc.) and can be implicitly converted from native arrays.
/// </summary>
/// <remarks>
/// This is the base class for all vector types in the Weaviate client library.
/// Use <see cref="VectorSingle{T}"/> for single vectors or <see cref="VectorMulti{T}"/> for multi-vector representations.
/// Supports implicit conversion from and to native C# arrays.
/// </remarks>
public abstract record Vector : IEnumerable, IHybridVectorInput
{
    /// <summary>
    /// Gets or initializes the name of this vector. Defaults to "default".
    /// Used to identify specific vectors in multi-vector configurations.
    /// </summary>
    public string Name { get; init; } = "default";
}
```

## Documentation Generation Scripts

### HTML Generation (DocFX)

**Files:**
- `scripts/generate-docs-html.sh` - Bash script for macOS/Linux
- `scripts/generate-docs-html.ps1` - PowerShell script for Windows
- `docfx.json` - DocFX configuration
- `api/index.md` - API documentation landing page

**Prerequisites:**
```bash
# Install DocFX
dotnet tool install -g docfx
```

**Process:**
1. Builds the project in Release configuration
2. Generates XML documentation file
3. Runs `docfx metadata` to extract API information
4. Runs `docfx build` to generate HTML site

**Output:** `_site/` directory containing complete HTML documentation

### Markdown Generation (Python)

**Files:**
- `scripts/generate-docs-markdown.py` - Python script (cross-platform)
- `scripts/generate-docs-markdown.sh` - Bash wrapper script

**Prerequisites:**
- Python 3.x (usually pre-installed on macOS/Linux)

**Process:**
1. Builds the project in Release configuration
2. Parses the generated XML documentation file
3. Generates individual markdown files for each type
4. Creates an index file organized by namespace

**Output:** `docs/api-markdown/` directory with markdown files

## Viewing Documentation

### HTML Documentation

**Local Preview:**
```bash
docfx serve _site
# Open http://localhost:8080
```

**Features:**
- Full-text search
- Namespace/type navigation tree
- Cross-references between types
- Dark/light theme
- Responsive design

### Markdown Documentation

**View Locally:**
- Open `docs/api-markdown/index.md` in any markdown viewer
- Use VSCode, Typora, or GitHub's markdown preview
- Or use tools like `grip`: `grip docs/api-markdown/index.md`

**Features:**
- Simple, portable format
- Easy to read and version control
- Compatible with GitHub, GitLab, etc.

## Publishing Documentation

### GitHub Pages

1. Generate HTML documentation:
   ```bash
   ./scripts/generate-docs-html.sh
   ```

2. Push to `gh-pages` branch:
   ```bash
   git add _site -f
   git subtree push --prefix _site origin gh-pages
   ```

3. Enable GitHub Pages in repository settings pointing to `gh-pages` branch

### CI/CD Integration

**GitHub Actions Example:**

```yaml
name: Generate Documentation

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  docs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Install DocFX
        run: dotnet tool install -g docfx

      - name: Generate Documentation
        run: ./scripts/generate-docs-html.sh

      - name: Deploy to GitHub Pages
        if: github.ref == 'refs/heads/main'
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./_site
```

## Maintenance

### Adding Documentation to New Code

When adding new public APIs:

1. **Write XMLDoc comments** following the patterns in this guide
2. **Build and check warnings** to ensure proper formatting:
   ```bash
   dotnet build --configuration Release
   ```
3. **Test documentation generation** to verify output:
   ```bash
   ./scripts/generate-docs-markdown.sh
   ```

### Updating Existing Documentation

1. Edit XMLDoc comments in source files
2. Rebuild the project
3. Regenerate documentation
4. Review changes in generated files

### Removing CS1591 Warning Suppression

Once all public APIs are documented:

1. Remove `<NoWarn>$(NoWarn);CS1591</NoWarn>` from `Weaviate.Client.csproj`
2. Build and fix any remaining warnings about missing documentation
3. Enable this as a CI check to enforce documentation on new code

## Troubleshooting

### XML File Not Generated

**Problem:** Documentation scripts report missing XML file

**Solution:**
1. Ensure `GenerateDocumentationFile` is `true` in project file
2. Build in Release configuration: `dotnet build --configuration Release`
3. Check for build errors

### DocFX Not Found

**Problem:** `docfx: command not found`

**Solution:**
```bash
dotnet tool install -g docfx
# Or update: dotnet tool update -g docfx
```

### Python Not Found

**Problem:** `python3: command not found` (Windows)

**Solution:**
- Download and install Python from [python.org](https://www.python.org/downloads/)
- Ensure "Add Python to PATH" is checked during installation

### Warnings About Missing Documentation

**Problem:** CS1591 warnings during build

**Status:** Expected - these indicate undocumented public APIs

**Solution:**
- Add XMLDoc comments to the reported types/members
- Or keep `CS1591` suppressed in `NoWarn` (current configuration)

## Resources

### Documentation Tools

- **DocFX:** https://dotnet.github.io/docfx/
- **XMLDoc Guide:** https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/
- **Markdown Guide:** https://www.markdownguide.org/

### Example Documentation

See these files for well-documented examples:
- `src/Weaviate.Client/Models/VectorData.cs` - Vector types
- `src/Weaviate.Client/Models/Filter.cs` - Filter API
- `src/Weaviate.Client/Models/Sort.cs` - Sort API
- `src/Weaviate.Client/Models/Batch.cs` - Batch operations

## Contributing

When contributing documentation:

1. Follow the patterns established in existing documentation
2. Include examples for complex APIs
3. Cross-reference related types using `<see cref=""/>`
4. Test documentation generation before submitting PR
5. Ensure all new public APIs have XMLDoc comments

---

For questions or issues related to documentation, please file an issue on the GitHub repository.
