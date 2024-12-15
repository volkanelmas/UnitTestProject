using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProject.Web.Controllers;
using UnitTestProject.Web.Models;
using UnitTestProject.Web.Repository;

namespace UnitTestProject.Test
{
    public class ProductsControllerTest
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly ProductsController _controller;
        private List<Product> products;

        public ProductsControllerTest()
        {
            _mockRepo = new Mock<IProductRepository>();
            _controller = new ProductsController(_mockRepo.Object);
            products = new List<Product> {
                new Product { Id = 1, Name = "Kalem", Price = 100, Color = "Kırmızı" },
                new Product { Id = 2, Name = "Defter", Price = 200, Color = "Mavi" }
            };
        }

        [Fact]
        public async void Index_ActionExecute_ReturnView()
        {
            var result = await _controller.Index();
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async void Index_ActionExecute_ReturnProductList()
        {
            //sahte bir obje üretiyor. index bir GetAll() model dönüyor.
            //yukarıda tanımlanan products i GetAll() için sahte obje için kullanıyoruz
            //Böylelikle; Moq sayesinde "GetAll() model döndüren Index methodu" sahte bir veriyle test etmeyi sağladık.
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);
            //methodu çalıştır
            var result = await _controller.Index();

            //Kontrol1:view result dönüyormu?
            var viewResult = Assert.IsType<ViewResult>(result);
            //kontrol2:view resultun modeli productlist mi?
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            //kontrol3:product listin sayısı statik oluşturduğumuz listeyle aynı mı
            Assert.Equal<int>(2, productList.Count());
        }

        //Details sayfasının id null gelme testi
        [Fact]
        public async void Details_IdIsNull_RedirectToIndexAction()
        {
            var result = await _controller.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        //Details sayfasının id si bulunamadıysa null dönsün
        [Fact]
        public async void Details_IdInValid_ReturnNotFound()
        {
            //sahte bir obje üret.
            _mockRepo.Setup(repo => repo.GetById(-1)).ReturnsAsync(new Product());
            //methodu çalıştır
            var result = await _controller.Details(-1);

            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }
        //Details sayfasının id varsa product dönmesi kontrolü
        [Theory]
        [InlineData(1)]
        public async void Details_ValidId_ReturnProduct(int productId)
        {
            var product = products.Where(x => x.Id == productId).First();
            //sahte bir obje üret.
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            //methodu çalıştır
            var result = await _controller.Details(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
        }

        [Fact]
        public async void Create_ActionExecute_ReturnView()
        {
            var result = await _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        //create IsValid durumu testi. Örnek bir hata verdik. test ettik
        [Fact]
        public async void Create_IsValidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir");
            var result = await _controller.Create(products.First());
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }
        //create post sayfası testi
        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Create(products.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        //create post veri kaydetme testi
        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;
            //Callback te gelen nesneyi newProduct a aktarıyoruz.
            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);

            //methodu çalıştırıyoruz.
            var result = await _controller.Create(products.First());

            //method doğrulama yapılıyor.
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);

            //create edilen products.id ile newProduct.Id eşit mi
            Assert.Equal(products.First().Id, newProduct.Id);
        }
        //create post methodun çalışmaması gereken durum test
        [Fact]
        public async void CreatePOST_InValidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "");
            var result = await _controller.Create(products.First());
            //_mockRepo setup ta create çalıştırmadık. Times.Never doğru olacak.
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        //edit methodu çalışma kontrolü
        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Edit(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        //edit edilecek id yok testi
        [Theory]
        [InlineData(3)]
        public async void Edit_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }
        //edit edilecek doğru veri testi
        [Theory]
        [InlineData(2)]
        public async void Edit_ActionExecutes_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
        }
        //edit edilecek id ile model arasında uyuşmazlık varsa testi
        [Theory]
        [InlineData(1)]
        public async void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(2, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }
        //edit IsValid durumu testi. Örnek bir hata verdik. test ettik
        [Theory]
        [InlineData(1)]
        public async void EditPOST_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "Name boş olmaz");
            var result = await _controller.Edit(productId, products.First(x => x.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }
        //edit post method testi
        [Theory]
        [InlineData(1)]
        public async void EditPOST_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = await _controller.Edit(productId, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        //edit update etme testi
        [Theory]
        [InlineData(1)]
        public async void EditPOST_ValidModelState_UpdateMethodExecute(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Update(product));
            await _controller.Edit(productId, product);
            //mock repo da update oldu mu?
            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }

        //Delete sayfasının id null gelme testi
        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }
        //Delete id ye sahip ürün var mı?
        [Theory]
        [InlineData(0)]
        public async void Delete_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(new Product());
            var result = await _controller.Delete(productId);
            Assert.IsType<NotFoundResult>(result);
        }
        //Delete var olan bir id ve product dönmesi testi
        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecutes_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }
        //Delete POST methodunun çalışması testi
        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_ReturnRedirectToIndexAction(int productId)
        {
            var result = await _controller.DeleteConfirmed(productId);
            Assert.IsType<RedirectToActionResult>(result);
        }
        //Delete POST veri silme testi
        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Delete(product.Id));
            await _controller.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.Delete(product.Id), Times.Once);
        }
    }
}
