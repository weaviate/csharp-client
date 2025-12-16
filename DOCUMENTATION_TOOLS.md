# C# Documentation Generation Tools

This document compares different tools available for generating documentation from C# XML comments.

## Available Tools (No Python Required!)

### 1. **DocFX** ⭐ Recommended for HTML
**Official Microsoft tool for .NET documentation**

- **Output:** HTML website with search, navigation, themes
- **Status:** Actively maintained by Microsoft
- **Installation:** `dotnet tool install -g docfx`
- **Pros:**
  - Official Microsoft tool
  - Professional-looking output
  - Full-text search
  - Multiple themes (modern, default)
  - Cross-platform (.NET tool)
  - Used by Microsoft for official docs
- **Cons:**
  - Requires Node.js for some templates
  - Configuration can be complex
- **Scripts:** `scripts/generate-docs-html.sh|ps1`
- **Documentation:** https://dotnet.github.io/docfx/

### 2. **xmldocmd** ⭐ Recommended for Markdown
**.NET tool that generates Markdown directly from XML**

- **Output:** Clean Markdown files
- **Status:** Actively maintained
- **Installation:** `dotnet tool install -g xmldocmd`
- **Pros:**
  - Pure .NET tool (no Python/Node.js)
  - Simple, clean Markdown output
  - Minimal configuration
  - Fast execution
  - Perfect for GitHub/GitLab wikis
  - Cross-platform
- **Cons:**
  - Less feature-rich than DocFX HTML
  - No built-in search (but works great with static site generators)
- **Scripts:** `scripts/generate-docs-xmldocmd.sh|ps1` ✨ NEW
- **Documentation:** https://ejball.com/XmlDocMarkdown/
- **GitHub:** https://github.com/ejball/XmlDocMarkdown

### 3. **Sandcastle Help File Builder (SHFB)**
**Traditional Windows help file generator**

- **Output:** CHM (Compiled Help), HTML, Open XML, Markdown
- **Status:** Actively maintained (latest: December 2024)
- **Installation:** Download from GitHub releases
- **Pros:**
  - Mature, feature-rich
  - GUI and MSBuild integration
  - Multiple output formats
  - Visual Studio integration
  - Can generate CHM files for Windows help
- **Cons:**
  - Windows-only GUI (though command-line works on .NET)
  - More complex setup
  - Larger installation size
- **GitHub:** https://github.com/EWSoftware/SHFB
- **Releases:** https://github.com/EWSoftware/SHFB/releases
- **NuGet:** https://www.nuget.org/packages/EWSoftware.SHFB

### 4. **xmldoc2md**
**Alternative .NET tool for Markdown**

- **Output:** Markdown files
- **Status:** Actively maintained
- **Installation:** `dotnet tool install -g xmldoc2md`
- **Pros:**
  - Simple command-line interface
  - Fast
  - Customizable templates
- **Cons:**
  - Less documentation than xmldocmd
  - Smaller community
- **GitHub:** https://github.com/charlesdevandiere/xmldoc2md
- **Documentation:** https://charlesdevandiere.github.io/xmldoc2md/

### 5. **Doxygen**
**Universal documentation generator**

