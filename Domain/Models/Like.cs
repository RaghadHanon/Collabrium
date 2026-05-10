using Domain.Common;

namespace Domain.Models
{
    public abstract class Like : Entity
    {
        public string CreatedByUserId { get; private set; } = null!;

        protected Like() { }

        protected Like(string createdByUserId)
        {
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

            Touch();
        }

        protected Like(string id, string createdByUserId) : base(id)
        {
            CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
        }
    }
}
