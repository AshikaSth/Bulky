using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.Models;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeatureFlagRepository _featureFlagRepository;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IFeatureFlagRepository featureFlagRepository)
        {
            _logger = logger;
            _unitOfWork= unitOfWork;
            _featureFlagRepository = featureFlagRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }

		public  async Task<IActionResult> DetailsAsync(int productId)
		{
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };

            //var product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category");
            // Check if the "AddToCart" feature flag is enabled
            bool isAddToCartEnabled = await _featureFlagRepository.GetFeatureFlagStatusAsync("AddToCart");

			// Fetch the "IncludeCategory" feature flag status
			bool isIncludeCategoryEnabled = await _featureFlagRepository.GetFeatureFlagStatusAsync("IncludeCategory");
			
            // Pass the feature flag status to the view
			ViewBag.IsAddToCartEnabled = isAddToCartEnabled;
			ViewBag.IsIncludeCategoryEnabled = isIncludeCategoryEnabled;
			return View(cart);
		}
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId
            && u.ProductId == shoppingCart.ProductId );

            if(cartFromDb != null)
            {
                //shopping cart already exists
                cartFromDb.Count += shoppingCart.Count;
               _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                //add a new shopping cart
                _unitOfWork.ShoppingCart.Add(shoppingCart);

            }

            _unitOfWork.Save();
            TempData["success"] = "Cart updated successfully!";

            return RedirectToAction(nameof(Index));
        }

		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
		[AllowAnonymous]
		public IActionResult FeatureDisabled()
        {
            return View();  // Show a view that indicates the feature is disabled
        }
    }
}
