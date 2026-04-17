using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaCRM.Domain.Staff.Model
{
    public class StaffItem
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int Status { get; set; }
        public Guid LocationId { get; set; }
        public string? Notes { get; set; }
        public bool CRMAccess { get; set; }
        public int CompensationTypeId { get; set; }
        public int Salary { get; set; }
        public Guid OrgId { get; set; }
    }
}
