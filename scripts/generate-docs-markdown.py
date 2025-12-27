#!/usr/bin/env python3
"""
Generate Markdown documentation from C# XML documentation files.
This script parses the generated XML documentation and creates markdown files
for easy reading and integration with documentation sites.
"""

import xml.etree.ElementTree as ET
import os
import sys
import re
from pathlib import Path
from collections import defaultdict


class XmlDocParser:
    """Parser for C# XML documentation files."""

    def __init__(self, xml_file_path):
        """Initialize the parser with an XML documentation file."""
        self.tree = ET.parse(xml_file_path)
        self.root = self.tree.getroot()
        self.members = {}
        self._parse_members()

    def _parse_members(self):
        """Parse all member elements from the XML."""
        for member in self.root.findall('.//member'):
            name = member.get('name', '')
            self.members[name] = member

    def get_member_doc(self, member_name):
        """Get documentation for a specific member."""
        member = self.members.get(member_name)
        if not member:
            return None

        doc = {
            'summary': self._get_text(member, 'summary'),
            'remarks': self._get_text(member, 'remarks'),
            'returns': self._get_text(member, 'returns'),
            'params': self._get_params(member),
            'typeparams': self._get_typeparams(member),
            'exceptions': self._get_exceptions(member),
            'examples': self._get_examples(member),
        }
        return doc

    def _get_text(self, element, tag):
        """Extract text from an XML element."""
        child = element.find(tag)
        if child is not None:
            text = self._element_to_text(child)
            return text.strip()
        return None

    def _get_params(self, element):
        """Extract parameter documentation."""
        params = {}
        for param in element.findall('param'):
            name = param.get('name', '')
            params[name] = self._element_to_text(param).strip()
        return params

    def _get_typeparams(self, element):
        """Extract type parameter documentation."""
        typeparams = {}
        for typeparam in element.findall('typeparam'):
            name = typeparam.get('name', '')
            typeparams[name] = self._element_to_text(typeparam).strip()
        return typeparams

    def _get_exceptions(self, element):
        """Extract exception documentation."""
        exceptions = {}
        for exception in element.findall('exception'):
            cref = exception.get('cref', '')
            exceptions[cref] = self._element_to_text(exception).strip()
        return exceptions

    def _get_examples(self, element):
        """Extract code examples."""
        examples = []
        for example in element.findall('example'):
            examples.append(self._element_to_text(example).strip())
        return examples

    def _element_to_text(self, element):
        """Convert XML element to text, handling nested tags."""
        text = element.text or ''

        for child in element:
            # Handle see/seealso/cref tags
            if child.tag in ['see', 'seealso']:
                cref = child.get('cref', '')
                if cref:
                    # Convert cref to markdown link format
                    link_text = cref.split(':')[-1] if ':' in cref else cref
                    text += f'`{link_text}`'
            elif child.tag == 'code':
                # Format code blocks
                code_text = (child.text or '').strip()
                text += f'\n```csharp\n{code_text}\n```\n'
            elif child.tag == 'para':
                # Paragraph
                text += '\n\n' + self._element_to_text(child)
            elif child.tag == 'paramref':
                # Parameter reference
                name = child.get('name', '')
                text += f'`{name}`'
            elif child.tag == 'typeparamref':
                # Type parameter reference
                name = child.get('name', '')
                text += f'`{name}`'
            else:
                # Recursively process other elements
                text += self._element_to_text(child)

            text += child.tail or ''

        return text

    def get_all_types(self):
        """Get all type (class, interface, enum, struct) members."""
        types = []
        for name in self.members.keys():
            if name.startswith('T:'):
                types.append(name)
        return sorted(types)

    def get_type_members(self, type_name):
        """Get all members (methods, properties, etc.) of a type."""
        prefix = type_name[2:]  # Remove 'T:' prefix
        members = defaultdict(list)

        for name, element in self.members.items():
            if name.startswith('M:') and name.startswith(f'M:{prefix}.'):
                members['methods'].append(name)
            elif name.startswith('P:') and name.startswith(f'P:{prefix}.'):
                members['properties'].append(name)
            elif name.startswith('F:') and name.startswith(f'F:{prefix}.'):
                members['fields'].append(name)
            elif name.startswith('E:') and name.startswith(f'E:{prefix}.'):
                members['events'].append(name)

        return members


