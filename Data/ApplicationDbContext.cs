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
        public DbSet<Blurb> Blurb { get; set; }
        public DbSet<Slug> Slugs { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Narrator> Narrators { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookNarrator> BookNarrators { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<SeriesBook> SeriesBooks { get; set; }
        public DbSet<ErrorReport> ErrorReports { get; set; }

        
    }
}
