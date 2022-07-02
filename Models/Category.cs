using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Categories")]
    public class Category
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; } 

        [Required(ErrorMessage = "Пожалуйста введите название категории")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 50 символов")]
        [Display(Name = "Категория")]
        public string Name { get; set; } 
        public IEnumerable<Product> Products { get; set; }
    }
}