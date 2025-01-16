using MauiApp6.Model;
using MauiApp6.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MauiApp6.Components.Pages
{
    public partial class Debts
    {
        [CascadingParameter]
        private GlobalState _globalState { get; set; }
        private bool _showEditDebtDialog { get; set; }
        private bool _showDeleteDebtDialog { get; set; }
        private List<DebtItem> _debts { get; set; }
        private DebtItem _deleteDebt { get; set; }
        private string _dialogTitle { get; set; }
        private string _dialogOkLabel { get; set; }
        private string _editDebtErrorMessage { get; set; }
        private string _deleteDebtErrorMessage { get; set; }
        private DebtItem _debtModel { get; set; }
        private string _tabFilter = "All";
        private string _sortBy = "dueDate";
        private string _sortDirection = "ascending";
        private DateTime? fromDate;
        private DateTime? toDate;

        protected override void OnInitialized()
        {
            _debts = DebtService.GetAll(_globalState.CurrentUser.UserId);
        }
        private void ClearDateFilter()
        {
            fromDate = null;
            toDate = null;
        }
        private void SortByHandler(string sortByName)
        {
            if (_sortBy == sortByName)
            {
                _sortDirection = _sortDirection == "ascending" ? "descending" : "ascending";
            }
            else
            {
                _sortBy = sortByName;
                _sortDirection = "ascending";
            }
        }

            private List<string> _availableCategories = new()
            {
                "Personal", "Business", "Home", "Education", "Vehicle", "Credit Card", "Other"
            };

        private string _selectedDebtType = "All";
        private string _selectedCategory = "All";

        private void ToggleCategory(string category, bool isChecked)
        {
            if (isChecked && !_debtModel.Categories.Contains(category))
            {
                _debtModel.Categories.Add(category);
            }
            else if (!isChecked && _debtModel.Categories.Contains(category))
            {
                _debtModel.Categories.Remove(category);
            }
        }

        private void OpenAddDebtDialog()
        {
            _dialogTitle = "Add Debt";
            _dialogOkLabel = "Add";
            _debtModel = new DebtItem();
            _debtModel.Id = Guid.Empty;
            _showEditDebtDialog = true;
        }

        private void SearchDebtName(ChangeEventArgs e)
        {
            var searchTerm = e.Value.ToString();
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 2)
            {
                _debts = DebtService.GetAll(_globalState.CurrentUser.UserId).Where(t =>
                t.DebtName.ToLower().Contains(searchTerm.ToLower())).ToList();
            }
            else
            {
                _debts = DebtService.GetAll(_globalState.CurrentUser.UserId);
            }
        }

        private void OpenEditDebtDialog(DebtItem editDebt)
        {
            _dialogTitle = "Edit Debt";
            _dialogOkLabel = "Save";
            _debtModel = editDebt;
            _showEditDebtDialog = true;
        }

        private void OpenDeleteDebtDialog(DebtItem debtItem)
        {
            _deleteDebt = debtItem;
            _showDeleteDebtDialog = true;
        }

        private void updateDate(ChangeEventArgs e)
        {
            _debtModel.DueDate = DateTime.Parse(e.Value.ToString());
        }

        private void OnEditDebtDialogClose(bool isCancel)
        {
            if (isCancel)
            {
                _showEditDebtDialog = false;
            }
            else
            {
                try
                {
                    _editDebtErrorMessage = "";
                    if (_debtModel.Id == Guid.Empty)
                    {
                        // Creating new debt
                        _debts = DebtService.Create(
                            _globalState.CurrentUser.UserId,
                            _debtModel.DebtName,
                            _debtModel.DueDate,
                            _debtModel.DebtType,
                            _debtModel.Amount,
                            _debtModel.Categories
                        );
                    }
                    else
                    {
                        // Updating existing debt
                        _debts = DebtService.Update(
                            _globalState.CurrentUser.UserId,
                            _debtModel.Id,
                            _debtModel.DebtName,
                            _debtModel.DueDate,
                            _debtModel.IsPaid,
                            _debtModel.DebtType,
                            _debtModel.Amount,
                            _debtModel.Categories
                        );
                    }
                    _showEditDebtDialog = false;
                }
                catch (Exception e)
                {
                    _editDebtErrorMessage = e.Message;
                }
            }
        }

        private void OnDeleteDebtDialogClose(bool isCancel)
        {
            if (isCancel)
            {
                _showDeleteDebtDialog = false;
                _deleteDebt = null;
            }
            else
            {
                try
                {
                    _deleteDebtErrorMessage = "";
                    _debts = DebtService.Delete(_globalState.CurrentUser.UserId, _deleteDebt.Id);
                    _showDeleteDebtDialog = false;
                    _deleteDebt = null;
                }
                catch (Exception e)
                {
                    _deleteDebtErrorMessage = e.Message;
                }
            }
        }

        private void TabFilter(string status)
        {
            _tabFilter = status;
        }

        private void TogglePaid(DebtItem debt)
        {
            debt.IsPaid = !debt.IsPaid;
            // Using the passed debt parameter instead of _debtModel
            _debts = DebtService.Update(
                _globalState.CurrentUser.UserId,
                debt.Id,  // Changed from _debtModel.Id
                debt.DebtName,  // Changed from _debtModel.DebtName
                debt.DueDate,  // Changed from _debtModel.DueDate
                debt.IsPaid,  // Changed from _debtModel.IsPaid
                debt.DebtType,  // Changed from _debtModel.DebtType
                debt.Amount,  // Changed from _debtModel.Amount
                debt.Categories  // Changed from _debtModel.Categories
            );
        }
    }
}