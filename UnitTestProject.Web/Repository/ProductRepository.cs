using Microsoft.EntityFrameworkCore;
using UnitTestProject.Web.Models;

namespace UnitTestProject.Web.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly MyDbContext _dbContext;

        public ProductRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(Product product)
        {
            _dbContext.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> GetAll()
        {
            List<Product> products = await _dbContext.Products.ToListAsync();
            if (products == null)
            {
                return new List<Product>();
            }
            return products;
        }

        public async Task<Product> GetById(int id)
        {
            var product = await _dbContext.Products
              .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return new Product();
            }
            return product;
        }

        public async Task Update(Product product)
        {
            _dbContext.Update(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}
