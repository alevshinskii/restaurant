using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Models
{
    [Table("Products")]
    public class Product
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста введите название")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 150 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; }
        
        [Required]
        [Range(0, 5000, ErrorMessage = "Недопустимая цена")]
        [Display(Name = "Цена")]
        public int Price { get; set; }

        [Required]
        [Display(Name = "Описание")]
        public string Description { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        [Display(Name = "Категория")]
        public Category Category { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Изображение")]
        public string? ImageUrl { get; set; }

        [Required]
        [Range(0, 1000000, ErrorMessage = "Недопустимое количество")]
        [Display(Name = "Доступно к заказу")]
        public int Available { get; set; }

    }


}