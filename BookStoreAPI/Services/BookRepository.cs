using BookStoreAPI.Contracts;
using BookStoreAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Services
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _db;

        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(Book entity)
        {
            await _db.Books.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Book entity)
        {
            _db.Books.Remove(entity);
            return await Save();
        }

        public async Task<IList<Book>> FindAll()
        {
            var books =  await _db.Books.ToListAsync();
            return books;
        }

        public async Task<Book> FindById(int id)
        {
            var obj = await _db.Books.FindAsync(id);
            return obj;
        }

        public async Task<bool> isExsists(int id)
        {
            var isExsists = await _db.Books.AnyAsync(x => x.Id == id);
            return isExsists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Book entity)
        {
            _db.Books.Update(entity);
            return await Save();
        }
    }
}
