# Code Generation Architecture

## Separation of Concerns

The code generation system now properly separates **data** from **code generation instructions**:

```
┌─────────────────────────┐
│  vectorizers.data.json  │  ← Pure data model (platform-agnostic)
│  - Properties           │
│  - Types                │
│  - Descriptions         │
│  - Requirements         │
└────────────┬────────────┘
             │
             ├──────────────────┬──────────────────┬────────────────┐
             ▼                  ▼                  ▼                ▼
┌───────────────────────┐ ┌──────────────────┐ ┌─────────────┐ ┌──────────────┐
│ codegen-config        │ │ codegen-config   │ │ codegen-... │ │ codegen-...  │
│ .csharp.json          │ │ .python.json     │ │ .java.json  │ │ .go.json     │
│                       │ │                  │ │             │ │              │
│ - Output paths        │ │ - Output paths   │ │ - Packages  │ │ - Packages   │
│ - Type mapping        │ │ - Type mapping   │ │ - Lombok    │ │ - Tags       │
│ - Naming conventions  │ │ - Naming (snake) │ │ - Jackson   │ │ - Options    │
│ - Factory methods     │ │ - Dataclasses    │ │ - Builders  │ │ - Funcs      │
│ - Property overrides  │ │ - Pydantic       │ │             │ │              │
└───────────┬───────────┘ └────────┬─────────┘ └──────┬──────┘ └──────┬───────┘
            │                      │                   │               │
            ▼                      ▼                   ▼               ▼
   ┌────────────────┐     ┌────────────────┐  ┌──────────────┐ ┌─────────────┐
   │ C# Generator   │     │ Python Gen     │  │ Java Gen     │ │ Go Gen      │
   └────────────────┘     └────────────────┘  └──────────────┘ └─────────────┘
```

## File Structure

### 1. `vectorizers.data.json` - Pure Data Model

**Purpose**: Language-agnostic definition of vectorizer properties

**Contains**:

- Vectorizer names and identifiers
- Property definitions (name, type, required, nullable, default)
- Documentation/descriptions
- Deprecation information
- Inheritance relationships

**Example**:

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

### 2. `codegen-config.csharp.json` - C# Code Generation Rules

**Purpose**: C#-specific instructions for code generation

**Contains**:

- Output file paths
- Type mappings (string → string, int → int)
- Naming conventions (PascalCase, camelCase)
- Code style preferences (records, partials, nullable ref types)
- Factory method configurations per vectorizer
- Property-specific overrides (JSON converters, etc.)

**Example**:

```json
{
  "outputPaths": {
    "declarations": "../src/.../Vectorizer.Declarations.cs",
    "properties": "../src/.../Vectorizer.cs"
  },
  "typeMapping": {
    "string": "string",
    "int": "int"
  },
  "vectorizerOverrides": {
    "Text2VecOpenAI": {
      "factoryMethod": {
        "generate": true,
        "namespace": "Vectors",
        "parameterOrder": ["baseURL", "model"]
      }
    }
  }
}
```

### 3. Future: `codegen-config.python.json`

**Example structure**:

```json
{
  "language": "python",
  "outputPaths": {
    "models": "weaviate/vectorizers/models.py",
    "factories": "weaviate/vectorizers/factories.py"
  },
  "typeMapping": {
    "string": "str",
    "int": "int",
    "bool": "bool"
  },
  "namingConventions": {
    "class": "PascalCase",
    "property": "snake_case",
    "parameter": "snake_case"
  },
  "codeStyle": {
    "useDataclasses": true,
    "usePydantic": true,
    "indentation": "    "
  },
  "vectorizerOverrides": {
    "Text2VecOpenAI": {
      "properties": {
        "Model": {
          "pydanticValidator": "Field(min_length=1)"
        }
      }
    }
  }
}
```

## Benefits of This Architecture

### ✅ Clear Separation of Concerns

- **Data** is platform-agnostic
- **Code generation rules** are language-specific
- No mixing of concerns

### ✅ Single Source of Truth

- One `vectorizers.data.json` for all languages
- Add a vectorizer once, generate for all languages

### ✅ Language-Specific Customization

- Each language has its own config file
- Customize type mappings, naming, conventions
- Override specific vectorizers when needed

### ✅ Easy to Extend

- Add new language: create new `codegen-config.{lang}.json`
- No changes to data model needed
- Implement language-specific generator

### ✅ Better Maintainability

- Update data model: edit `vectorizers.data.json`
- Update C# generation: edit `codegen-config.csharp.json`
- Changes are isolated and predictable

## Generator Implementation

The generator reads both files:

```csharp
// Load data model (shared across all languages)
var data = LoadData("vectorizers.data.json");

// Load language-specific config
var config = LoadConfig("codegen-config.csharp.json");

// Merge: apply config rules to data
foreach (var vectorizer in data.Vectorizers)
{
    // Look up language-specific overrides
    if (config.VectorizerOverrides.TryGetValue(vectorizer.Name, out var overrides))
    {
        // Apply factory method config
        // Apply property overrides (converters, etc.)
        // Apply naming conventions
    }

    // Generate code using merged configuration
    GenerateCode(vectorizer, config);
}
```

## Migration Path

To add support for a new language:

1. Create `codegen-config.{language}.json` with your language's rules
2. Implement `{Language}Generator.cs` that reads both files
3. Run generator to produce code for that language

The data model remains unchanged!

## Example: Adding Python Support

1. **Create config**:

```bash
touch codegen-config.python.json
```

2. **Define Python rules**:

```json
{
  "language": "python",
  "typeMapping": {
    "string": "str",
    "int": "int",
    "bool": "bool",
    "string[]": "list[str]",
    "double[]": "list[float]"
  },
  "namingConventions": {
    "property": "snake_case"
  }
}
```

3. **Implement generator**:

```csharp
public class PythonGenerator
{
    public void Generate(VectorizerData data, PythonConfig config)
    {
        // Read same vectorizers.data.json
        // Apply Python-specific rules
        // Generate Python code
    }
}
```

4. **No changes needed** to `vectorizers.data.json`!

## Consistency Across Languages

Because all languages share the same data model, you get automatic consistency:

```
vectorizers.data.json
    ↓
    ├→ C# (via codegen-config.csharp.json)
    ├→ Python (via codegen-config.python.json)
    ├→ TypeScript (via codegen-config.typescript.json)
    ├→ Java (via codegen-config.java.json)
    └→ Go (via codegen-config.go.json)
```

All will have:

- Same vectorizers
- Same properties
- Same documentation
- Same requirements
- Language-appropriate code style

## Validation

The data model can be validated independently:

```bash
# Validate data model structure
ajv validate -s vectorizers.schema.json -d vectorizers.data.json

# Validate C# config structure
ajv validate -s codegen-config.schema.json -d codegen-config.csharp.json
```

## Summary

| File | Purpose | Scope | Changes When... |
|------|---------|-------|----------------|
| `vectorizers.data.json` | Data model | All languages | Adding/modifying vectorizers |
| `codegen-config.csharp.json` | C# generation rules | C# only | Changing C# output format |
| `codegen-config.python.json` | Python generation rules | Python only | Changing Python output format |
| `VectorizerGenerator.cs` | C# code generator | C# only | Fixing C# generation bugs |
| `PythonGenerator.py` | Python code generator | Python only | Fixing Python generation bugs |

This architecture ensures that data and presentation are properly separated, making the system maintainable and extensible.
