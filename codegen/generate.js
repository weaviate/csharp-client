#!/usr/bin/env node

const Handlebars = require('handlebars');
const Ajv = require('ajv');
const fs = require('fs');
const path = require('path');
const { glob } = require('glob');

// ============================================================================
// Handlebars Helper Functions
// ============================================================================

// Comparison helpers
Handlebars.registerHelper('eq', (a, b) => a === b);
Handlebars.registerHelper('ne', (a, b) => a !== b);
Handlebars.registerHelper('and', (a, b) => a && b);
Handlebars.registerHelper('or', (a, b) => a || b);
Handlebars.registerHelper('not', (a) => !a);

// String helpers
Handlebars.registerHelper('toCamelCase', (str) => {
  if (!str) return '';
  return str.charAt(0).toLowerCase() + str.slice(1);
});

Handlebars.registerHelper('toPascalCase', (str) => {
  if (!str) return '';
  return str.charAt(0).toUpperCase() + str.slice(1);
});

Handlebars.registerHelper('toSnakeCase', (str) => {
  if (!str) return '';
  return str.replace(/([A-Z])/g, '_$1').toLowerCase().replace(/^_/, '');
});

// Type mapping helper
Handlebars.registerHelper('mapType', function(type, options) {
  const config = options.data.root.config;
  const typeMapping = config.typeMapping || {};

  // Check if it's a basic type
  if (typeMapping[type]) {
    return typeMapping[type];
  }

  // Custom type - keep as is
  return type;
});

// Nullable type helper for C#
Handlebars.registerHelper('nullableType', function(property, options) {
  const config = options.data.root.config;
  const typeMapping = config.typeMapping || {};

  let baseType = typeMapping[property.type] || property.type;

  if (!property.required && property.nullable) {
    // For value types in C#, add ?
    if (['int', 'bool', 'double'].includes(baseType)) {
      return baseType + '?';
    }
    // For reference types, add ? for nullable reference types
    return baseType + '?';
  }

  return baseType;
});

// Default value helper
Handlebars.registerHelper('defaultValue', function(property) {
  if (property.required) {
    return '';
  }

  if (property.defaultValue === null || property.defaultValue === undefined) {
    return ' = null;';
  }

  if (property.type === 'string') {
    return ` = "${property.defaultValue}";`;
  }

  if (property.type === 'bool') {
    return ` = ${property.defaultValue.toString().toLowerCase()};`;
  }

  if (property.type === 'int' || property.type === 'double') {
    return ` = ${property.defaultValue};`;
  }

  return ' = null;';
});

// Get vectorizer config override
Handlebars.registerHelper('getVectorizerConfig', function(vectorizerName, options) {
  const config = options.data.root.config;
  const overrides = config.vectorizerOverrides || {};
  return overrides[vectorizerName] || {};
});

// Get property config override
Handlebars.registerHelper('getPropertyConfig', function(vectorizerName, propertyName, options) {
  const config = options.data.root.config;
  const overrides = config.vectorizerOverrides || {};
  const vectorizerOverride = overrides[vectorizerName] || {};
  const properties = vectorizerOverride.properties || {};
  return properties[propertyName] || {};
});

// Check if should generate factory method
Handlebars.registerHelper('shouldGenerateFactory', function(vectorizerName, namespace, options) {
  const config = options.data.root.config;
  const overrides = config.vectorizerOverrides || {};
  const vectorizerOverride = overrides[vectorizerName] || {};
  const factoryMethod = vectorizerOverride.factoryMethod || config.defaultFactoryMethod || {};

  return factoryMethod.generate !== false &&
         (factoryMethod.namespace === namespace ||
          (namespace === 'MultiVectors' && factoryMethod.generateInMultiVectors));
});

