# Documentation Generation Scripts

This directory contains scripts for generating documentation from the XMLDoc comments in the Weaviate C# Client codebase.

## Prerequisites

### For HTML Documentation (DocFX)

Install DocFX using one of these methods:

**Option 1: .NET Tool (Recommended)**
```bash
dotnet tool install -g docfx
```

**Option 2: Homebrew (macOS)**
```bash
brew install docfx
```

**Option 3: Chocolatey (Windows)**
```bash
choco install docfx
```

### For Markdown Documentation

- Python 3.x (usually pre-installed on macOS/Linux)
- Windows users: Install Python from [python.org](https://www.python.org/downloads/)

## Scripts

### `generate-docs-html.sh` / `generate-docs-html.ps1`

Generates HTML documentation using DocFX.

**Usage (macOS/Linux):**
```bash
./scripts/generate-docs-html.sh
```

**Usage (Windows PowerShell):**
```powershell
.\scripts\generate-docs-html.ps1
```

**Output:**
- Location: `_site/` directory in project root
- View locally: `docfx serve _site` then open http://localhost:8080
- Or directly open: `_site/index.html`

**Features:**
- Professional HTML documentation with search
- Automatic API reference generation
- Includes all XMLDoc comments
- Navigation tree for all types
- Dark/light theme support

### `generate-docs-markdown.sh` / `generate-docs-markdown.py`

Generates Markdown documentation from XML documentation files.

**Usage (macOS/Linux):**
```bash
./scripts/generate-docs-markdown.sh
```

**Usage (Direct Python):**
```bash
python3 scripts/generate-docs-markdown.py src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.xml docs/api-markdown
```

**Output:**
- Location: `docs/api-markdown/` directory
- Start with: `docs/api-markdown/index.md`
- Each type has its own `.md` file

**Features:**
- Easy to read markdown format
- Organized by namespace
- Includes summaries, parameters, returns, examples
- Can be viewed in any markdown viewer or GitHub

## Workflow

### 1. Build the Project

Both scripts will build the project automatically, but you can also build manually:

```bash
dotnet build src/Weaviate.Client/Weaviate.Client.csproj --configuration Release
```

This generates the XML documentation file at:
```
src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.xml
```

### 2. Generate Documentation

Choose one or both formats:

**HTML (Professional, searchable website):**
```bash
./scripts/generate-docs-html.sh
```

**Markdown (Simple, portable format):**
```bash
./scripts/generate-docs-markdown.sh
```

### 3. View Documentation

**HTML:**
```bash
docfx serve _site
# Then open http://localhost:8080
```

**Markdown:**
```bash
# Open in your favorite markdown viewer
open docs/api-markdown/index.md  # macOS
start docs/api-markdown/index.md # Windows
```

## Publishing Documentation

### GitHub Pages

1. Generate HTML documentation
2. Push `_site` directory to `gh-pages` branch:
   ```bash
   ./scripts/generate-docs-html.sh
   git add _site -f
   git commit -m "Update documentation"
   git subtree push --prefix _site origin gh-pages
   ```

### Other Static Hosting

Copy the contents of `_site` directory to your web server.

## Troubleshooting

### Missing XML Documentation File

If you see an error about missing XML file:
1. Ensure `GenerateDocumentationFile` is set to `true` in `Weaviate.Client.csproj`
2. Build the project in Release mode: `dotnet build --configuration Release`

### DocFX Not Found

Install DocFX using the methods listed in Prerequisites section above.

### Python Not Found

Install Python 3 from:
- macOS: `brew install python3` or download from python.org
- Windows: Download from python.org
- Linux: `sudo apt-get install python3` (Ubuntu/Debian)

## Customization

### Modify HTML Theme

Edit `docfx.json` and change the `template` section:
```json
"template": [
  "default",
  "modern"
]
```

Available templates: `default`, `modern`, `statictoc`

### Modify Markdown Output

Edit `scripts/generate-docs-markdown.py` to customize:
- Output format
- Section ordering
- Filtering logic
- Link generation

## CI/CD Integration

Add to your CI/CD pipeline:

**GitHub Actions:**
```yaml
- name: Generate Documentation
  run: |
    dotnet tool install -g docfx
    ./scripts/generate-docs-html.sh

- name: Deploy to GitHub Pages
  uses: peaceiris/actions-gh-pages@v3
  with:
    github_token: ${{ secrets.GITHUB_TOKEN }}
    publish_dir: ./_site
```

## License

These scripts are part of the Weaviate C# Client project and follow the same license.
