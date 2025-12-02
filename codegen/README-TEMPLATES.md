# Template-Based Code Generation

This directory contains a **template-based code generator** for Weaviate vectorizer configurations using Handlebars templates and JSON Schema validation.

## Why Template-Based?

✅ **Better separation**: Data (JSON) and presentation (templates) are completely separated
✅ **Language agnostic**: Same data file generates code for any language
✅ **Standard tooling**: Uses industry-standard tools (Handlebars, JSON Schema)
✅ **Easy to maintain**: Templates are easier to read and modify than code generators
✅ **Better debugging**: Generated code is directly visible in templates
✅ **Portable**: Works on any platform with Node.js

## Quick Start

### 1. Install Dependencies

```bash
npm install
```

### 2. Validate Data

```bash
npm run validate
```

### 3. Generate Code

```bash
# Generate C# code
npm run generate:csharp

# Or simply
node generate.js csharp
```

## Architecture

```plaintext
Data (JSON) + Templates (Handlebars) + Config (JSON) → Generated Code
```

### Data Flow

```plaintext
vectorizers.data.json        ← Pure data model (all languages)
        ↓
vectorizers.schema.json      ← Validation rules
        ↓
    [validate]
        ↓
codegen-config.csharp.json   ← C#-specific rules
        ↓
templates/csharp/*.hbs       ← C# templates
        ↓
    [generate]
        ↓
  Generated C# Files
```

## File Structure

```plaintext
codegen/
├── vectorizers.data.json              # Pure data model (shared)
├── vectorizers.schema.json            # JSON Schema for validation
├── codegen-config.csharp.json         # C# generation config
├── codegen-config.python.json         # Python generation config (future)
├── templates/
│   ├── csharp/
│   │   ├── declarations.hbs           # Vectorizer.Declarations.cs
│   │   ├── properties.hbs             # Vectorizer.cs
│   │   ├── configure.hbs              # Configure/Vectorizer.cs
│   │   └── multiVectorConfigure.hbs   # Configure/Vectorizer.Multivector.cs
│   ├── python/
│   │   ├── models.hbs                 # (future)
│   │   └── factories.hbs              # (future)
│   └── typescript/
│       ├── interfaces.hbs             # (future)
│       └── factories.hbs              # (future)
├── generate.js                        # Main generator script
├── validate.js                        # Validation script
├── package.json                       # Node.js dependencies
└── README-TEMPLATES.md                # This file
```

## Data Model

### Basic Vectorizer

```json
{
  "name": "Text2VecOpenAI",
  "identifier": "text2vec-openai",
  "category": "text2vec",
  "description": "Configuration for OpenAI vectorization",
  "properties": [
    {
      "name": "Model",
      "type": "string",
      "required": false,
      "nullable": true,
      "description": "The model to use"
    }
  ]
}
```

### Vectorizer with Nested Types

```json
{
  "name": "Multi2VecClip",
  "identifier": "multi2vec-clip",
  "category": "multi2vec",
  "properties": [
    {
      "name": "Weights",
      "type": "Weights",
      "required": false,
      "nullable": true
    }
  ],
  "nestedTypes": [
    {
      "name": "Weights",
      "description": "Weights configuration",
      "properties": [
        {
          "name": "ImageFields",
          "type": "double[]",
          "required": false,
          "nullable": true
        }
      ]
    }
  ]
}
```

This generates:

```csharp
public partial record Multi2VecClip : VectorizerConfig
{
    public Weights? Weights { get; set; } = null;

    public record Weights
    {
        public double[]? ImageFields { get; set; } = null;
    }
}
```

## Templates

