using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

		}
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
			Console.WriteLine($"Count: {objCompanyList.Count}");
			return View(objCompanyList);
        }

        public IActionResult Upsert(int? id) //Update+Insert
        {

            if(id==null || id == 0)
            {
                //Create/Insert
                return View(new Company());
            }
            else
            {
                //UPdate
                Company companyobj = _unitOfWork.Company.Get(u => u.Id == id);
				return View(companyobj);
			}
			
        }
        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
           
            if (ModelState.IsValid)
            {
      

                if (CompanyObj.Id == 0)
                {
					_unitOfWork.Company.Add(CompanyObj);

				}
                else
                {
					_unitOfWork.Company.Update(CompanyObj);
				}
				_unitOfWork.Save();
                TempData["success"] = "Company created successfully!";
                return RedirectToAction("Index");

            }
            else
            {
				
                return View(CompanyObj);
			}   
        }





        //#region APICALLS

        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
        //    return Json(new { data = objCompanyList });
        //}


        //[HttpDelete]
        //public IActionResult Delete(int? id)
        //{
        //    var CompanyTobeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
        //    if(CompanyTobeDeleted == null)
        //    {
        //        return Json(new { success = false, message = "Error while deleting" });
        //    }
          
        //    _unitOfWork.Company.Remove(CompanyTobeDeleted);
        //    _unitOfWork.Save();
        //    return Json(new { success = true, message = "Delete Successful." });

        //}

        //#endregion

    }
}