- **Output:** HTML, LaTeX, RTF, XML
- **Status:** Very mature, actively maintained
- **Installation:** Via package managers (brew, choco, apt)
- **Pros:**
  - Supports many languages (C++, C#, Java, Python, etc.)
  - Highly configurable
  - Many output formats
  - Graph generation (class diagrams, dependencies)
- **Cons:**
  - Configuration is complex
  - Requires additional tools for Markdown output
  - Not .NET-native
- **Website:** https://www.doxygen.nl/
- **For Markdown:** Requires converters like moxygen or doxybook2

## Comparison Matrix

| Tool | Installation | Output | Ease of Use | Quality | Cross-Platform | .NET Native |
|------|-------------|---------|-------------|---------|----------------|-------------|
| **DocFX** | .NET tool | HTML | Medium | Excellent | ✅ | ✅ |
| **xmldocmd** | .NET tool | Markdown | Easy | Very Good | ✅ | ✅ |
| **SHFB** | Installer | CHM/HTML/MD | Medium | Excellent | Partial | ✅ |
| **xmldoc2md** | .NET tool | Markdown | Easy | Good | ✅ | ✅ |
| **Doxygen** | Package | HTML/LaTeX | Hard | Excellent | ✅ | ❌ |

## Recommendations

### For This Project (Weaviate C# Client)

**HTML Documentation (Website/GitHub Pages):**
```bash
# Use DocFX
./scripts/generate-docs-html.sh
```
- Best for: Public API documentation website
- Perfect for: GitHub Pages deployment
- Used by: Microsoft Docs, many .NET libraries

**Markdown Documentation (Simple/Portable):**
```bash
# Use xmldocmd (NEW - No Python!)
./scripts/generate-docs-xmldocmd.sh
```
- Best for: Internal docs, GitHub wiki, simple hosting
- Perfect for: Quick reference, version control
- Easy integration with MkDocs, Jekyll, Hugo

### By Use Case

**I want professional documentation website:**
→ Use **DocFX** (Microsoft's official tool)

**I want simple Markdown files:**
→ Use **xmldocmd** (pure .NET, no dependencies)

**I need Windows CHM help files:**
→ Use **Sandcastle Help File Builder**

**I'm documenting multiple languages:**
→ Use **Doxygen**

**I want the simplest possible setup:**
→ Use **xmldocmd** (one command install, one command generate)

## Quick Start Examples

### DocFX (HTML)
```bash
# Install
dotnet tool install -g docfx

# Generate
./scripts/generate-docs-html.sh

# Serve
docfx serve _site
# Open http://localhost:8080
```

### xmldocmd (Markdown) ✨ NEW
```bash
# Install
dotnet tool install -g xmldocmd

# Generate
./scripts/generate-docs-xmldocmd.sh

# View
open docs/api-markdown/README.md
```

### Sandcastle (Windows)
```bash
# Download from GitHub releases
# Install SandcastleInstaller.exe

# Or via NuGet for build server
nuget install EWSoftware.SHFB -excludeversion

# Build using MSBuild integration or GUI
```

### xmldoc2md (Alternative Markdown)
```bash
# Install
dotnet tool install -g xmldoc2md

# Generate (example)
dotnet build --configuration Release
xmldoc2md src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.dll docs/api
```

## Migration from Python Script

If you were using the Python script (`generate-docs-markdown.py`), you can now use **xmldocmd** instead:

**Old way (Python):**
```bash
python3 scripts/generate-docs-markdown.py [xml-file] [output-dir]
```

**New way (xmldocmd):**
```bash
./scripts/generate-docs-xmldocmd.sh
```

**Benefits:**
- ✅ No Python dependency
- ✅ Native .NET tool
- ✅ Better performance
- ✅ Better formatting
- ✅ More features (namespaces, inheritance, etc.)
- ✅ Actively maintained

## Sources & References

### Tool Documentation
- [DocFX Official Docs](https://dotnet.github.io/docfx/)
- [XmlDocMarkdown (xmldocmd)](https://ejball.com/XmlDocMarkdown/)
- [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB)
- [xmldoc2md](https://charlesdevandiere.github.io/xmldoc2md/)
- [Doxygen Manual](https://www.doxygen.nl/manual/)

### Comparison Resources
- [Slant - Best documentation tools for .NET developers](https://www.slant.co/topics/4111/~documentation-tools-for-net-developers)
- [AlternativeTo - DocFX Alternatives](https://alternativeto.net/software/docfx/)
- [Wikipedia - Comparison of documentation generators](https://en.wikipedia.org/wiki/Comparison_of_documentation_generators)

### Installation Guides
- [Microsoft Learn - Generate XML documentation](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation)
- [Sandcastle Installation Guide](https://www.thebestcsharpprogrammerintheworld.com/2018/08/29/sandcastle-xml-documentation-for-visual-studio-and-c-how-to-install-and-configure-sandcastle/)

## Conclusion

For the **Weaviate C# Client** project, I recommend:

1. **Primary Documentation:** Use **DocFX** for the main HTML documentation site
   - Deploy to GitHub Pages for public access
   - Professional appearance matches project quality

2. **Quick Reference:** Use **xmldocmd** for Markdown documentation
   - Commit to `docs/api-markdown/` for version control
   - Easy to browse on GitHub without building

Both tools are:
- ✅ Pure .NET (no Python, Node.js, or other language dependencies)
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ Easy to install (`dotnet tool install`)
- ✅ Fast and reliable
- ✅ Actively maintained

The Python script can be removed in favor of `xmldocmd`.
