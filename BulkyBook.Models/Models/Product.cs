using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BulkyBook.Models.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        
        public string Title { get; set; }
        [Required]

       
        public string Author { get; set; }
        public string Description { get; set; }
        [Required]
        public string ISBN { get; set; }
        

        [Required]
        [Display(Name ="List price")]
        [Range(1,2000)]
        public decimal ListPrice { get; set; }

        [Required]
        [Display(Name = "Price for 1-50")]
        [Range(1, 2000)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Price for 50+")]
        [Range(1, 2000)]
        public decimal Price50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")]
        [Range(1, 2000)]
        public decimal Price100 { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId {  get; set; }
        [ForeignKey("CategoryId")]

		[ValidateNever]
		public Category? Category { get; set; }
		[ValidateNever]
		public string Imageurl { get; set; }
       

    }
}
