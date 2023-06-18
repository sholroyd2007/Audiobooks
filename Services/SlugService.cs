using Microsoft.EntityFrameworkCore;
using Audiobooks.Data;
using Audiobooks.Models;
using System.Linq;
using System.Threading.Tasks;
using Audiobooks.Helpers;

namespace Audiobooks.Services
{
    public interface ISlugService
    {
        Task GenerateSlugs();
        Task ClearSlugs();
        Task DeleteSlug(long slugId);
        Task<int> GetEntityIdBySlug<TEntity>(string slug) where TEntity : Entity;
        Task<Slug> GetOrCreateSlugForEntity<TEntity>(TEntity entity) where TEntity : Entity;

        Task<Slug> GetSlugForEntity<TEntity>(TEntity entity) where TEntity : Entity;
    }

    public class SlugService : ISlugService
    {
        public SlugService(ApplicationDbContext databaseContext)
        {
            DatabaseContext = databaseContext;
        }

        public ApplicationDbContext DatabaseContext { get; }

        public async Task<Slug> GetOrCreateSlugForEntity<TEntity>(TEntity entity) where TEntity : Entity
        {
            var generatedSlug = SlugHelper.Slugify(entity);
            var slugObj = await DatabaseContext.Slugs.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == generatedSlug && e.EntityType == typeof(TEntity).Name && e.EntityId == entity.Id);

            if (slugObj == null)
            {
                slugObj = new Slug
                {
                    Name = generatedSlug,
                    EntityId = entity.Id,
                    EntityType = entity.GetType().Name
                };

                DatabaseContext.Add(slugObj);
                await DatabaseContext.SaveChangesAsync();
            }

            return slugObj;
        }

        public async Task DeleteSlug(long slugId)
        {
            var entity = await DatabaseContext.Slugs.FindAsync(slugId);
            DatabaseContext.Remove(entity);
            await DatabaseContext.SaveChangesAsync();
        }

        public async Task<int> GetEntityIdBySlug<TEntity>(string slug) where TEntity : Entity
        {

            var entity = await DatabaseContext.Slugs.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EntityType == typeof(TEntity).Name && e.Name == slug);

            return entity.EntityId;
        }

        public async Task<Slug> GetSlugForEntity<TEntity>(TEntity entity) where TEntity : Entity
        {
            var slugs = await DatabaseContext.Slugs.AsNoTracking()
                .Where(e => e.EntityType == typeof(TEntity).Name && e.EntityId == entity.Id)
                .ToListAsync();

            return slugs.OrderByDescending(e => e.Created).FirstOrDefault();
        }

        public async Task ClearSlugs()
        {
            DatabaseContext.RemoveRange(DatabaseContext.Slugs);
            await DatabaseContext.SaveChangesAsync();
        }

        public async Task GenerateSlugs()
        {
            var audiobooks = await DatabaseContext.Audiobook.AsNoTracking().ToListAsync();
            foreach (var item in audiobooks)
            {
                await GetOrCreateSlugForEntity(item);
            }

            var authors = await DatabaseContext.Authors.AsNoTracking().ToListAsync();
            foreach (var item in authors)
            {
                await GetOrCreateSlugForEntity(item);
            }

            var narrators = await DatabaseContext.Narrators.AsNoTracking().ToListAsync();
            foreach (var item in narrators)
            {
                await GetOrCreateSlugForEntity(item);
            }

            var categories = await DatabaseContext.Category.AsNoTracking().ToListAsync();
            foreach (var item in categories)
            {
                await GetOrCreateSlugForEntity(item);
            }

            var series = await DatabaseContext.Series.AsNoTracking().ToListAsync();
            foreach (var item in series)
            {
                await GetOrCreateSlugForEntity(item);
            }
        }
    }
}
