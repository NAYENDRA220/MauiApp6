using MauiApp6.Base;
using MauiApp6.Model;
using System.Text.Json;

namespace MauiApp6.Services
{
    public static class DebtService
    {
        private static void SaveAll(Guid Id, List<DebtItem> debts)
        {
            string appDataDirectoryPath = Utils.GetAppDirectoryPath();
            string debtsFilePath = Utils.GetDebtsFilePath(Id);
            if (!Directory.Exists(appDataDirectoryPath))
            {
                Directory.CreateDirectory(appDataDirectoryPath);
            }
            var json = JsonSerializer.Serialize(debts);
            File.WriteAllText(debtsFilePath, json);
        }

        public static List<DebtItem> GetAll(Guid userId)
        {
            string debtsFilePath = Utils.GetDebtsFilePath(userId);
            if (!File.Exists(debtsFilePath))
            {
                return new List<DebtItem>();
            }
            var json = File.ReadAllText(debtsFilePath);
            return JsonSerializer.Deserialize<List<DebtItem>>(json);
        }

        private static decimal CalculateOverallBalance(Guid userId)
        {
            // Get all transactions
            var transactions = TransactionService.GetAll(userId);
            decimal transactionBalance = transactions.Sum(t => t.TransactionType switch
            {
                TransactionType.Income => t.Amount,
                TransactionType.Expense => -t.Amount,
                TransactionType.Debt => t.Amount,
                _ => 0
            });

            // Get all debts and subtract paid debt amounts from balance
            var debts = GetAll(userId);
            decimal paidDebtsAmount = debts
                .Where(d => d.IsPaid)
                .Sum(d => d.Amount);

            return transactionBalance - paidDebtsAmount;
        }

        public static List<DebtItem> Create(Guid userId, string debtName, DateTime dueDate,
            DebtType debtType, decimal amount, List<string> categories)
        {
            if (dueDate < DateTime.Today)
            {
                throw new Exception("Due date must be in the future.");
            }

            List<DebtItem> debts = GetAll(userId);
            debts.Add(new DebtItem
            {
                DebtName = debtName,
                DueDate = dueDate,
                CreatedBy = userId,
                DebtType = debtType,
                Amount = amount,
                Categories = categories
            });
            SaveAll(userId, debts);
            return debts;
        }

        public static List<DebtItem> Delete(Guid userId, Guid id)
        {
            List<DebtItem> debts = GetAll(userId);
            DebtItem debt = debts.FirstOrDefault(x => x.Id == id);
            if (debt == null)
            {
                throw new Exception("Debt not found.");
            }
            debts.Remove(debt);
            SaveAll(userId, debts);
            return debts;
        }

        public static void DeleteByUserId(Guid userId)
        {
            string debtsFilePath = Utils.GetDebtsFilePath(userId);
            if (File.Exists(debtsFilePath))
            {
                File.Delete(debtsFilePath);
            }
        }

        public static List<DebtItem> Update(Guid userId, Guid id, string debtName,DateTime dueDate, bool isPaid, DebtType debtType, decimal amount, List<string> categories)
        {
            try
            {
                List<DebtItem> debts = GetAll(userId);
                DebtItem debtToUpdate = debts.FirstOrDefault(x => x.Id == id);
                if (debtToUpdate == null)
                {
                    throw new Exception("Debt not found.");
                }

                // Only check balance for payment type AND when newly marking as paid
                if (debtType == DebtType.Payment && !debtToUpdate.IsPaid && isPaid)
                {
                    decimal overallBalance = CalculateOverallBalance(userId);
                    if (overallBalance < amount)
                    {
                        throw new Exception($"Insufficient overall balance. Available: {overallBalance:C}, Required: {amount:C}");
                    }
                }

                debtToUpdate.DebtName = debtName;
                debtToUpdate.DueDate = dueDate;
                debtToUpdate.DebtType = debtType;
                debtToUpdate.Amount = amount;
                debtToUpdate.Categories = categories;
                debtToUpdate.IsPaid = isPaid;  // Set IsPaid last after all validations pass

                SaveAll(userId, debts);
                return debts;
            }
            catch (Exception) when (AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                // Safely handle shutdown case
                return new List<DebtItem>();
            }
        }
    }
}