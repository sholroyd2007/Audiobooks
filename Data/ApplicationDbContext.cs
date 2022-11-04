using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Audiobooks.Models;

namespace Audiobooks.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Audiobook> Audiobook { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Recommendation> Recommendation { get; set; }
        public DbSet<Sample> Sample { get; set; }
        public DbSet<Blurb> Blurb { get; set; }
    }
}
