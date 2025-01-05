
using MauiApp6.Base;
using MauiApp6.Model;
using System.Text.Json;


namespace MauiApp6.Services;

public static class TransactionService
{
    private static void SaveAll(Guid Id, List<TransactionItem> todos)
    {
        string appDataDirectoryPath = Utils.GetAppDirectoryPath();
        string todosFilePath = Utils.GetTodosFilePath(Id);

        if (!Directory.Exists(appDataDirectoryPath))
        {
            Directory.CreateDirectory(appDataDirectoryPath);
        }

        var json = JsonSerializer.Serialize(todos);
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

    public static List<TransactionItem> Create(Guid userId, string taskName, DateTime dueDate,
      TransactionType transactionType, decimal amount, List<string> tags, string notes)
    {
        if (dueDate < DateTime.Today)
        {
            throw new Exception("Due date must be in the future.");
        }

        // For expenses, check if sufficient balance exists
        if (transactionType == TransactionType.Expense)
        {
            var transactions = GetAll(userId);
            decimal balance = transactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount) +
                             transactions.Where(t => t.TransactionType == TransactionType.Debt).Sum(t => t.Amount) -
                             transactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);

            if (balance < amount)
            {
                throw new Exception($"Insufficient balance. Available: {balance:C}, Required: {amount:C}");
            }
        }

        List<TransactionItem> todos = GetAll(userId);
        todos.Add(new TransactionItem
        {
            TaskName = taskName,
            DueDate = dueDate,
            CreatedBy = userId,
            TransactionType = transactionType,
            Amount = amount,
            Tags = tags,
             Notes = notes
        });
        SaveAll(userId, todos);
        return todos;
    }

    public static List<TransactionItem> Delete(Guid userId, Guid id)
    {
        List<TransactionItem> todos = GetAll(userId);
        TransactionItem todo = todos.FirstOrDefault(x => x.Id == id);

        if (todo == null)
        {
            throw new Exception("Todo not found.");
        }

        todos.Remove(todo);
        SaveAll(userId, todos);
        return todos;
    }

    public static void DeleteByUserId(Guid userId)
    {
        string todosFilePath = Utils.GetTodosFilePath(userId);
        if (File.Exists(todosFilePath))
        {
            File.Delete(todosFilePath);
        }
    }

    public static List<TransactionItem> Update(Guid userId, Guid id, string taskName,
    DateTime dueDate, bool isDone, TransactionType transactionType, decimal amount, List<string> tags, string notes)
    {
        List<TransactionItem> todos = GetAll(userId);
        TransactionItem todoToUpdate = todos.FirstOrDefault(x => x.Id == id);

        if (todoToUpdate == null)
        {
            throw new Exception("Todo not found.");
        }

        todoToUpdate.TaskName = taskName;
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

    