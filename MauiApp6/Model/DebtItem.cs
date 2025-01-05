using MauiApp6.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MauiApp6.Model
{
    public class DebtItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Please provide the debt name.")]
        public string? DebtName { get; set; }

        [Required(ErrorMessage = "Please specify debt type.")]
        public DebtType DebtType { get; set; }

        [Required(ErrorMessage = "Please provide amount.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select at least one category.")]
        public List<string> Categories { get; set; } = new List<string>();

        public bool IsPaid { get; set; }

        [Required(ErrorMessage = "Please provide a due date.")]
        public DateTime DueDate { get; set; } = DateTime.Today;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}