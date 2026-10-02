using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WsSofterpRD.Model;

namespace WsSofterpRD.Data
{
    public class WsSofterpRDContext : DbContext
    {
        public WsSofterpRDContext (DbContextOptions<WsSofterpRDContext> options)
            : base(options)
        {
        }

 
        public DbSet<WsSofterpRD.Model.ClientesWS> ClientesWS { get; set; } = default!;
      
    }
}
