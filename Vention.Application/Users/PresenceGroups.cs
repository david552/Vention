namespace Vention.Application.Users
{

    public static class PresenceGroups
    {
        public static string ForOrganization(Guid organizationId) => $"org-{organizationId}";

        public static bool TryParseOrganizationId(string groupName, out Guid organizationId)
        {
            const string prefix = "org-";

            if (groupName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(groupName[prefix.Length..], out organizationId))
            {
                return true;
            }

            organizationId = default;
            return false;
        }
    }
}