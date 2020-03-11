using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using System.IO;

namespace OrchardCore.ContentManagement
{
    public class CustomFileContentDefinitionStore : IContentDefinitionStore
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;

        public CustomFileContentDefinitionStore(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }

        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// </summary>
        public async Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            var scopedCache = ShellScope.Services.GetRequiredService<FileContentDefinitionScopedCache>();

            if (scopedCache.ContentDefinitionRecord != null)
            {
                return scopedCache.ContentDefinitionRecord;
            }

            return scopedCache.ContentDefinitionRecord = await GetContentDefinitionAsync();
        }

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// </summary>
        public Task<ContentDefinitionRecord> GetContentDefinitionAsync()
        {
            ContentDefinitionRecord result;

            if (!File.Exists(Filename))
            {
                result = new ContentDefinitionRecord();
            }
            else
            {
#pragma warning disable S2551 // Shared resources should not be used for locking
                lock (this)
#pragma warning restore S2551 // Shared resources should not be used for locking
                {
                    using (var file = File.OpenText(Filename))
                    {
                        var serializer = new JsonSerializer();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        result = (ContentDefinitionRecord)serializer.Deserialize(file, typeof(ContentDefinitionRecord));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    }
                }
            }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Task.FromResult(result);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        {
#pragma warning disable S2551 // Shared resources should not be used for locking
            lock (this)
#pragma warning restore S2551 // Shared resources should not be used for locking
            {
                using (var file = File.CreateText(Filename))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, contentDefinitionRecord);
                }
            }

            return Task.CompletedTask;
        }

        private string Filename => PathExtensions.Combine(
            _shellOptions.Value.ShellsApplicationDataPath,
            _shellOptions.Value.ShellsContainerName,
            _shellSettings.Name, "ContentDefinition.json");
    }

    internal class FileContentDefinitionScopedCache
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ContentDefinitionRecord ContentDefinitionRecord { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    }
}
