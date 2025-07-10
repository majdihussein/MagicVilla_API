using System.Linq.Expressions;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            //_db.VillaNumbers.Include(u =>u.Villa).ToList();
            this.dbset = db.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await dbset.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includePropeties = null)
        {
            IQueryable<T> query = dbset;
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includePropeties != null)
            {
                foreach (var includeProp in includePropeties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includePropeties = null,
            int pageSize = 0, int pageNumber = 1)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (pageSize >0 )
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }


            if (includePropeties != null)
            {
                foreach (var includeProp in includePropeties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            dbset.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
