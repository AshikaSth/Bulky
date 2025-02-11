﻿using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Web.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class

	{
		private readonly ApplicationDbContext _db;
		internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
			_db = db; 
			this.dbSet = _db.Set<T>();
			_db.Products.Include(u=> u.Category).Include(u=> u.CategoryId);
        }
        public void Add(T entity)
		{
			_db.Add(entity);

		}

		public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
		{
			IQueryable<T> query;

            if (tracked)
			{
                query = dbSet;
			
			}
			else
			{
                query = dbSet.AsNoTracking();
               
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }


		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveAll(IEnumerable<T> entities)
		{
			dbSet.RemoveRange(entities);
		}

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties)
        {
                IQueryable<T> query = dbSet;
                if(filter!= null) {
                    query = query.Where(filter);
                }
               
                if (!string.IsNullOrEmpty(includeProperties))
                {
                    foreach (var includeProp in includeProperties
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }
                return query.ToList();
            }
        }
}
