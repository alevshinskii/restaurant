using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class Bestseller
    {
        public Product product { get; set; }
        [Display(Name = "Количество")]
        public int Quanity { get; set; }
    }
}
