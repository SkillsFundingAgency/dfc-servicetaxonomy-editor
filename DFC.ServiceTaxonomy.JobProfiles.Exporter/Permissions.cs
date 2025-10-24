using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageJobProfilesExporter = new Permission(nameof(ManageJobProfilesExporter), "Export Job Profiles");

        public Task<IEnumerable<Permission>> GetPermissionsAsync() => Task.FromResult(new[] { ManageJobProfilesExporter }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageJobProfilesExporter }
                }
            };
    }
}
