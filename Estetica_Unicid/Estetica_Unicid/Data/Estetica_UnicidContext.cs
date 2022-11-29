using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Estetica_Unicid.Models;

namespace Estetica_Unicid.Data
{
    public class Estetica_UnicidContext : DbContext
    {
        public Estetica_UnicidContext (DbContextOptions<Estetica_UnicidContext> options)
            : base(options)
        {
        }

        public DbSet<Estetica_Unicid.Models.ClienteModel> ClienteModel { get; set; } = default!;

        public DbSet<Estetica_Unicid.Models.ServicoModel> ServicoModel { get; set; }
    }
}
