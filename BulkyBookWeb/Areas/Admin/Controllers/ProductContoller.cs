﻿using BulkyBook.DataAccess.Data;
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
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;

		}
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            
            return View(objProductList);
        }

        public IActionResult Upsert(int? id) //Update+Insert
        {
	
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				}),
				Product = new Product()
            };
            if(id==null || id == 0)
            {
                //Create/Insert
                return View(productVM);
            }
            else
            {
                //UPdate
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
				return View(productVM);
			}
			
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
           
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
            else
            {
				productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return View(productVM);
			}   
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