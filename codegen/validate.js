#!/usr/bin/env node

const Ajv = require('ajv');
const fs = require('fs');

console.log('Weaviate Vectorizer Schema Validator');
console.log('====================================\n');

// Load schema
console.log('Loading JSON schema...');
const schema = JSON.parse(fs.readFileSync('vectorizers.schema.json', 'utf8'));

// Load data
console.log('Loading data model...');
const data = JSON.parse(fs.readFileSync('vectorizers.data.json', 'utf8'));

// Validate
console.log('Validating...\n');
const ajv = new Ajv({
  allErrors: true,
  verbose: true,
  strictTypes: false  // Allow union types like ["string", "number", "boolean", "null"]
});
const validate = ajv.compile(schema);

const valid = validate(data);

if (valid) {
  console.log('✓ Validation successful!');
  console.log(`\nValidated ${data.vectorizers.length} vectorizers:`);

  const categories = {};
  data.vectorizers.forEach(v => {
    categories[v.category] = (categories[v.category] || 0) + 1;
  });

  Object.entries(categories).forEach(([category, count]) => {
    console.log(`  - ${category}: ${count}`);
  });

  process.exit(0);
} else {
  console.error('❌ Validation failed!\n');
  console.error('Errors:');
  validate.errors.forEach((error, index) => {
    console.error(`\n${index + 1}. ${error.instancePath || '(root)'}`);
    console.error(`   ${error.message}`);
    if (error.params) {
      console.error(`   ${JSON.stringify(error.params, null, 2)}`);
    }
  });
  process.exit(1);
}