Templates use [Handlebars](https://handlebarsjs.com/) syntax with custom helpers.

### Example Template

```handlebars
{{! templates/csharp/properties.hbs }}
namespace Weaviate.Client.Models;

public static partial class Vectorizer
{
{{#each vectorizers}}
    public partial record {{name}}
    {
{{#each properties}}
        public {{nullableType this}} {{name}} { get; set; }{{defaultValue this}}
{{/each}}
{{#if nestedTypes}}
{{#each nestedTypes}}
        public record {{name}}
        {
{{#each properties}}
            public {{nullableType this}} {{name}} { get; set; }{{defaultValue this}}
{{/each}}
        }
{{/each}}
{{/if}}
    }
{{/each}}
}
```

### Available Helpers

#### Comparison

- `{{#if (eq a b)}}` - Equal
- `{{#if (ne a b)}}` - Not equal
- `{{#if (and a b)}}` - Logical AND
- `{{#if (or a b)}}` - Logical OR
- `{{#if (not a)}}` - Logical NOT

#### String Transformation

- `{{toCamelCase str}}` - Convert to camelCase
- `{{toPascalCase str}}` - Convert to PascalCase
- `{{toSnakeCase str}}` - Convert to snake_case

#### Type Mapping

- `{{mapType type}}` - Map type using config.typeMapping
- `{{nullableType property}}` - Get nullable type string
- `{{defaultValue property}}` - Get default value string

#### Config Access

- `{{getVectorizerConfig name}}` - Get vectorizer-specific config
- `{{getPropertyConfig vectorizerName propName}}` - Get property-specific config
- `{{shouldGenerateFactory name namespace}}` - Check if factory should be generated
- `{{getParameterOrder vectorizer}}` - Get ordered parameters for factory

#### Formatting

- `{{formatDescription text indent}}` - Format multi-line description

## Language Configuration

Each language has its own configuration file specifying:

### C# Config Example

```json
{
  "language": "csharp",
  "outputPaths": {
    "declarations": "../src/.../Vectorizer.Declarations.cs",
    "properties": "../src/.../Vectorizer.cs"
  },
  "typeMapping": {
    "string": "string",
    "int": "int",
    "bool": "bool",
    "double": "double",
    "string[]": "string[]",
    "double[]": "double[]"
  },
  "namingConventions": {
    "class": "PascalCase",
    "property": "PascalCase",
    "parameter": "camelCase"
  },
  "vectorizerOverrides": {
    "Text2VecOpenAI": {
      "factoryMethod": {
        "generate": true,
        "namespace": "Vectors",
        "parameterOrder": ["baseURL", "model"]
      },
      "properties": {
        "Model": {
          "jsonConverter": "CustomConverter"
        }
      }
    }
  }
}
```

## Adding a New Vectorizer

1. **Edit `vectorizers.data.json`**:

```json
{
  "vectorizers": [
    {
      "name": "Text2VecNewProvider",
      "identifier": "text2vec-newprovider",
      "category": "text2vec",
      "description": "Configuration for the new provider",
      "properties": [
        {
          "name": "ApiKey",
          "type": "string",
          "required": true,
          "nullable": false
        }
      ]
    }
  ]
}
```

2. **Optionally configure in `codegen-config.csharp.json`**:

```json
{
  "vectorizerOverrides": {
    "Text2VecNewProvider": {
      "factoryMethod": {
        "generate": true,
        "namespace": "Vectors",
        "parameterOrder": ["apiKey", "model"]
      }
    }
  }
}
```

3. **Validate**:

```bash
npm run validate
```

4. **Generate**:

```bash
npm run generate:csharp
```

That's it! All files are regenerated automatically.

## Adding a New Language

1. **Create config**: `codegen-config.python.json`

```json
{
  "language": "python",
  "outputPaths": {
    "models": "weaviate/vectorizers/models.py"
  },
  "typeMapping": {
    "string": "str",
    "int": "int",
    "bool": "bool",
    "string[]": "list[str]"
  },
  "namingConventions": {
    "property": "snake_case"
  }
}
```

2. **Create templates**: `templates/python/models.hbs`

```handlebars
{{! templates/python/models.hbs }}
from dataclasses import dataclass
from typing import Optional

{{#each vectorizers}}
@dataclass
class {{name}}:
    """{{description}}"""
    identifier: str = "{{identifier}}"
{{#each properties}}
    {{toSnakeCase name}}: Optional[{{mapType type}}] = None
{{/each}}
{{/each}}
```

3. **Generate**:

```bash
node generate.js python
```

## Validation

### Validate Data

```bash
npm run validate
```

This checks:

- JSON syntax is valid
- All required fields are present
- Types match the schema
- References are valid

### Validation Output

```
✓ Validation successful!

Validated 30 vectorizers:
  - text2vec: 15
  - multi2vec: 10
  - img2vec: 1
  - ref2vec: 1
  - none: 1
```

## Workflow

### Making Changes

1. **Edit data**: Modify `vectorizers.data.json`
2. **Validate**: Run `npm run validate`
3. **Generate**: Run `npm run generate:csharp`
4. **Review**: Check generated files
5. **Commit**: Commit both JSON and generated files

### Editing Templates

1. **Edit template**: Modify `.hbs` file in `templates/csharp/`
2. **Regenerate**: Run `npm run generate:csharp`
3. **Review**: Check all generated files
4. **Test**: Run tests to ensure code compiles
5. **Commit**: Commit template and regenerated files

## Debugging

### Template Syntax Errors

```bash
node generate.js csharp
```

Will show compilation errors with line numbers.

### Data Validation Errors

```bash
npm run validate
```

Shows exactly which fields are invalid.

### Inspect Generated Context

Add `{{log this}}` in templates to see available data:

```handlebars
{{#each vectorizers}}
  {{log this}}  {{! Will print vectorizer object }}
{{/each}}
```

## Best Practices

### 1. Keep Data Pure

❌ Don't add C#-specific stuff to `vectorizers.data.json`:

```json
{
  "properties": [
    {
      "name": "Model",
      "jsonConverter": "CustomConverter"  // ❌ C#-specific
    }
  ]
}
```

✅ Put it in `codegen-config.csharp.json`:

```json
{
  "vectorizerOverrides": {
    "Text2VecOpenAI": {
      "properties": {
        "Model": {
          "jsonConverter": "CustomConverter"  // ✅ C#-specific
        }
      }
    }
  }
}
```

### 2. Use Nested Types

❌ Don't create separate vectorizers for helper classes:

```json
{
  "vectorizers": [
    {
      "name": "Multi2VecClipWeights",  // ❌ Helper class as vectorizer
      "identifier": "",
      "category": "helper"
    }
  ]
}
```

✅ Use nested types:

```json
{
  "vectorizers": [
    {
      "name": "Multi2VecClip",
      "nestedTypes": [  // ✅ Nested inside parent
        {
          "name": "Weights",
          "properties": [...]
        }
      ]
    }
  ]
}
```

### 3. Validate Before Committing

Always run:

```bash
npm run validate && npm run generate:csharp
```

### 4. Document Complex Templates

Add comments in templates:

```handlebars
{{! Generate factory methods for vectorizers in the Vectors namespace }}
{{#each vectorizers}}
{{#if (shouldGenerateFactory name "Vectors")}}
  ...
{{/if}}
{{/each}}
```

## Comparison: Old vs New

### Old Approach (C# Code Generator)

```csharp
// VectorizerGenerator.cs (1000+ lines)
public class VectorizerGenerator
{
    public void GenerateDeclarations()
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Weaviate.Client.Models;");
        sb.AppendLine();
        foreach (var vectorizer in _schema.Vectorizers)
        {
            sb.AppendLine($"public partial record {vectorizer.Name}");
            // ... lots of string concatenation
        }
    }
}
```

**Issues**:

- Hard to maintain
- Mixing code with output
- Difficult to visualize output
- Language-specific

### New Approach (Templates)

```handlebars
{{! templates/csharp/declarations.hbs }}
namespace Weaviate.Client.Models;

public static partial class Vectorizer
{
{{#each vectorizers}}
    public partial record {{name}} : VectorizerConfig
    {
        public const string IdentifierValue = "{{identifier}}";

        public {{name}}()
            : base(IdentifierValue) { }
    }
{{/each}}
}
```

**Benefits**:

- Easy to read and maintain
- Output is directly visible
- Reusable helpers
- Language-agnostic data

## Migration from C# Generator

The old C# generator (`VectorizerGenerator.cs`, `Program.cs`) can coexist with the new template-based one during migration:

```bash
# Old way
cd codegen
dotnet run

# New way
npm run generate:csharp
```

Both generate the same output! Once templates are validated, remove the C# generator.

## Troubleshooting

### "Template not found"

- Check template exists in `templates/{language}/`
- Verify filename matches config (without `.hbs`)

### "No output path configured"

- Add mapping in `codegen-config.{lang}.json`:

  ```json
  {
    "outputPaths": {
      "templateName": "path/to/output.cs"
    }
  }
  ```

### "Validation failed"

- Run `npm run validate` to see detailed errors
- Check JSON syntax
- Verify all required fields present

### Generated code doesn't compile

- Check template syntax
- Verify type mappings are correct
- Ensure all referenced types exist

## Future Enhancements

- [ ] Add Python templates
- [ ] Add TypeScript templates
- [ ] Add Java templates
- [ ] Add Go templates
- [ ] Add watch mode for development
- [ ] Add diff preview before writing files
- [ ] Add dry-run mode
- [ ] Generate unit tests
- [ ] Add custom helper plugins
