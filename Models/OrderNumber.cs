using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class OrderNumber
    {

        [Required(ErrorMessage = "Пожалуйста, введите начальную дату")]
        [Display(Name = "Начало диапазона")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите конечную дату")]
        [Display(Name = "Конец диапазона")]
        public DateTime End { get; set; }
        [Display(Name = "Число заказов")]
        public int Number { get; set; }
        [Display(Name = "Средняя стоимость одного заказа")]
        public int Average { get; set; }
        [Display(Name = "Общая сумма заказов")]
        public int TotalSum { get; set; }

    }
}