// Get factory method parameter order
Handlebars.registerHelper('getParameterOrder', function(vectorizer, options) {
  const config = options.data.root.config;
  const overrides = config.vectorizerOverrides || {};
  const vectorizerOverride = overrides[vectorizer.name] || {};
  const factoryMethod = vectorizerOverride.factoryMethod || {};

  const parameterOrder = factoryMethod.parameterOrder || [];
  const properties = vectorizer.properties || [];

  // If parameter order is specified, use it
  if (parameterOrder.length > 0) {
    const ordered = [];
    const propertyMap = {};

    // Create a map of properties by name (case-insensitive)
    properties.forEach(prop => {
      propertyMap[prop.name.toLowerCase()] = prop;
    });

    // Add properties in the specified order
    parameterOrder.forEach(paramName => {
      const prop = propertyMap[paramName.toLowerCase()];
      if (prop) {
        ordered.push(prop);
      }
    });

    // Add any remaining properties not in the order
    properties.forEach(prop => {
      if (!ordered.includes(prop)) {
        ordered.push(prop);
      }
    });

    return ordered;
  }

  // Default order: required first, then optional
  const required = properties.filter(p => p.required);
  const optional = properties.filter(p => !p.required);
  return [...required, ...optional];
});

// Format multiline description
Handlebars.registerHelper('formatDescription', function(description, indent) {
  if (!description) return '';

  const lines = description.split('\n');
  const indentStr = ' '.repeat(indent || 0);

  return lines.map(line => `${indentStr}/// ${line.trim()}`).join('\n');
});

// ============================================================================
// Main Generator Function
// ============================================================================

async function generate(language = 'csharp') {
  console.log('Weaviate Vectorizer Code Generator');
  console.log('===================================\n');

  // Load data
  console.log('Loading data model...');
  const data = JSON.parse(fs.readFileSync('vectorizers.data.json', 'utf8'));

  // Load schema
  console.log('Loading JSON schema...');
  const schema = JSON.parse(fs.readFileSync('vectorizers.schema.json', 'utf8'));

  // Validate data against schema
  console.log('Validating data...');
  const ajv = new Ajv({ strictTypes: false });
  const validate = ajv.compile(schema);

  if (!validate(data)) {
    console.error('❌ Validation errors:', validate.errors);
    process.exit(1);
  }
  console.log('✓ Data validated successfully\n');

  // Load language config
  const configPath = `codegen-config.${language}.json`;
  if (!fs.existsSync(configPath)) {
    console.error(`❌ Config file not found: ${configPath}`);
    process.exit(1);
  }

  console.log(`Loading ${language} configuration...`);
  const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));

  // Find all templates for this language
  const templateDir = `templates/${language}`;
  if (!fs.existsSync(templateDir)) {
    console.error(`❌ Template directory not found: ${templateDir}`);
    process.exit(1);
  }

  console.log(`\nGenerating ${language} code...\n`);

  const templateFiles = await glob(`${templateDir}/*.hbs`);

  if (templateFiles.length === 0) {
    console.warn(`⚠ No templates found in ${templateDir}`);
    return;
  }

  // Process each template
  for (const templatePath of templateFiles) {
    const templateName = path.basename(templatePath, '.hbs');
    console.log(`Processing template: ${templateName}`);

    // Read and compile template
    const templateContent = fs.readFileSync(templatePath, 'utf8');
    const template = Handlebars.compile(templateContent);

    // Get output path from config
    const outputPath = config.outputPaths[templateName];
    if (!outputPath) {
      console.warn(`  ⚠ No output path configured for ${templateName}, skipping`);
      continue;
    }

    // Generate code
    const output = template({
      vectorizers: data.vectorizers,
      metadata: data.metadata,
      version: data.version,
      config: config
    });

    // Ensure output directory exists
    const outputDir = path.dirname(outputPath);
    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    // Write file
    fs.writeFileSync(outputPath, output, 'utf8');
    console.log(`  ✓ Generated: ${outputPath}`);
  }

  console.log('\n✓ Code generation completed successfully!');
  console.log(`\nGenerated files for ${language}:`);
  Object.entries(config.outputPaths).forEach(([name, path]) => {
    if (fs.existsSync(path)) {
      console.log(`  - ${path}`);
    }
  });
}

// ============================================================================
// CLI
// ============================================================================

const language = process.argv[2] || 'csharp';

generate(language).catch(error => {
  console.error('❌ Error:', error.message);
  console.error(error.stack);
  process.exit(1);
});
