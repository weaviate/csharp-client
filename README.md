# Weaviate C# Client

[![NuGet](https://badgen.net/nuget/v/Weaviate.Client?icon=nuget)](https://www.nuget.org/packages/Weaviate.Client)

Welcome to the official C# client for Weaviate, the open-source vector database. This library provides a convenient and idiomatic way for .NET developers to interact with a Weaviate instance.

> [!WARNING]  
> This client is a **beta release and under active development**. We welcome your [feedback](#-feedback) and contributions!

---

## 🚀 Installation

You can install the Weaviate C# client via the NuGet Package Manager or the `dotnet` CLI.

```bash
dotnet add package Weaviate.Client --version 0.0.1-beta.4
```

Alternatively, you can add the `PackageReference` to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Weaviate.Client" Version="0.0.1-beta.4" />
</ItemGroup>
```

---

## ✨ Getting Started

The best way to get started is by following our quickstart guide. It will walk you through setting up the client, connecting to Weaviate, creating a collection, and performing your first vector search.

- **[➡️ Quickstart Guide](https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/quickstart)**

---

## 📚 Documentation

For more detailed information on specific features, please refer to the official documentation and the how-to guides.

- **[Client library overview](https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/client-libraries/csharp)**
- **[How-to: Configure the client](https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/configuration)**
- **[How-to: Manage collections](https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/manage-collections)**
- **[How-to: Manage data objects](https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/manage-objects)**
- **[How-to: Query & search data](https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/search)**

---

## 💬 Feedback

We would love to hear your feedback! For specific feature requests, bug reports, or issues with the client, please open an issue on this repository or reach out to us directly at **devex@weaviate.io**.

---

## 🤝 Community

## 🧪 Integration Testing

To run the integration test suite locally you need a Weaviate server at version >= **1.31.0**.

Start a local instance with the helper script (defaults to the minimum supported version):

```bash
./ci/start_weaviate.sh            # uses 1.31.0
# or explicitly
./ci/start_weaviate.sh 1.32.7     # any version >= 1.31.0
```

Run the tests:

```bash
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj
```

Stop the environment when finished:

```bash
./ci/stop_weaviate.sh
```

If the server version is below 1.31.0 the integration tests will be skipped automatically.


Connect with the Weaviate community and the team through our online channels.

- **[Weaviate Forum](https://forum.weaviate.io/)**: For questions and discussions.
- **[Weaviate Slack](https://weaviate.io/slack)**: For live chats with the community and team.
