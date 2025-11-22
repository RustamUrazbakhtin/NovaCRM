
namespace NovaCRM.Data.Model
{
    public partial class AspNetUserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
        public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();

    }
}
