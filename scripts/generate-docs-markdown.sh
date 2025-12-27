#!/bin/bash
# Generate Markdown documentation from XMLDoc comments

set -e

echo "Weaviate C# Client - Markdown Documentation Generator"
echo "======================================================"
echo ""

# Get the script directory and project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Project root: $PROJECT_ROOT"
echo ""

# Navigate to project root
cd "$PROJECT_ROOT"

# Check if Python 3 is installed
if ! command -v python3 &> /dev/null; then
    echo "Error: Python 3 is not installed."
    echo "Please install Python 3 to use this script."
    exit 1
fi

# Build the project to generate XML documentation files
echo "Building project to generate XML documentation..."
dotnet build src/Weaviate.Client/Weaviate.Client.csproj --configuration Release

# Check if XML file was generated
XML_FILE="src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.xml"
if [ ! -f "$XML_FILE" ]; then
    echo "Error: XML documentation file not found at $XML_FILE"
    echo "Make sure GenerateDocumentationFile is set to true in the project file."
    exit 1
fi

echo "XML documentation file generated successfully at $XML_FILE"
echo ""

# Set output directory
OUTPUT_DIR="docs/api-markdown"

# Generate markdown documentation
echo "Generating Markdown documentation..."
python3 "$SCRIPT_DIR/generate-docs-markdown.py" "$XML_FILE" "$OUTPUT_DIR"

echo ""
echo "======================================================"
echo "Markdown documentation generated successfully!"
echo ""
echo "Output location: $PROJECT_ROOT/$OUTPUT_DIR"
echo ""
echo "To view the documentation:"
echo "  1. Open $OUTPUT_DIR/index.md in your favorite markdown viewer"
echo "  2. Or use a markdown preview tool like grip: grip $OUTPUT_DIR/index.md"
echo ""
