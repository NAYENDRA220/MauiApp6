
using MauiApp6.Base;
using MauiApp6.Model;
using System.Text.Json;// Facilitates JSON serialization and deserialization


namespace MauiApp6.Services;

public static class TransactionService
{
    private static void SaveAll(Guid userId, List<TransactionItem> transactions)
    {
        string appDataDirectoryPath = Utils.GetAppDirectoryPath();
        string transactionFilePath = Utils.GetTransactionFilePath(userId);

        if (!Directory.Exists(appDataDirectoryPath))
        {
            Directory.CreateDirectory(appDataDirectoryPath);
        }

        var json = JsonSerializer.Serialize(transactions);
        File.WriteAllText(transactionFilePath, json);
    }

    public static List<TransactionItem> GetAll(Guid userId)
    {
        string transactionFilePath = Utils.GetTransactionFilePath(userId);
        if (!File.Exists(transactionFilePath))
        {
            return new List<TransactionItem>();
        }

        var json = File.ReadAllText(transactionFilePath);

        return JsonSerializer.Deserialize<List<TransactionItem>>(json);
    }

    //Creates a new transaction for a user and adds it to their transaction list.
    public static List<TransactionItem> Create(Guid userId, string TransactionName, DateTime dueDate,
      TransactionType transactionType, decimal amount, List<string> tags, string notes)
    {

        // Validate that the due date is not in the past
        if (dueDate < DateTime.Today)
        {
            throw new Exception("Due date must be in the future.");
        }

        // For expenses, check if sufficient balance exists
        if (transactionType == TransactionType.Expense)
        {
            var trans = GetAll(userId);
            var debts = DebtService.GetAll(userId);

            // Calculate total income (including debts/loans received)
            decimal totalIncome = trans.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount);
                                

            // Calculate total expenses
            decimal totalExpenses = trans.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);

            // Calculate total paid debts
            decimal totalPaidDebts = debts.Where(d => d.IsPaid).Sum(d => d.Amount);

            // Calculate final balance
            decimal balance = totalIncome - totalExpenses - totalPaidDebts;
            if (balance < amount)
            {
                throw new Exception($"Insufficient balance. Available: {balance:C}, Required: {amount:C}");
            }
        }

        List<TransactionItem> transactions = GetAll(userId);
        transactions.Add(new TransactionItem
        {
            TransactionName = TransactionName,
            DueDate = dueDate,
            CreatedBy = userId,
            TransactionType = transactionType,
            Amount = amount,
            Tags = tags,
             Notes = notes
        });
        SaveAll(userId, transactions);
        return transactions;
       
    }

    public static List<TransactionItem> Delete(Guid userId, Guid id)
    {
        List<TransactionItem> transactions = GetAll(userId);
        TransactionItem transaction = transactions.FirstOrDefault(x => x.Id == id);

        if (transactions == null)
        {
            throw new Exception("Transaction data not found.");
        }

        transactions.Remove(transaction);
        SaveAll(userId, transactions);
        return transactions;
    }

    public static void DeleteByUserId(Guid userId)
    {
        string transactionFilePath = Utils.GetTransactionFilePath(userId);
        if (File.Exists(transactionFilePath))
        {
            File.Delete(transactionFilePath);
        }
    }

    public static List<TransactionItem> Update(Guid userId, Guid id, string TransactionName,
    DateTime dueDate, bool isDone, TransactionType transactionType, decimal amount, List<string> tags, string notes)
    {
        List<TransactionItem> transactions = GetAll(userId);
        TransactionItem transactionToUpdate = transactions.FirstOrDefault(x => x.Id == id);

        if (transactionToUpdate == null)
        {
            throw new Exception("Transaction data not found.");
        }

        transactionToUpdate.TransactionName = TransactionName;
        transactionToUpdate.IsDone = isDone;
        transactionToUpdate.DueDate = dueDate;
        transactionToUpdate.TransactionType = transactionType;
        transactionToUpdate.Amount = amount;
        transactionToUpdate.Tags = tags;
        transactionToUpdate.Notes = notes;
        
        SaveAll(userId, transactions);
        return transactions;
    }
}

    