class MarkdownGenerator:
    """Generate markdown documentation from parsed XML."""

    def __init__(self, parser, output_dir):
        """Initialize the generator."""
        self.parser = parser
        self.output_dir = Path(output_dir)
        self.output_dir.mkdir(parents=True, exist_ok=True)

    def generate_all(self):
        """Generate markdown for all documented types."""
        types = self.parser.get_all_types()
        index_content = "# API Reference\n\n"
        index_content += "## Namespaces\n\n"

        # Group types by namespace
        namespaces = defaultdict(list)
        for type_name in types:
            full_name = type_name[2:]  # Remove 'T:' prefix
            namespace = '.'.join(full_name.split('.')[:-1])
            type_simple_name = full_name.split('.')[-1]
            namespaces[namespace].append((type_simple_name, type_name, full_name))

        # Generate index
        for namespace in sorted(namespaces.keys()):
            index_content += f"### {namespace}\n\n"
            for type_simple_name, type_full_id, full_name in sorted(namespaces[namespace]):
                file_name = full_name.replace('.', '_').replace('`', '_') + '.md'
                index_content += f"- [{type_simple_name}]({file_name})\n"
            index_content += "\n"

        # Write index
        with open(self.output_dir / 'index.md', 'w', encoding='utf-8') as f:
            f.write(index_content)

        # Generate individual type documentation
        for type_name in types:
            self.generate_type_doc(type_name)

        print(f"Generated markdown documentation in {self.output_dir}")
        print(f"Total types documented: {len(types)}")

    def generate_type_doc(self, type_name):
        """Generate markdown for a single type."""
        full_name = type_name[2:]  # Remove 'T:' prefix
        doc = self.parser.get_member_doc(type_name)

        content = f"# {full_name}\n\n"

        if doc:
            if doc['summary']:
                content += doc['summary'] + "\n\n"

            if doc['remarks']:
                content += "## Remarks\n\n"
                content += doc['remarks'] + "\n\n"

            if doc['typeparams']:
                content += "## Type Parameters\n\n"
                for name, desc in doc['typeparams'].items():
                    content += f"- **{name}**: {desc}\n"
                content += "\n"

            if doc['examples']:
                content += "## Examples\n\n"
                for example in doc['examples']:
                    content += example + "\n\n"

        # Get members
        members = self.parser.get_type_members(type_name)

        if members['properties']:
            content += "## Properties\n\n"
            for prop_name in sorted(members['properties']):
                prop_simple = prop_name.split('.')[-1]
                prop_doc = self.parser.get_member_doc(prop_name)
                content += f"### {prop_simple}\n\n"
                if prop_doc and prop_doc['summary']:
                    content += prop_doc['summary'] + "\n\n"

        if members['methods']:
            content += "## Methods\n\n"
            for method_name in sorted(members['methods']):
                method_simple = method_name.split('.')[-1].split('(')[0]
                method_doc = self.parser.get_member_doc(method_name)
                content += f"### {method_simple}\n\n"
                if method_doc:
                    if method_doc['summary']:
                        content += method_doc['summary'] + "\n\n"
                    if method_doc['params']:
                        content += "**Parameters:**\n\n"
                        for param_name, param_desc in method_doc['params'].items():
                            content += f"- `{param_name}`: {param_desc}\n"
                        content += "\n"
                    if method_doc['returns']:
                        content += f"**Returns:** {method_doc['returns']}\n\n"

        # Write file
        file_name = full_name.replace('.', '_').replace('`', '_') + '.md'
        with open(self.output_dir / file_name, 'w', encoding='utf-8') as f:
            f.write(content)


def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python generate-docs-markdown.py <xml-file> [output-dir]")
        print("")
        print("Example:")
        print("  python generate-docs-markdown.py src/Weaviate.Client/bin/Release/net9.0/Weaviate.Client.xml docs/api-markdown")
        sys.exit(1)

    xml_file = sys.argv[1]
    output_dir = sys.argv[2] if len(sys.argv) > 2 else 'docs/api-markdown'

    if not os.path.exists(xml_file):
        print(f"Error: XML file not found: {xml_file}")
        print("")
        print("Make sure to build the project first to generate XML documentation:")
        print("  dotnet build --configuration Release")
        sys.exit(1)

    print("Weaviate C# Client - Markdown Documentation Generator")
    print("=" * 60)
    print(f"Input XML: {xml_file}")
    print(f"Output directory: {output_dir}")
    print("")

    parser = XmlDocParser(xml_file)
    generator = MarkdownGenerator(parser, output_dir)
    generator.generate_all()

    print("")
    print("=" * 60)
    print("Documentation generation complete!")
    print(f"View the documentation at: {output_dir}/index.md")


if __name__ == '__main__':
    main()
