using MauiApp6.Model;
using MauiApp6.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MauiApp6.Components.Pages
{
    public partial class Home
    {
        [CascadingParameter]
        private GlobalState _globalState { get; set; }
        private bool _showEditTransactionDialog { get; set; }
        private bool _showDeleteTransactionDialog { get; set; }
        private List<TransactionItem> _transaction { get; set; }
        private TransactionItem _deleteTransaction { get; set; }
        private string _dialogTitle { get; set; }
        private string _dialogOkLabel { get; set; }
        private string _editTransactionErrorMessage { get; set; }
        private string _deleteTransactionErrorMessage { get; set; }
        private TransactionItem _transactionModel { get; set; }
        private string _transactionsortBy = "dueDate";
        private string _transactionsortDirection = "ascending";
        private DateTime? _fromDate;
        private DateTime? _toDate;



        // Add this method with your other private methods
        private void ClearDateFilter()
        {
            _fromDate = null;
            _toDate = null;
        }


        protected override void OnInitialized()
        {
            _transaction = TransactionService.GetAll(_globalState.CurrentUser.UserId);
        }

        private void SortByHandler(string sortByName)
        {
            if (_transactionsortBy == sortByName)
            {
                _transactionsortDirection = _transactionsortDirection == "ascending" ? "descending" : "ascending";
            }
            else
            {
                _transactionsortBy = sortByName;
                _transactionsortDirection = "ascending";
            }
        }
        private List<string> _availableTags = new()
    {
    "Yearly", "Monthly", "Food & Drinks", "Clothes", "Gadgets", "Miscellaneous"
    };

        private string _selectedTransactionType = "All";
        private string _selectedTag = "All";

        private void ToggleTag(string tag, bool isChecked)
        {
            if (isChecked && !_transactionModel.Tags.Contains(tag))
            {
                _transactionModel.Tags.Add(tag);
            }
            else if (!isChecked && _transactionModel.Tags.Contains(tag))
            {
                _transactionModel.Tags.Remove(tag);
            }
        }



        private void OpenAddTransactionDialog()
        {
            _dialogTitle = "Add Transaction";
            _dialogOkLabel = "Add";
            _transactionModel = new TransactionItem();
            _transactionModel.Id = Guid.Empty;
            _showEditTransactionDialog = true;
        }

        private void SearchTaskName(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString();
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 2)
            {
                _transaction = TransactionService.GetAll(_globalState.CurrentUser.UserId).Where(t =>
                t.TransactionName.ToLower().Contains(searchTerm.ToLower())).ToList();
            }
            else
            {
                _transaction = TransactionService.GetAll(_globalState.CurrentUser.UserId);
            }
        }

        private void OpenEditTodoDialog(TransactionItem editT)
        {
            _dialogTitle = "Edit Transaction";
            _dialogOkLabel = "Save";

            _transactionModel = editT;

            _showEditTransactionDialog = true;
        }

        private void OpenDeleteTransactionDialog(TransactionItem tItem)
        {
            _deleteTransaction = tItem;
            _showDeleteTransactionDialog = true;
        }

        private void updateDate(ChangeEventArgs e)
        {
            _transactionModel.DueDate = DateTime.Parse(e.Value.ToString());
        }
        private void OnEditTransactionDialogClose(bool isCancel)
        {
            if (isCancel)
            {
                _showEditTransactionDialog = false;
            }
            else
            {
                try
                {
                    _editTransactionErrorMessage = "";
                    if (_transactionModel.Id == Guid.Empty)
                    {
                        _transaction = TransactionService.Create(_globalState.CurrentUser.UserId, _transactionModel.TransactionName, _transactionModel.DueDate, _transactionModel.TransactionType, _transactionModel.Amount, _transactionModel.Tags, _transactionModel.Notes);
                    }
                    else
                    {
                        _transaction = TransactionService.Update(_globalState.CurrentUser.UserId, _transactionModel.Id, _transactionModel.TransactionName, _transactionModel.DueDate,
                        _transactionModel.IsDone, _transactionModel.TransactionType, _transactionModel.Amount, _transactionModel.Tags, _transactionModel.Notes);
                    }

                    _showEditTransactionDialog = false;
                }
                catch (Exception e)
                {
                    _editTransactionErrorMessage = e.Message;
                }
            }
        }
        private void OnDeleteTransactionDialogClose(bool isCancel)
        {
            if (isCancel)
            {

                _showDeleteTransactionDialog = false;
                _deleteTransaction = null;
            }
            else
            {
                try
                {
                    _deleteTransactionErrorMessage = "";
                    _transaction = TransactionService.Delete(_globalState.CurrentUser.UserId, _deleteTransaction.Id);
                    _showDeleteTransactionDialog = false;
                    _deleteTransaction = null;
                }
                catch (Exception e)
                {
                    _deleteTransactionErrorMessage = e.Message;
                }
            }
        }


    }
}