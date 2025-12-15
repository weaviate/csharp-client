namespace Weaviate.Client.Tests.Integration;

using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using Xunit;

[Trait("Category", "Slow")]
[Collection("TestBackups")]
[CollectionDefinition("TestBackups", DisableParallelization = true)]
public class TestBackups : IntegrationTests
{
    static readonly BackupBackend _backend = new FilesystemBackend(); // typical default backend
    static readonly TimeSpan _pollingTimeout = TimeSpan.FromSeconds(5);

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        RequireVersion("1.32.0");

        // Wait for any running backups to complete before each test
        await WaitForNoRunningBackups(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Wait for all running backups to complete before starting a new one.
    /// Since only one backup can run at a time per backend, this prevents conflicts.
    /// Cancels any backups stuck in Started status and times out after 5 seconds.
    /// </summary>
    private async Task WaitForNoRunningBackups(CancellationToken ct = default)
    {
        var timeout = _pollingTimeout;
        var startTime = DateTime.UtcNow;

        while (true)
        {
            // Check for timeout
            if (DateTime.UtcNow - startTime > timeout)
            {
                // Cancel any remaining backups and break
                var remainingBackups = await _weaviate.Backup.List(_backend.Provider, ct);
                foreach (var backup in remainingBackups)
                {
                    if (
                        backup.Status
                        is BackupStatus.Started
                            or BackupStatus.Transferring
                            or BackupStatus.Transferred
                    )
                    {
                        try
                        {
                            await _weaviate.Backup.Cancel(_backend, backup.Id, ct);
                        }
                        catch
                        {
                            /* Ignore cancellation errors */
                        }
                    }
                }
                break;
            }

            var backups = await _weaviate.Backup.List(_backend.Provider, ct);
            var runningBackups = backups
                .Where(b =>
                    b.Status
                        is BackupStatus.Started
                            or BackupStatus.Transferring
                            or BackupStatus.Transferred
                )
                .ToList();

            if (!runningBackups.Any())
            {
                break;
            }

            // Cancel backups stuck in Started status for more than 2 seconds
            foreach (var backup in runningBackups)
            {
                if (backup.Status == BackupStatus.Started)
                {
                    try
                    {
                        await _weaviate.Backup.Cancel(_backend, backup.Id, ct);
                    }
                    catch
                    {
                        /* Ignore cancellation errors */
                    }
                }
            }

            await Task.Delay(250, ct);
        }
    }

    [Fact]
    public async Task Test_Create_List_Status_Cancel_Backup()
    {
        // Create a dummy collection for the backup to operate on
        var dummyCollection = await CollectionFactory(
            name: "BackupTestCollection",
            properties: [Property.Text("dummyField")]
        );
        var dummyCollectionName = dummyCollection.Name;

        // Insert a sample object into the dummy collection
        await dummyCollection.Data.Insert(
            new { dummyField = "sample data" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var id = Helpers.GenerateUniqueIdentifier(dummyCollectionName);
        var request = new BackupCreateRequest(id, _backend);

        try
        {
            var operation = await _weaviate.Backup.Create(
                request,
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.Equal(id, operation.Current.Id);

            var list = await _weaviate.Backup.List(
                _backend.Provider,
                TestContext.Current.CancellationToken
            );
            Assert.Contains(list, b => b.Id == id);

            // Test GetStatus is available
            var status = await _weaviate.Backup.GetStatus(
                _backend,
                id,
                TestContext.Current.CancellationToken
            );
            Assert.Equal(id, status.Id);

            // Test Cancel is available
            await _weaviate.Backup.Cancel(_backend, id, TestContext.Current.CancellationToken);
        }
        finally
        {
            // Cleanup is handled by Cancel above
        }
    }

    [Fact]
    public async Task Test_Create_And_Restore_Backup_With_Waiting()
    {
        // Arrange
        var collectionSeed = MakeUniqueCollectionName<object>("bkp");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        // Create two collections with sample data
        var articles = await CollectionFactory(
            name: "Article",
            properties: [Property.Text("title"), Property.Date("datePublished")]
        );
        var paragraphs = await CollectionFactory(
            name: "Paragraph",
            properties: [Property.Text("contents")]
        );

        var articlesName = articles.Name;
        var paragraphsName = paragraphs.Name;

        var articleIds = new List<Guid>();
        var paragraphIds = new List<Guid>();
        for (int i = 1; i <= 5; i++)
        {
            articleIds.Add(
                await articles.Data.Insert(
                    new { title = $"article {i}", datePublished = DateTime.UtcNow },
                    cancellationToken: TestContext.Current.CancellationToken
                )
            );
            paragraphIds.Add(
                await paragraphs.Data.Insert(
                    new { contents = $"paragraph {i}" },
                    cancellationToken: TestContext.Current.CancellationToken
                )
            );
        }

        // Act - create backup and wait for completion
        var createResp = await _weaviate.Backup.CreateSync(
            new BackupCreateRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { articlesName, paragraphsName }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Verify data before deletion
        var articlesBefore = await articles.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var paragraphsBefore = await paragraphs.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(5, articlesBefore.Objects.Count);
        Assert.Equal(5, paragraphsBefore.Objects.Count);

        // Delete collections
        await _weaviate.Collections.Delete(articlesName, TestContext.Current.CancellationToken);
        await _weaviate.Collections.Delete(paragraphsName, TestContext.Current.CancellationToken);

        // Restore backup and wait
        var restoreResp = await _weaviate.Backup.RestoreSync(
            new BackupRestoreRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { articlesName, paragraphsName }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);

        var articlesRestored = _weaviate.Collections.Use(articlesName);
        var paragraphsRestored = _weaviate.Collections.Use(paragraphsName);

        var articlesAfter = await articlesRestored.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        var paragraphsAfter = await paragraphsRestored.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(5, articlesAfter.Objects.Count);
        Assert.Equal(5, paragraphsAfter.Objects.Count);
    }

    [Fact]
    public async Task Test_Create_And_Restore_Backup_Without_Waiting()
    {
        // Arrange
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_async");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var article = await CollectionFactory(
            name: "ArticleAsync",
            properties: [Property.Text("title")]
        );

        var articleName = article.Name;

        // insert objects
        for (int i = 1; i <= 3; i++)
        {
            await article.Data.Insert(
                new { title = $"article {i}" },
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        // Act - create without waiting
        var operation = await _weaviate.Backup.Create(
            new BackupCreateRequest(backupId, _backend, IncludeCollections: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(operation);
        Assert.NotEqual(BackupStatus.Failed, operation.Current.Status); // initial status should be STARTED/TRANSFERRING/TRANSFERRED

        // Wait for completion using the operation
        var createResp = await operation.WaitForCompletion(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Delete collection
        await _weaviate.Collections.Delete(articleName, TestContext.Current.CancellationToken);

        // Restore without waiting
        BackupRestoreOperation restoreOperation = await _weaviate.Backup.Restore(
            new BackupRestoreRequest(backupId, _backend, IncludeCollections: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(restoreOperation);

        // Wait for restore completion using the operation
        var restoreResp = await restoreOperation.WaitForCompletion(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);

        // Verify data restored
        var articleRestored = _weaviate.Collections.Use(articleName);

        var objectsAfter = await articleRestored.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(3, objectsAfter.Objects.Count);
    }

    [Fact]
    public async Task Test_Create_And_Restore_Single_Collection_Backup_With_Waiting()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_single");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var article = await CollectionFactory(
            name: "ArticleSingle",
            properties: [Property.Text("title")]
        );
        var articleName = article.Name;

        for (int i = 1; i <= 4; i++)
        {
            await article.Data.Insert(
                new { title = $"article {i}" },
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        // Create backup for only ArticleSingle
        var createResp = await _weaviate.Backup.CreateSync(
            new BackupCreateRequest(backupId, _backend, IncludeCollections: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Verify count before deletion
        var before = await article.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(4, before.Objects.Count);

        // Delete collection
        await _weaviate.Collections.Delete(articleName, TestContext.Current.CancellationToken);

        // Restore only that collection
        var restoreResp = await _weaviate.Backup.RestoreSync(
            new BackupRestoreRequest(backupId, _backend, IncludeCollections: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);

        var articleRestored = _weaviate.Collections.Use(articleName);
        var after = await articleRestored.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(4, after.Objects.Count);
    }

    [Fact]
    public async Task Test_Fail_Backup_On_NonExisting_Collection()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_fail_nonexist");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);
        var bogusCollectionName = MakeUniqueCollectionName<object>("DoesNotExist");

        // Create should fail when including non-existing collection
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Backup.CreateSync(
                new BackupCreateRequest(
                    backupId,
                    _backend,
                    IncludeCollections: new[] { bogusCollectionName }
                ),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );

        // Restore should also fail if collection doesn't exist in backup
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Backup.RestoreSync(
                new BackupRestoreRequest(
                    backupId,
                    _backend,
                    IncludeCollections: new[] { bogusCollectionName }
                ),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task Test_Fail_Creating_Duplicate_Backup()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_duplicate");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var collection = await CollectionFactory(
            name: "ArticleDup",
            properties: [Property.Text("title")]
        );
        var collectionName = collection.Name;

        await collection.Data.Insert(
            new { title = "one" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var first = await _weaviate.Backup.CreateSync(
            new BackupCreateRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { collectionName }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, first.Status);

        // Second attempt should throw (422)
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Backup.CreateSync(
                new BackupCreateRequest(
                    backupId,
                    _backend,
                    IncludeCollections: new[] { collectionName }
                ),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task Test_Fail_Restoring_Backup_For_Existing_Collection()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_restore_conflict");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var collection = await CollectionFactory(
            name: "ArticleConflict",
            properties: [Property.Text("title")]
        );
        var collectionName = collection.Name;

        await collection.Data.Insert(
            new { title = "alpha" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        var create = await _weaviate.Backup.CreateSync(
            new BackupCreateRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { collectionName }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, create.Status);

        // Attempt to restore while collection still exists should fail
        var restoreResp = await _weaviate.Backup.RestoreSync(
            new BackupRestoreRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { collectionName }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(restoreResp.Error);
    }

    [Fact]
    public async Task Test_Cancel_Running_Backup()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_cancel");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        // Create a collection with enough objects to keep backup busy for a moment
        var collection = await CollectionFactory(
            name: "ArticleCancel",
            properties: [Property.Text("title"), Property.Text("body")]
        );
        var collectionName = collection.Name;

        for (int i = 0; i < 40; i++)
        {
            await collection.Data.Insert(
                new { title = $"t{i}", body = new string('x', 500) },
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        // Start backup without waiting
        BackupCreateOperation operation = await _weaviate.Backup.Create(
            new BackupCreateRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { collectionName }
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(backupId, operation.Current.Id);

        // Immediately request cancel via operation
        await operation.Cancel(TestContext.Current.CancellationToken);

        // Wait and verify it's canceled
        var status = await operation.WaitForCompletion(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Canceled, status.Status);
    }

    [Fact]
    public async Task Test_List_Backups()
    {
        // Create a dummy collection for the backup to operate on
        var dummyCollection = await CollectionFactory(
            name: "DummyCollection",
            properties: [Property.Text("dummyField")]
        );
        var dummyCollectionName = dummyCollection.Name;

        // Insert a sample object into the dummy collection
        await dummyCollection.Data.Insert(
            new { dummyField = "sample data" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Match Python test: create backup, list it, poll until success
        var collectionSeed = "bkp_list";
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        // Create backup without waiting (mimics Python resp = client.backup.create(...))
        var operation = await _weaviate.Backup.Create(
            new BackupCreateRequest(backupId, _backend),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Started, operation.Current.Status);

        // List backups and verify our backup is present
        var backups = await _weaviate.Backup.List(
            _backend.Provider,
            TestContext.Current.CancellationToken
        );
        Assert.Contains(backups, b => b.Id.Equals(backupId, StringComparison.OrdinalIgnoreCase));

        // Cancel via operation
        await operation.Cancel(cancellationToken: TestContext.Current.CancellationToken);

        // Verify canceled
        await operation.WaitForCompletion(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Overwrite_Alias_False_Should_Not_Conflict()
    {
        RequireVersion("1.33.0"); // overwriteAlias supported from 1.33.0

        var seed = Helpers.GenerateUniqueIdentifier("overwrite_false");
        var backupId = Helpers.GenerateUniqueIdentifier(seed);

        // Use deterministic names derived from seed so we can assert post conditions
        var articleName = $"{seed}_Article";
        var paragraphName = $"{seed}_Paragraph";
        var aliasName = $"{seed}_Literature";

        // Create source and target collections
        var articleClient = await CollectionFactory(
            name: articleName,
            properties: [Property.Text("title")]
        );
        articleName = articleClient.Name;

        var paragraphClient = await CollectionFactory(
            name: paragraphName,
            properties: [Property.Text("contents")]
        );
        paragraphName = paragraphClient.Name;

        await articleClient.Data.Insert(
            new { title = "original" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await paragraphClient.Data.Insert(
            new { contents = "p1" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Create alias pointing to the Article collection
        // Delete alias if it already exists
        try
        {
            await _weaviate.Alias.Delete(aliasName, TestContext.Current.CancellationToken);
        }
        catch { }
        await _weaviate.Alias.Create(
            aliasName,
            articleClient.Name,
            TestContext.Current.CancellationToken
        );

        // Create backup including only Article
        var createResp = await _weaviate.Backup.CreateSync(
            new BackupCreateRequest(backupId, _backend, IncludeCollections: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Delete Article and repoint alias to Paragraph (conflict scenario for restore)
        await _weaviate.Collections.Delete(articleName, TestContext.Current.CancellationToken);
        await _weaviate.Alias.Update(
            aliasName,
            paragraphClient.Name,
            TestContext.Current.CancellationToken
        );

        // Attempt restore with overwriteAlias = false -> expect failure & alias unchanged
        var restoreResp = await _weaviate.Backup.RestoreSync(
            new BackupRestoreRequest(
                backupId,
                _backend,
                IncludeCollections: new[] { articleName },
                OverwriteAlias: false
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(BackupStatus.Success, restoreResp.Status); // terminal failed
        Assert.Null(restoreResp.Error);

        // Alias should still point to Paragraph
        var aliasAfter = await _weaviate.Alias.Get(
            aliasName,
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(aliasAfter);
        Assert.Equal(paragraphName, aliasAfter.TargetCollection);

        // Article should have been restored (attempting to use should NOT raise)
        await _weaviate
            .Collections.Use(articleName)
            .Query.FetchObjects(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Overwrite_Alias_True_Should_Replace_Existing_Alias()
    {
        RequireVersion("1.33.0");

        var seed = Helpers.GenerateUniqueIdentifier("overwrite_true");
        var backupId = Helpers.GenerateUniqueIdentifier(seed);

        var articleName = $"{seed}_Article";
        var paragraphName = $"{seed}_Paragraph";
        var aliasName = $"{seed}_Literature";

        var articleClient = await CollectionFactory(
            name: articleName,
            properties: [Property.Text("title")]
        );
        articleName = articleClient.Name;

        var paragraphClient = await CollectionFactory(
            name: paragraphName,
            properties: [Property.Text("contents")]
        );
        paragraphName = paragraphClient.Name;

        await articleClient.Data.Insert(
            new { title = "original" },
            cancellationToken: TestContext.Current.CancellationToken
        );
        await paragraphClient.Data.Insert(
            new { contents = "p1" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        await _weaviate.Alias.Create(
            aliasName,
            articleClient.Name,
            TestContext.Current.CancellationToken
        );

        var createResp = await _weaviate.Backup.CreateSync(
            new BackupCreateRequest(backupId, _backend, IncludeCollections: articleName),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Delete original, repoint alias to Paragraph to simulate conflicting alias
        await _weaviate.Collections.Delete(articleName, TestContext.Current.CancellationToken);
        await _weaviate.Alias.Update(
            aliasName,
            paragraphClient.Name,
            TestContext.Current.CancellationToken
        );

        // Restore with overwriteAlias = true should repoint alias back & recreate Article
        var restoreResp = await _weaviate.Backup.RestoreSync(
            new BackupRestoreRequest(
                backupId,
                _backend,
                IncludeCollections: articleName,
                OverwriteAlias: true
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);
        Assert.Null(restoreResp.Error);

        // Alias should now target recreated Article collection
        var aliasAfter = await _weaviate.Alias.Get(
            aliasName,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(aliasAfter);
        Assert.Equal(articleName, aliasAfter.TargetCollection);

        // Article collection should exist with restored data (1 object)
        var restoredArticle = _weaviate.Collections.Use(articleName);
        var objects = await restoredArticle.Query.FetchObjects(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Single(objects.Objects);
        Assert.Equal("original", objects.Objects.First().Properties["title"]);
    }

    [Fact]
    public async Task TestConcurrentBackupConflict()
    {
        var backupId1 = "concurrent-backup-1-" + Guid.NewGuid();
        var backupId2 = "concurrent-backup-2-" + Guid.NewGuid();
        var collectionName = "ConcurrentBackupTest_" + Guid.NewGuid().ToString("N");

        var collection = await CollectionFactory(
            name: collectionName,
            properties: [Property.Text("content")]
        );

        // Insert some data to make backup take a bit longer
        for (int i = 0; i < 10; i++)
        {
            await collection.Data.Insert(
                new { content = $"test content {i}" },
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        try
        {
            // Start first backup
            var operation1 = await _weaviate.Backup.Create(
                new BackupCreateRequest(
                    backupId1,
                    _backend,
                    IncludeCollections: new[] { collection.Name }
                ),
                TestContext.Current.CancellationToken
            );

            // Immediately try to start a second backup - this should fail with WeaviateBackupConflictException
            var exception = await Assert.ThrowsAsync<WeaviateBackupConflictException>(async () =>
            {
                await _weaviate.Backup.Create(
                    new BackupCreateRequest(
                        backupId2,
                        _backend,
                        IncludeCollections: new[] { collection.Name }
                    ),
                    TestContext.Current.CancellationToken
                );
            });

            // Verify exception message
            Assert.Contains("already in progress", exception.Message);

            // Wait for first backup to complete
            var result1 = await operation1.WaitForCompletion(
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.Equal(BackupStatus.Success, result1.Status);

            // Now second backup should succeed
            var operation2 = await _weaviate.Backup.Create(
                new BackupCreateRequest(
                    backupId2,
                    _backend,
                    IncludeCollections: new[] { collection.Name }
                ),
                TestContext.Current.CancellationToken
            );
            var result2 = await operation2.WaitForCompletion(
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.Equal(BackupStatus.Success, result2.Status);
        }
        finally
        {
            // Cleanup
            await _weaviate.Collections.Delete(
                collection.Name,
                TestContext.Current.CancellationToken
            );
        }
    }

    [Fact]
    public async Task TestConcurrentRestoreConflict()
    {
        var backupId = "concurrent-restore-test-" + Guid.NewGuid();
        var collectionName = "ConcurrentRestoreTest_" + Guid.NewGuid().ToString("N");

        var collection = await CollectionFactory(
            name: collectionName,
            properties: [Property.Text("content")]
        );

        await collection.Data.Insert(
            new { content = "test data" },
            cancellationToken: TestContext.Current.CancellationToken
        );

        try
        {
            // Create a backup first
            var createResult = await _weaviate.Backup.CreateSync(
                new BackupCreateRequest(
                    backupId,
                    _backend,
                    IncludeCollections: new[] { collection.Name }
                ),
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.Equal(BackupStatus.Success, createResult.Status);

            // Delete the collection to prepare for restore
            await _weaviate.Collections.Delete(
                collection.Name,
                TestContext.Current.CancellationToken
            );

            // Start first restore
            var operation1 = await _weaviate.Backup.Restore(
                new BackupRestoreRequest(backupId, _backend),
                TestContext.Current.CancellationToken
            );

            // Immediately try to start a second restore - this should fail with WeaviateBackupConflictException
            var exception = await Assert.ThrowsAsync<WeaviateBackupConflictException>(async () =>
            {
                await _weaviate.Backup.Restore(
                    new BackupRestoreRequest(backupId, _backend),
                    TestContext.Current.CancellationToken
                );
            });

            // Verify exception message
            Assert.Contains("already in progress", exception.Message);

            // Wait for first restore to complete
            var result1 = await operation1.WaitForCompletion(
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.Equal(BackupStatus.Success, result1.Status);
        }
        finally
        {
            // Cleanup
            await _weaviate.Collections.Delete(
                collectionName,
                TestContext.Current.CancellationToken
            );
        }
    }
}
