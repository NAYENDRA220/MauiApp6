
using MauiApp6.Base;
using MauiApp6.Model;
using Microsoft.AspNetCore.Components;
using System.Text.Json;


namespace MauiApp6.Services;

public static class TransactionService
{
    private static void SaveAll(Guid Id, List<TransactionItem> transactions)
    {
        string appDataDirectoryPath = Utils.GetAppDirectoryPath();
        string todosFilePath = Utils.GetTodosFilePath(Id);

        if (!Directory.Exists(appDataDirectoryPath))
        {
            Directory.CreateDirectory(appDataDirectoryPath);
        }

        var json = JsonSerializer.Serialize(transactions);
        File.WriteAllText(todosFilePath, json);
    }

    public static List<TransactionItem> GetAll(Guid userId)
    {
        string todosFilePath = Utils.GetTodosFilePath(userId);
        if (!File.Exists(todosFilePath))
        {
            return new List<TransactionItem>();
        }

        var json = File.ReadAllText(todosFilePath);

        return JsonSerializer.Deserialize<List<TransactionItem>>(json);
    }

    public static List<TransactionItem> Create(Guid userId, string TransactionName, DateTime dueDate,
      TransactionType transactionType, decimal amount, List<string> tags, string notes)
    {

       
        if (dueDate < DateTime.Today)
        {
            throw new Exception("Due date must be in the future.");
        }

        // For expenses, check if sufficient balance exists
        if (transactionType == TransactionType.Expense)
        {
            var trans = GetAll(userId);
            decimal balance = trans.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount) +
                             trans.Where(t => t.TransactionType == TransactionType.Debt).Sum(t => t.Amount) -
                             trans.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);

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
            throw new Exception("Todo not found.");
        }

        transactions.Remove(transaction);
        SaveAll(userId, transactions);
        return transactions;
    }

    public static void DeleteByUserId(Guid userId)
    {
        string todosFilePath = Utils.GetTodosFilePath(userId);
        if (File.Exists(todosFilePath))
        {
            File.Delete(todosFilePath);
        }
    }

    public static List<TransactionItem> Update(Guid userId, Guid id, string TransactionName,
    DateTime dueDate, bool isDone, TransactionType transactionType, decimal amount, List<string> tags, string notes)
    {
        List<TransactionItem> todos = GetAll(userId);
        TransactionItem todoToUpdate = todos.FirstOrDefault(x => x.Id == id);

        if (todoToUpdate == null)
        {
            throw new Exception("Todo not found.");
        }

        todoToUpdate.TransactionName = TransactionName;
        todoToUpdate.IsDone = isDone;
        todoToUpdate.DueDate = dueDate;
        todoToUpdate.TransactionType = transactionType;
        todoToUpdate.Amount = amount;
        todoToUpdate.Tags = tags;
        todoToUpdate.Notes = notes;
        
        SaveAll(userId, todos);
        return todos;
    }
}

    