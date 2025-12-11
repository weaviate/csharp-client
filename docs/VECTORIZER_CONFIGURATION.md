# Vectorizer Configuration Guide

This guide documents the complete process of creating and configuring vectorizers in the Weaviate C# Client using `Configure.Vectors` and `Configure.MultiVectors`.

## Table of Contents

- [Overview](#overview)
- [Basic Concepts](#basic-concepts)
- [Configure.Vectors API](#configurevectors-api)
- [Configure.MultiVectors API](#configuremultivectors-api)
- [Vector Index Configuration](#vector-index-configuration)
- [Quantizer Configuration](#quantizer-configuration)
- [Available Vectorizers](#available-vectorizers)
  - [Self-Provided Vectorizers](#self-provided-vectorizers)
  - [Text Vectorizers](#text-vectorizers)
  - [Multi-Media Vectorizers](#multi-media-vectorizers)
  - [Image Vectorizers](#image-vectorizers)
  - [Reference Vectorizers](#reference-vectorizers)
  - [Multi-Vector Vectorizers](#multi-vector-vectorizers)
- [Advanced Usage](#advanced-usage)
- [Examples](#examples)

## Overview

The Weaviate C# Client provides a fluent API for configuring vectorizers through the `Configure.Vectors` and `Configure.MultiVectors` static classes. These classes expose factory methods that return a `VectorConfigBuilder`, which can then be used to create `VectorConfig` instances with the `.New()` method.

## Basic Concepts

### VectorConfig

A `VectorConfig` represents a complete vector configuration with:
- **Name**: Identifier for the vector (default: "default")
- **Vectorizer**: The vectorizer implementation to use
- **VectorIndexConfig**: Index settings (HNSW, Flat, Dynamic)
- **SourceProperties**: Properties to vectorize (optional)

### VectorConfigBuilder Pattern

The builder pattern is used throughout:

```csharp
// 1. Get a builder with vectorizer configuration
var builder = Configure.Vectors.Text2VecOpenAI(model: "text-embedding-3-small");

// 2. Create a VectorConfig with the builder
var vectorConfig = builder.New(
    name: "default",
    sourceProperties: ["title", "description"]
);
```

### VectorType

Vectorizers are classified into two types:
- **Single Vector**: Standard vectorizers that produce one vector per object (default)
- **Multi-Vector**: Vectorizers that produce multiple vectors per object (e.g., ColBERT-style embeddings)

## Configure.Vectors API

The `Configure.Vectors` class provides factory methods for single-vector vectorizers.

### VectorConfigBuilder Methods

The `VectorConfigBuilder` class provides several overloaded `.New()` methods:

#### Basic Usage

```csharp
VectorConfig New(string name = "default", params string[] sourceProperties)
```

Creates a vector configuration with basic settings.

**Example:**
```csharp
var config = Configure.Vectors.Text2VecOpenAI(model: "text-embedding-3-small")
    .New("embeddings", "title", "content");
```

#### With HNSW Index

```csharp
VectorConfig New(
    string name,
    VectorIndex.HNSW? indexConfig,
    VectorIndexConfig.QuantizerConfigBase? quantizerConfig = null,
    params string[] sourceProperties
)
```

Creates a vector configuration with HNSW index settings and optional quantization.

**Example:**
```csharp
var config = Configure.Vectors.Text2VecCohere()
    .New(
        name: "default",
        indexConfig: new VectorIndex.HNSW
        {
            Distance = VectorIndexConfig.VectorDistance.Cosine,
            EfConstruction = 128,
            MaxConnections = 64
        },
        quantizerConfig: new VectorIndex.Quantizers.PQ
        {
            Segments = 96,
            Centroids = 256
        },
        sourceProperties: ["description"]
    );
```

#### With Flat Index

```csharp
VectorConfig New(
    string name,
    VectorIndex.Flat? indexConfig,
    VectorIndexConfig.QuantizerConfigFlat? quantizerConfig = null,
    params string[] sourceProperties
)
```

Creates a vector configuration with Flat index (supports only BQ quantization).

**Example:**
```csharp
var config = Configure.Vectors.Text2VecTransformers()
    .New(
        name: "flat_vec",
        indexConfig: new VectorIndex.Flat
        {
            Distance = VectorIndexConfig.VectorDistance.Cosine
        },
        quantizerConfig: new VectorIndex.Quantizers.BQ
        {
            Cache = true,
            RescoreLimit = 200
        },
        sourceProperties: ["text"]
    );
```

#### With Dynamic Index

```csharp
VectorConfig New(
    string name,
    VectorIndex.Dynamic? indexConfig,
    params string[] sourceProperties
)
```

Creates a vector configuration with Dynamic index (switches between HNSW and Flat based on object count).

**Example:**
```csharp
var config = Configure.Vectors.Text2VecOpenAI()
    .New(
        name: "dynamic_vec",
        indexConfig: new VectorIndex.Dynamic
        {
            Distance = VectorIndexConfig.VectorDistance.Cosine,
            Threshold = 10000,
            Hnsw = new VectorIndex.HNSW { EfConstruction = 128 },
            Flat = new VectorIndex.Flat()
        },
        sourceProperties: ["content"]
    );
```

### Important Notes

- Quantizer configurations cannot be set both on the index config AND passed as a separate parameter - doing so will throw a `WeaviateClientException`
- Flat index only supports BQ quantization
- Dynamic index must specify quantizers in their respective sub-configurations (HNSW/Flat)
- If no name is provided or an empty string is passed, "default" is used

## Configure.MultiVectors API

The `Configure.MultiVectors` class provides factory methods for multi-vector vectorizers (e.g., ColBERT-style embeddings).

### VectorConfigBuilder for MultiVectors

Multi-vector configurations require additional settings:

```csharp
VectorConfig New(
    string name = "default",
    VectorIndex.HNSW? indexConfig = null,
    QuantizerConfigBase? quantizerConfig = null,
    params string[] sourceProperties
)
```

**Key Differences:**
- Automatically ensures `MultiVector` configuration is present on HNSW index
- Defaults to HNSW index with MultiVector settings if none provided
- Only supports HNSW index type

**Example:**
```csharp
var config = Configure.MultiVectors.Text2MultiVecJinaAI(
        model: "jina-colbert-v2"
    )
    .New(
        name: "colbert",
        indexConfig: new VectorIndex.HNSW
        {
            MultiVector = new VectorIndexConfig.MultiVectorConfig
            {
                Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                Encoding = new VectorIndexConfig.MuveraEncoding
                {
                    KSim = 4,
                    DProjections = 16,
                    Repetitions = 10
                }
            }
        },
        sourceProperties: ["content"]
    );
```

### Multi-Vector Configuration

Multi-vector configurations require the `MultiVector` property on the HNSW index:

```csharp
public record MultiVectorConfig
{
    // Aggregation method for combining multiple vectors during search
    public string? Aggregation { get; init; } = "maxSim";
    
    // Encoding configuration for compressing multi-vectors
    public EncodingConfig? Encoding { get; init; } = new MuveraEncoding();
}
```

#### Aggregation

- **maxSim**: Maximum similarity aggregation (default and recommended)

#### Encoding

Multi-vectors can use Muvera encoding for compression:

```csharp
public record MuveraEncoding : EncodingConfig
{
    public double? KSim { get; init; } = 4;           // Number of similar vectors to compare
    public double? DProjections { get; init; } = 16;   // Projection dimensions
    public double? Repetitions { get; init; } = 10;    // Number of encoding repetitions
}
```

## Vector Index Configuration

### Index Types

#### HNSW (Hierarchical Navigable Small World)

The most common index type, providing fast approximate nearest neighbor search.

```csharp
public sealed record HNSW : VectorIndexConfig
{
    public int? CleanupIntervalSeconds { get; set; }
    public VectorDistance? Distance { get; set; }
    public int? DynamicEfMin { get; set; }
    public int? DynamicEfMax { get; set; }
    public int? DynamicEfFactor { get; set; }
    public int? EfConstruction { get; set; }
    public int? Ef { get; set; }
    public VectorIndexFilterStrategy? FilterStrategy { get; set; }
    public int? FlatSearchCutoff { get; set; }
    public int? MaxConnections { get; set; }
    public bool? Skip { get; set; }
    public long? VectorCacheMaxObjects { get; set; }
    public QuantizerConfigBase? Quantizer { get; set; }
    public MultiVectorConfig? MultiVector { get; set; }
    public bool? SkipDefaultQuantization { get; set; }
}
```

**Key Properties:**
- **Distance**: Distance metric (Cosine, Dot, L2Squared, Hamming)
- **EfConstruction**: Controls index build quality (higher = better recall, slower build)
- **MaxConnections**: Max connections per node (higher = better recall, more memory)
- **Ef**: Controls search quality at query time
- **Quantizer**: Compression settings (BQ, PQ, SQ, RQ)
- **MultiVector**: Multi-vector configuration (for ColBERT-style embeddings)

#### Flat (Brute Force)

Exact nearest neighbor search with optional BQ quantization.

```csharp
public sealed record Flat : VectorIndexConfig
{
    public VectorDistance? Distance { get; set; }
    public long? VectorCacheMaxObjects { get; set; }
    public QuantizerConfigFlat? Quantizer { get; set; }  // Only BQ or RQ
}
```

**Use Cases:**
- Small datasets where exact search is preferred
- When recall must be 100%
- Development/testing

#### Dynamic

Automatically switches between HNSW and Flat based on object count.

```csharp
public sealed record Dynamic : VectorIndexConfig
{
    public VectorDistance? Distance { get; set; }
    public int? Threshold { get; set; }
    public VectorIndex.HNSW? Hnsw { get; set; }
    public VectorIndex.Flat? Flat { get; set; }
}
```

**Key Properties:**
- **Threshold**: Number of objects at which to switch from Flat to HNSW
- **Hnsw**: HNSW configuration to use after threshold
- **Flat**: Flat configuration to use before threshold

### Distance Metrics

```csharp
public enum VectorDistance
{
    Cosine,      // Cosine similarity (normalized)
    Dot,         // Dot product
    L2Squared,   // Squared Euclidean distance
    Hamming      // Hamming distance (for binary vectors)
}
```

### Filter Strategies

```csharp
public enum VectorIndexFilterStrategy
{
    Sweeping,    // Pre-filter approach
    Acorn        // Post-filter approach (faster for selective filters)
}
```

## Quantizer Configuration

Quantizers compress vectors to reduce memory usage and improve search speed.

### BQ (Binary Quantization)

Compresses vectors to binary (1 bit per dimension).

```csharp
public record BQ : QuantizerConfigFlat
{
    public bool Cache { get; set; }
    public int RescoreLimit { get; set; }
}
```

**Use Cases:**
- High-dimensional vectors
- When 95%+ recall is acceptable
- Memory-constrained environments

**Supported By:** HNSW, Flat

### PQ (Product Quantization)

Divides vectors into segments and quantizes each segment.

```csharp
public record PQ : QuantizerConfigBase
{
    public bool BitCompression { get; set; }
    public int Centroids { get; set; }
    public EncoderConfig? Encoder { get; set; }
    public int Segments { get; set; }
    public int TrainingLimit { get; set; }
}

public record EncoderConfig
{
    public EncoderType Type { get; set; }          // Kmeans or Tile
    public DistributionType Distribution { get; set; } // Normal or LogNormal
}
```

**Use Cases:**
- Balancing compression ratio and recall
- Large-scale deployments
- When some recall trade-off is acceptable

**Supported By:** HNSW only

### SQ (Scalar Quantization)

Quantizes each dimension independently to 8-bit integers.

```csharp
public record SQ : QuantizerConfigBase
{
    public int RescoreLimit { get; set; }
    public int TrainingLimit { get; set; }
}
```

**Use Cases:**
- Better recall than BQ
- Lower compression than BQ
- Good balance between quality and size

**Supported By:** HNSW only

### RQ (Residual Quantization)

Multi-stage quantization for better accuracy.

```csharp
public record RQ : QuantizerConfigFlat
{
    public int RescoreLimit { get; set; }
    public int? Bits { get; set; }
    public bool Cache { get; set; }
}
```

**Use Cases:**
- When high recall is critical
- Larger memory budget than BQ
- Better quality than single-stage quantization

**Supported By:** HNSW, Flat

### Quantizer Selection Guide

| Quantizer | Compression | Recall | Speed | Index Support |
|-----------|-------------|--------|-------|---------------|
| BQ        | Highest     | Good   | Fastest | HNSW, Flat |
| RQ        | High        | Better | Fast | HNSW, Flat |
| SQ        | Medium      | Better | Medium | HNSW |
| PQ        | High        | Good   | Fast | HNSW |

## Available Vectorizers

### Self-Provided Vectorizers

Use when you provide your own vectors.

#### SelfProvided (Configure.Vectors)

```csharp
var config = Configure.Vectors.SelfProvided().New("default");
```

**Wire Format:** `none`  
**Type:** Single Vector

#### SelfProvided (Configure.MultiVectors)

```csharp
var config = Configure.MultiVectors.SelfProvided().New("colbert");
```

**Wire Format:** `none`  
**Type:** Multi-Vector

### Text Vectorizers

Text-only vectorization modules.

#### Text2VecOpenAI

OpenAI embedding models.

```csharp
var config = Configure.Vectors.Text2VecOpenAI(
    baseURL: null,
    dimensions: 1536,
    model: "text-embedding-3-small",
    modelVersion: null,
    type: null,
    vectorizeCollectionName: true
).New("default", "title", "content");
```

**Wire Format:** `text2vec-openai`  
**Type:** Single Vector

**Parameters:**
- `baseURL`: Custom API endpoint (optional)
- `dimensions`: Embedding dimensions (model-dependent)
- `model`: Model name (e.g., "text-embedding-3-small")
- `modelVersion`: Specific model version (optional)
- `type`: Embedding type (optional)
- `vectorizeCollectionName`: Include collection name in vectorization

#### Text2VecAzureOpenAI

Azure OpenAI embedding models.

```csharp
var config = Configure.Vectors.Text2VecAzureOpenAI(
    deploymentId: "my-deployment",
    resourceName: "my-resource",
    baseURL: null,
    dimensions: 1536,
    model: "text-embedding-ada-002",
    vectorizeCollectionName: true
).New("default", "text");
```

**Wire Format:** `text2vec-azure-openai`  
**Type:** Single Vector

**Required Parameters:**
- `deploymentId`: Azure deployment identifier
- `resourceName`: Azure resource name

#### Text2VecCohere

Cohere embedding models.

```csharp
var config = Configure.Vectors.Text2VecCohere(
    baseURL: null,
    model: "embed-multilingual-v3.0",
    dimensions: null,
    truncate: "END",
    vectorizeCollectionName: true
).New("default", "content");
```

**Wire Format:** `text2vec-cohere`  
**Type:** Single Vector

**Parameters:**
- `truncate`: How to truncate long texts ("NONE", "START", "END")

#### Text2VecHuggingFace

HuggingFace embedding models.

```csharp
var config = Configure.Vectors.Text2VecHuggingFace(
    endpointURL: "https://api.huggingface.co/...",
    model: "sentence-transformers/all-MiniLM-L6-v2",
    passageModel: null,
    queryModel: null,
    useCache: true,
    useGPU: false,
    waitForModel: true,
    vectorizeCollectionName: false
).New("default", "text");
```

**Wire Format:** `text2vec-huggingface`  
**Type:** Single Vector

**Parameters:**
- `passageModel`: Model for passages (optional)
- `queryModel`: Model for queries (optional)
- `useCache`: Use model cache
- `useGPU`: Enable GPU acceleration
- `waitForModel`: Wait for model to load

#### Text2VecTransformers

Self-hosted Transformers models.

```csharp
var config = Configure.Vectors.Text2VecTransformers(
    inferenceUrl: "http://localhost:8080",
    passageInferenceUrl: null,
    queryInferenceUrl: null,
    poolingStrategy: "masked_mean",
    dimensions: null,
    vectorizeCollectionName: false
).New("default", "description");
```

**Wire Format:** `text2vec-transformers`  
**Type:** Single Vector

**Parameters:**
- `poolingStrategy`: Pooling method ("cls", "masked_mean", "mean")
- `passageInferenceUrl`: Separate URL for passage encoding
- `queryInferenceUrl`: Separate URL for query encoding

#### Text2VecGoogle (PaLM)

Google Vertex AI embedding models.

```csharp
var config = Configure.Vectors.Text2VecGoogle(
    apiEndpoint: null,
    model: "textembedding-gecko@003",
    projectId: "my-project",
    titleProperty: null,
    dimensions: 768,
    taskType: "RETRIEVAL_DOCUMENT",
    vectorizeCollectionName: true
).New("default", "text");
```

**Wire Format:** `text2vec-palm`  
**Type:** Single Vector

**Parameters:**
- `projectId`: GCP project ID
- `titleProperty`: Property to use as title
- `taskType`: Task type for embeddings

#### Text2VecAWS

AWS Bedrock/Sagemaker embedding models.

```csharp
var config = Configure.Vectors.Text2VecAWS(
    region: "us-east-1",
    service: "bedrock",
    endpoint: null,
    model: "amazon.titan-embed-text-v1",
    targetModel: null,
    targetVariant: null,
    vectorizeCollectionName: true
).New("default", "content");
```

**Wire Format:** `text2vec-aws`  
**Type:** Single Vector

**Required Parameters:**
- `region`: AWS region
- `service`: AWS service ("bedrock" or "sagemaker")

#### Text2VecJinaAI

Jina AI embedding models.

```csharp
var config = Configure.Vectors.Text2VecJinaAI(
    model: "jina-embeddings-v2-base-en",
    baseURL: null,
    dimensions: 768,
    vectorizeCollectionName: true
).New("default", "text");
```

**Wire Format:** `text2vec-jinaai`  
**Type:** Single Vector

#### Text2VecVoyageAI

VoyageAI embedding models.

```csharp
var config = Configure.Vectors.Text2VecVoyageAI(
    baseURL: null,
    model: "voyage-large-2",
    truncate: false,
    dimensions: 1536,
    vectorizeCollectionName: true
).New("default", "content");
```

**Wire Format:** `text2vec-voyageai`  
**Type:** Single Vector

#### Text2VecOllama

Ollama local embedding models.

```csharp
var config = Configure.Vectors.Text2VecOllama(
    apiEndpoint: "http://localhost:11434",
    model: "llama2",
    vectorizeCollectionName: false
).New("default", "text");
```

**Wire Format:** `text2vec-ollama`  
**Type:** Single Vector

#### Text2VecWeaviate

Weaviate's managed embedding service.

```csharp
var config = Configure.Vectors.Text2VecWeaviate(
    baseURL: null,
    dimensions: 768,
    model: "ada",
    vectorizeCollectionName: true
).New("default", "content");
```

**Wire Format:** `text2vec-weaviate`  
**Type:** Single Vector

#### Text2VecDatabricks

Databricks embedding models.

```csharp
var config = Configure.Vectors.Text2VecDatabricks(
    endpoint: "https://...",
    instruction: null,
    vectorizeCollectionName: true
).New("default", "text");
```

**Wire Format:** `text2vec-databricks`  
**Type:** Single Vector

**Required Parameters:**
- `endpoint`: Databricks model endpoint

#### Text2VecNvidia

NVIDIA NIM embedding models.

```csharp
var config = Configure.Vectors.Text2VecNvidia(
    baseURL: null,
    model: "nvidia/nv-embed-v1",
    truncate: false,
    vectorizeCollectionName: true
).New("default", "content");
```

**Wire Format:** `text2vec-nvidia`  
**Type:** Single Vector

#### Text2VecMistral

Mistral AI embedding models.

```csharp
var config = Configure.Vectors.Text2VecMistral(
    baseURL: null,
    model: "mistral-embed",
    vectorizeCollectionName: true
).New("default", "text");
```

**Wire Format:** `text2vec-mistral`  
**Type:** Single Vector

#### Text2VecMorph

Morph Labs embedding models.

```csharp
var config = Configure.Vectors.Text2VecMorph(
    baseURL: null,
    model: "morph-1",
    vectorizeCollectionName: true
).New("default", "content");
```

**Wire Format:** `text2vec-morph`  
**Type:** Single Vector

#### Text2VecModel2Vec

Model2Vec embedding models (lightweight embeddings).

```csharp
var config = Configure.Vectors.Text2VecModel2Vec(
    inferenceURL: "http://localhost:8080",
    vectorizeCollectionName: false
).New("default", "text");
```

**Wire Format:** `text2vec-model2vec`  
**Type:** Single Vector

### Multi-Media Vectorizers

Vectorizers that handle multiple modalities (text, images, video, etc.).

#### Multi2VecClip

OpenAI CLIP for text and images.

```csharp
var config = Configure.Vectors.Multi2VecClip(
    inferenceUrl: "http://localhost:8080",
    imageFields: ["image"],
    textFields: ["title", "description"],
    vectorizeCollectionName: false
).New("default");
```

**Wire Format:** `multi2vec-clip`  
**Type:** Single Vector

**With Weighted Fields:**
```csharp
var weightedFields = new WeightedFields
{
    ("title", 0.7),
    ("description", 0.3)
};

var config = Configure.Vectors.Multi2VecClip(
    imageFields: new WeightedFields { ("image", 1.0) },
    textFields: weightedFields
).New("default");
```

#### Multi2VecBind

ImageBind for multiple modalities.

```csharp
var config = Configure.Vectors.Multi2VecBind(
    audioFields: ["audio"],
    depthFields: ["depth_map"],
    imageFields: ["photo"],
    imuFields: ["imu_data"],
    textFields: ["description"],
    thermalFields: ["thermal_image"],
    videoFields: ["video"],
    vectorizeCollectionName: false
).New("default");
```

**Wire Format:** `multi2vec-bind`  
**Type:** Single Vector

**Supported Modalities:**
- Audio
- Depth images
- Images
- IMU (Inertial Measurement Unit) data
- Text
- Thermal images
- Video

**With Weights:**
```csharp
var config = Configure.Vectors.Multi2VecBind(
    audioFields: new WeightedFields { ("audio", 0.4) },
    textFields: new WeightedFields { ("description", 0.6) }
).New("default");
```

#### Multi2VecGoogle

Google Vertex AI multi-modal embeddings.

```csharp
var config = Configure.Vectors.Multi2VecGoogle(
    projectId: "my-project",
    location: "us-central1",
    imageFields: ["image"],
    textFields: ["title", "description"],
    videoFields: ["video"],
    videoIntervalSeconds: 2,
    model: "multimodalembedding",
    dimensions: 512,
    vectorizeCollectionName: true
).New("default");
```

**Wire Format:** `multi2vec-palm`  
**Type:** Single Vector

**Required Parameters:**
- `projectId`: GCP project ID
- `location`: GCP location

**Parameters:**
- `videoIntervalSeconds`: Interval for video frame sampling

#### Multi2VecCohere

Cohere multi-modal embeddings.

```csharp
var config = Configure.Vectors.Multi2VecCohere(
    baseURL: null,
    imageFields: ["photo"],
    model: "embed-english-v3.0",
    dimensions: null,
    textFields: ["title", "content"],
    truncate: "END",
    vectorizeCollectionName: true
).New("default");
```

**Wire Format:** `multi2vec-cohere`  
**Type:** Single Vector

#### Multi2VecAWS

AWS Bedrock multi-modal embeddings.

```csharp
var config = Configure.Vectors.Multi2VecAWS(
    region: "us-east-1",
    model: "amazon.titan-embed-image-v1",
    dimensions: null,
    imageFields: ["image"],
    textFields: ["caption"],
    vectorizeCollectionName: true
).New("default");
```

**Wire Format:** `multi2vec-aws`  
**Type:** Single Vector

#### Multi2VecJinaAI

Jina AI multi-modal embeddings.

```csharp
var config = Configure.Vectors.Multi2VecJinaAI(
    model: "jina-clip-v1",
    baseURL: null,
    dimensions: 768,
    imageFields: ["image"],
    textFields: ["description"],
    vectorizeCollectionName: true
).New("default");
```

**Wire Format:** `multi2vec-jinaai`  
**Type:** Single Vector

#### Multi2VecVoyageAI

VoyageAI multi-modal embeddings.

```csharp
var config = Configure.Vectors.Multi2VecVoyageAI(
    baseURL: null,
    imageFields: ["photo"],
    model: "voyage-multimodal-3",
    textFields: ["caption"],
    truncate: false,
    vectorizeCollectionName: true
).New("default");
```

**Wire Format:** `multi2vec-voyageai`  
**Type:** Single Vector

#### Multi2VecNvidia

NVIDIA multi-modal embeddings.

```csharp
var config = Configure.Vectors.Multi2VecNvidia(
    baseURL: null,
    model: "nvidia/nv-embedqa-e5-v5",
    properties: ["text", "image"],
    truncate: false
).New("default");
```

**Wire Format:** `multi2vec-nvidia`  
**Type:** Single Vector

### Image Vectorizers

Image-only vectorization.

#### Img2VecNeural

Neural network image embeddings.

```csharp
var config = Configure.Vectors.Img2VecNeural(
    imageFields: ["image", "thumbnail"]
).New("default");
```

**Wire Format:** `img2vec-neural`  
**Type:** Single Vector

**Required Parameters:**
- `imageFields`: Array of property names containing images (DataType.BLOB)

### Reference Vectorizers

Create vectors based on referenced objects.

#### Ref2VecCentroid

Calculate centroid of referenced object vectors.

```csharp
var config = Configure.Vectors.Ref2VecCentroid(
    referenceProperties: ["hasAuthor", "inCategory"],
    method: "mean"
).New("default");
```

**Wire Format:** `ref2vec-centroid`  
**Type:** Single Vector

**Required Parameters:**
- `referenceProperties`: Array of reference property names

**Parameters:**
- `method`: Aggregation method ("mean" is default and recommended)

### Multi-Vector Vectorizers

Vectorizers that produce multiple vectors per object (e.g., ColBERT-style embeddings).

#### Text2MultiVecJinaAI

Jina AI multi-vector text embeddings.

```csharp
var config = Configure.MultiVectors.Text2MultiVecJinaAI(
    model: "jina-colbert-v2",
    baseURL: null,
    dimensions: 128,
    vectorizeCollectionName: false
).New(
    name: "colbert",
    indexConfig: new VectorIndex.HNSW
    {
        MultiVector = new VectorIndexConfig.MultiVectorConfig
        {
            Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
            Encoding = new VectorIndexConfig.MuveraEncoding()
        }
    },
    sourceProperties: ["content"]
);
```

**Wire Format:** `text2multivec-jinaai`  
**Type:** Multi-Vector

#### Multi2MultiVecJinaAI

Jina AI multi-vector multi-modal embeddings.

```csharp
var config = Configure.MultiVectors.Multi2MultiVecJinaAI(
    baseURL: null,
    model: "jina-clip-colbert-v1",
    imageFields: ["image"],
    textFields: ["description"],
    vectorizeCollectionName: false
).New(
    name: "colbert",
    sourceProperties: ["image", "description"]
);
```

**Wire Format:** `multi2multivec-jinaai`  
**Type:** Multi-Vector

**With Weighted Fields:**
```csharp
var config = Configure.MultiVectors.Multi2MultiVecJinaAI(
    model: "jina-clip-colbert-v1",
    imageFields: new WeightedFields { ("image", 0.6) },
    textFields: new WeightedFields { ("title", 0.3), ("description", 0.1) }
).New("colbert");
```

## Advanced Usage

### Weighted Fields

Many multi-modal vectorizers support weighted fields to control the influence of different properties:

```csharp
// Create weighted fields
var weightedFields = new WeightedFields
{
    ("title", 0.7),
    ("description", 0.3)
};

// Or using tuple syntax
var fields = new WeightedFields
{
    new WeightedField("title", 0.7),
    new WeightedField("description", 0.3)
};

// Use in vectorizer configuration
var config = Configure.Vectors.Multi2VecClip(
    textFields: weightedFields
).New("default");
```

**Properties:**
- `FieldNames`: Get array of field names
- `Weights`: Get array of weights
- Implicit conversion to `string[]` for field names

### Multiple Vector Configurations

Collections can have multiple named vector configurations:

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Article",
        Properties = new[]
        {
            Property.Text("title"),
            Property.Text("content"),
            Property.Blob("image")
        },
        VectorConfig = new[]
    {
        // Text embeddings
        Configure.Vectors.Text2VecOpenAI(model: "text-embedding-3-small")
            .New("text_vec", "title", "content"),
        
        // Image embeddings
        Configure.Vectors.Img2VecNeural(imageFields: ["image"])
            .New("image_vec"),
        
        // Multi-modal embeddings
        Configure.Vectors.Multi2VecClip(
            textFields: ["title"],
            imageFields: ["image"]
        ).New("clip_vec")
        }
    }
);
```

### Mixing Single and Multi-Vector Configurations

You can combine regular and multi-vector configurations:

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Document",
        Properties = new[] { Property.Text("content") },
        VectorConfig = new[]
    {
        // Regular single vector
        Configure.Vectors.Text2VecOpenAI()
            .New("regular"),
        
        // Multi-vector for fine-grained retrieval
        Configure.MultiVectors.Text2MultiVecJinaAI(model: "jina-colbert-v2")
            .New(
                name: "colbert",
                indexConfig: new VectorIndex.HNSW
                {
                    MultiVector = new VectorIndexConfig.MultiVectorConfig()
                },
                sourceProperties: ["content"]
            )
        }
    }
);
```

### Adding Vector Configurations to Existing Collections

```csharp
var collection = client.Collections.Use("MyCollection");

// Add a new vector configuration
await collection.Config.AddVector(
    Configure.Vectors.Text2VecCohere(model: "embed-english-v3.0")
        .New("cohere_vec", "title", "description")
);
```

**Note:** Requires Weaviate 1.31.0 or later.

## Examples

### Example 1: Basic OpenAI Text Embeddings

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Article",
        Properties = new[]
        {
            Property.Text("title"),
            Property.Text("content")
        },
        VectorConfig = Configure.Vectors.Text2VecOpenAI(
                model: "text-embedding-3-small",
                dimensions: 1536
            )
            .New("default", "title", "content")
    }
);
```

### Example 2: HNSW with PQ Quantization

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "LargeDataset",
        Properties = new[] { Property.Text("content") },
        VectorConfig = Configure.Vectors.Text2VecCohere()
        .New(
            name: "default",
            indexConfig: new VectorIndex.HNSW
            {
                Distance = VectorIndexConfig.VectorDistance.Cosine,
                EfConstruction = 128,
                MaxConnections = 64
            },
            quantizerConfig: new VectorIndex.Quantizers.PQ
            {
                Segments = 96,
                Centroids = 256,
                Encoder = new VectorIndex.Quantizers.PQ.EncoderConfig
                {
                    Type = VectorIndex.Quantizers.EncoderType.Kmeans,
                    Distribution = VectorIndex.Quantizers.DistributionType.Normal
                }
            },
            sourceProperties: ["content"]
        )
    }
);
```

### Example 3: Multi-Modal with CLIP

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Product",
        Properties = new[]
        {
            Property.Text("name"),
            Property.Text("description"),
            Property.Blob("image")
        },
        VectorConfig = Configure.Vectors.Multi2VecClip(
            inferenceUrl: "http://localhost:8080",
            textFields: new WeightedFields
            {
                ("name", 0.7),
                ("description", 0.3)
            },
            imageFields: new WeightedFields { ("image", 1.0) }
        )
        .New("default")
    }
);
```

### Example 4: ColBERT-Style Multi-Vector

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Document",
        Properties = new[] { Property.Text("content") },
        VectorConfig = Configure.MultiVectors.Text2MultiVecJinaAI(
            model: "jina-colbert-v2",
            dimensions: 128
        )
        .New(
            name: "colbert",
            indexConfig: new VectorIndex.HNSW
            {
                Distance = VectorIndexConfig.VectorDistance.Cosine,
                MultiVector = new VectorIndexConfig.MultiVectorConfig
                {
                    Aggregation = VectorIndexConfig.MultiVectorAggregation.MaxSim,
                    Encoding = new VectorIndexConfig.MuveraEncoding
                    {
                        KSim = 4,
                        DProjections = 16,
                        Repetitions = 10
                    }
                }
            },
            quantizerConfig: new VectorIndex.Quantizers.BQ
            {
                Cache = true,
                RescoreLimit = 200
            },
            sourceProperties: ["content"]
        )
    }
);
```

### Example 5: Multiple Named Vectors

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Article",
        Properties = new[]
        {
            Property.Text("title"),
            Property.Text("content"),
            Property.Text("summary")
        },
        VectorConfig = new[]
    {
        // Fast vector for initial retrieval
        Configure.Vectors.Text2VecOpenAI(model: "text-embedding-3-small")
            .New("fast", "title", "summary"),
        
        // Detailed vector for reranking
        Configure.Vectors.Text2VecOpenAI(model: "text-embedding-3-large")
            .New("detailed", "title", "content"),
        
        // Specialized domain vector
        Configure.Vectors.Text2VecCohere(model: "embed-english-v3.0")
            .New("cohere", "content")
        }
    }
);
```

### Example 6: Dynamic Index with Threshold

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "GrowingDataset",
        Properties = new[] { Property.Text("text") },
        VectorConfig = Configure.Vectors.Text2VecTransformers()
        .New(
            name: "default",
            indexConfig: new VectorIndex.Dynamic
            {
                Distance = VectorIndexConfig.VectorDistance.Cosine,
                Threshold = 10000,
                Flat = new VectorIndex.Flat
                {
                    Distance = VectorIndexConfig.VectorDistance.Cosine,
                    Quantizer = new VectorIndex.Quantizers.BQ
                    {
                        Cache = true,
                        RescoreLimit = 100
                    }
                },
                Hnsw = new VectorIndex.HNSW
                {
                    Distance = VectorIndexConfig.VectorDistance.Cosine,
                    EfConstruction = 128,
                    MaxConnections = 64
                }
            },
            sourceProperties: ["text"]
        )
    }
);
```

### Example 7: Reference-Based Vectors

```csharp
// Create referenced collections first
var authorCollection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Author",
        Properties = new[] { Property.Text("name") },
        VectorConfig = Configure.Vectors.Text2VecOpenAI().New("default", "name")
    }
);

var categoryCollection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Category",
        Properties = new[] { Property.Text("name") },
        VectorConfig = Configure.Vectors.Text2VecOpenAI().New("default", "name")
    }
);

// Article collection uses ref2vec-centroid
var articleCollection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "Article",
        Properties = new[]
        {
            Property.Text("title"),
            Property.CrossReference("hasAuthor", "Author"),
            Property.CrossReference("inCategory", "Category")
        },
        VectorConfig = Configure.Vectors.Ref2VecCentroid(
            referenceProperties: ["hasAuthor", "inCategory"],
            method: "mean"
        )
        .New("default")
    }
);
```

### Example 8: Self-Provided Vectors

```csharp
var collection = await client.Collections.Create(
    new CollectionConfig
    {
        Name = "CustomEmbeddings",
        Properties = new[] { Property.Text("text") },
        VectorConfig = Configure.Vectors.SelfProvided().New("default")
    }
);

// Insert with custom vectors
await collection.Data.Insert(
    properties: new { text = "Hello world" },
    vector: new[] { 0.1f, 0.2f, 0.3f, /* ... */ }
);
```

## Summary

The Weaviate C# Client provides a comprehensive and type-safe API for configuring vectorizers:

1. **Use `Configure.Vectors`** for single-vector configurations
2. **Use `Configure.MultiVectors`** for multi-vector (ColBERT-style) configurations
3. **Chain `.New()`** on the builder to create `VectorConfig` instances
4. **Specify index type** (HNSW, Flat, Dynamic) with detailed settings
5. **Add quantization** (BQ, PQ, SQ, RQ) to reduce memory and improve speed
6. **Use weighted fields** for multi-modal vectorizers to control property influence
7. **Combine multiple vectors** in a single collection for different use cases

All vectorizer configurations follow a consistent pattern, making it easy to switch between different embedding providers while maintaining the same code structure.
