using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Cart")]
    public class CartItem
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }
        [HiddenInput(DisplayValue = false)]
        public string CartId { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int ProductId { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Некорректное значение")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }
        public Product SelectProduct { get; set; }
    }
}