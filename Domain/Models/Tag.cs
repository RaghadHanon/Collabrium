using Domain.Common;

namespace Domain.Models
{
    public sealed class Tag : Entity
    {
        public string TenantId { get; private set; } = null!;
        public string Name { get; private set; } = null!;

        private Tag() { } // EF

        public Tag(string tenantId, string name)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
            Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
            Touch();
        }

        public Tag(string id, string tenantId, string name) : base(id)
        {
            TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
            Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
        }

        public static Tag Rehydrate(
            string id,
            DateTime createdAt,
            DateTime? updatedAt,
            string tenantId,
            string name)
        {
            var t = new Tag(id, tenantId, name);
            t.SetTimestamps(createdAt, updatedAt);
            return t;
        }

        public void Rename(string name)
        {
            Name = Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim();
            Touch();
        }
    }
}
