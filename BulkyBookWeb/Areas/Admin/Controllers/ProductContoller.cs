using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IFeatureFlagRepository _featureFlagRepository;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IFeatureFlagRepository featureFlagRepository)
        {
            _unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
			_featureFlagRepository = featureFlagRepository;

		}
        public async Task<IActionResult> IndexAsync()
        {
			bool includeCategory = await _featureFlagRepository.GetFeatureFlagStatusAsync("IncludeCategory");
			HttpContext.Items["IncludeCategory"] = includeCategory;
			ViewBag.IncludeCategory = includeCategory;
			List<Product> objProductList;
			if (includeCategory)
			{

				objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			}
			else
			{
				objProductList = _unitOfWork.Product.GetAll().ToList();
			}
			foreach (var product in objProductList)
			{
				if (product.Category == null)
				{
					// Assign a default category (you can fetch it from the database or create a new one)
					product.Category = new Category { Name = "Uncategorized" }; // Or fetch a default from the DB
				}
			}

			return View(objProductList);
        }
		
		public async Task<IActionResult> UpsertAsync(int? id) //Update+Insert
        {
			bool isIncludeCategoryEnabled = await _featureFlagRepository.GetFeatureFlagStatusAsync("IncludeCategory");
			ViewBag.IncludeCategory = isIncludeCategoryEnabled;

			// Pass the flag status to the view using ViewBag
			ViewBag.IncludeCategory = isIncludeCategoryEnabled;

			ProductVM productVM = new()
            {
				
				CategoryList = isIncludeCategoryEnabled
				  ? _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				})
			 : new List<SelectListItem>(),
			 Product = new Product(),
			};
            if(id==null || id == 0)
            {
				//Create/Insert
				if (!isIncludeCategoryEnabled)
				{
					productVM.Product.CategoryId = 0;  // Default to "Uncategorized"
				}
				return View(productVM);
				
            }
            else
            {
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
				//UPdate
				if (!isIncludeCategoryEnabled && productVM.Product.CategoryId == null)
				{

					productVM.Product.CategoryId = 0;  // Default to "Uncategorized"
				}
				
				return View(productVM);
			}
			
        }
        [HttpPost]
	
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {

			// Retrieve the IncludeCategory flag from HttpContext
			bool isIncludeCategoryEnabled = HttpContext.Items["IncludeCategory"] as bool? ?? false;


            // Initialize CategoryList if IncludeCategory is enabled
            if (isIncludeCategoryEnabled)
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }).ToList();
            }
            // Handle "Uncategorized" logic only when IncludeCategory is disabled
            if (!isIncludeCategoryEnabled)
            {
                var uncategorizedCategory = _unitOfWork.Category.Get(u => u.Name == "Uncategorized");
                if (uncategorizedCategory == null)
                {
                    // Create "Uncategorized" category if it doesn't exist
                    uncategorizedCategory = new Category { Name = "Uncategorized" };
                    _unitOfWork.Category.Add(uncategorizedCategory);
                    _unitOfWork.Save();
                }

                // Check if the product already exists and has a category assigned
                if (productVM.Product.Id != 0)
                {
                    // Fetch the existing product from the database
                    var existingProduct = _unitOfWork.Product.Get(u => u.Id == productVM.Product.Id);
                    if (existingProduct != null && existingProduct.CategoryId != 0 && existingProduct.CategoryId != null)
                    {
                        // Retain the existing category
                        productVM.Product.CategoryId = existingProduct.CategoryId;
                    }
                    else
                    {
                        // Assign "Uncategorized" only if no category was previously assigned
                        productVM.Product.CategoryId = uncategorizedCategory.Id;
                    }
                }
                else
                {
                    // For new products, assign "Uncategorized" if no category is provided
                    if (productVM.Product.CategoryId == 0 || productVM.Product.CategoryId == null)
                    {
                        productVM.Product.CategoryId = uncategorizedCategory.Id;
                    }
                }
            }
            // Validate CategoryId if IncludeCategory is enabled
            if (isIncludeCategoryEnabled && productVM.Product.CategoryId == 0)
            {
                ModelState.AddModelError("Product.CategoryId", "Category is required.");
            }
           




            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName= Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"image/product");
                    if(!string.IsNullOrEmpty(productVM.Product.Imageurl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.Imageurl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        { 
                            System.IO.File.Delete(oldImagePath);

						}
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    { 
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.Imageurl = @"\image\product\"+fileName;
                }

                if (productVM.Product.Id == 0)
                {
					_unitOfWork.Product.Add(productVM.Product);

				}
                else
                {
					_unitOfWork.Product.Update(productVM.Product);
				}
				_unitOfWork.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");

            }
				// Initialize CategoryList only if IncludeCategory is enabled
			//if (isIncludeCategoryEnabled)
			//{
			//	productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
			//		{
			//			Text = u.Name,
			//			Value = u.Id.ToString(),
			//		}).ToList();
			//}
				return View(productVM);
			  
        }


		// GET: Delete Product
		public IActionResult Delete(int id)
		{
			// Fetch the product to delete
			var product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category");

			if (product == null)
			{
				return NotFound();
			}

			return View(product); // Return the product details to the delete confirmation view
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeletePost(int id)
		{
			// Fetch the product to delete
			var product = _unitOfWork.Product.Get(u => u.Id == id);

			if (product == null)
			{
				return NotFound();
			}

			// Delete the image from the server if it exists
			if (!string.IsNullOrEmpty(product.Imageurl))
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				var imagePath = Path.Combine(wwwRootPath, product.Imageurl.TrimStart('\\'));

				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}

			// Delete the product from the database
			_unitOfWork.Product.Remove(product);
			_unitOfWork.Save();

			TempData["success"] = "Product deleted successfully!";
			return RedirectToAction("Index");
		}




		//#region APICALLS

		//[HttpGet]
		//public IActionResult GetAll()
		//{
		//    List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
		//    return Json(new { data = objProductList });
		//}


		//[HttpDelete]
		//public IActionResult Delete(int? id)
		//{
		//    var productTobeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
		//    if(productTobeDeleted == null)
		//    {
		//        return Json(new { success = false, message = "Error while deleting" });
		//    }
		//    //delete the old image
		//    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,productTobeDeleted.Imageurl.TrimStart('\\'));

		//    if (System.IO.File.Exists(oldImagePath))
		//    {
		//        System.IO.File.Delete(oldImagePath);
		//    }
		//    _unitOfWork.Product.Remove(productTobeDeleted);
		//    _unitOfWork.Save();
		//    return Json(new { success = true, message = "Delete Successful." });

		//}

		//#endregion

	}
}
