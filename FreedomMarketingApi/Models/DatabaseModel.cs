using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FreedomMarketingApi.Models.DataModel;

namespace FreedomMarketingApi.Models
{
    public class DatabaseModel
    {
        public class MySqlContext : DbContext
        {
            //public MySqlContext(DbContextOptions<MySqlContext> options)
            //: base(options)
            //{
            //}
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseMySql("server=167.172.248.79;port=3306;database=thefreedommarketing_dev;uid=ennio;password=Bq$%Em=Y8L&5TDNH");
            }
            public DbSet<Roles> Roles { get; set; }
            public DbSet<Usuarios> Usuarios { get; set; }
        }
    }
}
