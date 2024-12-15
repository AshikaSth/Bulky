using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models.Models 
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
		[DisplayName("Category Name")]
		public string Name { get; set; }
        [DisplayName("Display order")]
        [Range(1,100)]
        public int DisplayOrder { get; set; }
    }
}
