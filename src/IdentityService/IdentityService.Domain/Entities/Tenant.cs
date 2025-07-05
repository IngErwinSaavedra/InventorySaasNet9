using System;
using System.Collections.Generic;

namespace IdentityService.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string InitialPlan { get; set; }
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        protected Tenant() { }

        public Tenant(string name, string initialPlan)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tenant name cannot be null or empty.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(initialPlan))
            {
                throw new ArgumentException("Initial plan cannot be null or empty.", nameof(initialPlan));
            }

            this.Id = Guid.NewGuid();
            this.Name = name;
            this.InitialPlan = initialPlan;
            
            Users = new List<ApplicationUser>();
        }

        public void UpdatePlan(string newPlan)
        {
            if (string.IsNullOrWhiteSpace(newPlan))
            {
                throw new ArgumentException("New plan cannot be null or empty.", nameof(newPlan));
            }
            this.InitialPlan = newPlan;
        }

        
    }
}