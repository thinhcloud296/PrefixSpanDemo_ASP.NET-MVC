using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PrefixSpanDemo.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("MyConnectionString") { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sequence> Sequences { get; set; }
    }
}