# Weaviate C# Client API Documentation

Welcome to the Weaviate C# Client API documentation. This documentation provides detailed information about all public APIs available in the Weaviate.Client library.

## Overview

The Weaviate C# Client is a comprehensive library for interacting with Weaviate vector databases. It provides:

- **Type-safe API** for all Weaviate operations
- **Async/await support** for modern C# applications
- **Dependency injection integration** for ASP.NET Core and other DI frameworks
- **gRPC and REST** protocol support
- **Strongly-typed queries** with LINQ-style syntax
- **Flexible vector operations** supporting single and multi-vector configurations

## Key Namespaces

### Weaviate.Client
Main entry point containing the `WeaviateClient` class and client builders.

### Weaviate.Client.Models
Data models for collections, properties, filters, vectors, and query results.

### Weaviate.Client.Configure
Configuration factories for vectorizers, generative AI, and rerankers.

### Weaviate.Client.Serialization
Property conversion system for mapping between C# types and Weaviate data types.

### Weaviate.Client.DependencyInjection
Extensions for registering Weaviate clients with Microsoft.Extensions.DependencyInjection.

## Getting Started

See the [README](../README.md) for installation instructions and quick start examples.

For detailed usage guides, see the [docs](../docs/) directory:
- [Client Initialization](../docs/CLIENT_INIT.md)
- [Dependency Injection](../docs/DEPENDENCY_INJECTION.md)
- [Property System](../docs/PROPERTY_SYSTEM.md)
- [Vectorizer Configuration](../docs/VECTORIZER_CONFIGURATION.md)
- [Error Handling](../docs/ERRORS.md)

## API Reference

Browse the API reference using the navigation on the left, or search for specific types and members using the search box.
