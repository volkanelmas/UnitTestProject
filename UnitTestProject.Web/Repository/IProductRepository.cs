using UnitTestProject.Web.Models;

namespace UnitTestProject.Web.Repository
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAll();
        Task<Product> GetById(int? id);
        Task Create(Product product);
        Task Update(Product product);
        Task Delete(int? id);
    }
}
