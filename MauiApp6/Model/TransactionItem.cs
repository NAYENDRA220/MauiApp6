using MauiApp6.Base;
using System.ComponentModel.DataAnnotations;
namespace MauiApp6.Model
{
    public class TransactionItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Please provide the task name.")]
        public string? TransactionName { get; set; }

        [Required(ErrorMessage = "Please specify transaction type.")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Please provide amount.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select at least one tag.")]
        public List<string> Tags { get; set; } = new List<string>();

        public bool IsDone { get; set; }

        [Required(ErrorMessage = "Please provide a due date.")]
        public DateTime DueDate { get; set; } = DateTime.Today;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Notes { get; set; }

    }
}
