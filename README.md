# Weaviate C# Client

[Notion Page](https://www.notion.so/weaviate/C-Client-Kickoff-1ac70562ccd680718356e4e0faf99ab6)

## Features

### Helpers

- [x] Connection helpers: Local, Cloud, Custom, FromEnvironment

### Collections

- [x] List collections.
- [x] Create collection.
- [x] Delete collection.
- [x] Get collection configuration.

### Objects

- [x] Insert data.
  - [X] Add object with named vector data.
- [x] Delete data.
- [X] Update data.
- [X] Get object by ID.
- [x] Get objects.
- [x] Get object metadata (vectors, schema, etc.)

### Search

- [X] Query objects over gRPC.
- [X] Perform a search with:
  - Search mode:
    - [X] BM5
    - [X] Hybrid
    - [X] Near vector
  - Pagination:
    - [X] Limit
    - [ ] Offset.
- Filters
  - [X] Property
    - [X] Property Length
  - [X] Creation/Update Time
  - [X] Single-Target References
    - [X] Reference Count
  - [ ] Multi-Target References
  - [ ] Geo Coordinates
