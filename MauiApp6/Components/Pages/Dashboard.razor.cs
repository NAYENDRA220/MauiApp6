using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Common.Time;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;
using MauiApp6.Base;
using MauiApp6.Model;
using MauiApp6.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MauiApp6.Components.Pages
{
    public partial class Dashboard
    {
        [CascadingParameter]
        private GlobalState _globalState { get; set; } // Global state for accessing user-related information.
        private List<TransactionItem> _transactions = new(); // Stores the user's transactions.
        private List<DebtItem> _debts = new(); // Stores the user's debts.
        private Chart _pieChart; // Instance of PieChart used for debt distribution.
        private Chart _barChart; // Instance of BarChart used for income vs. expense.
        private Chart _lineChart; // Instance of LineChart used for transaction history.
        private PieConfig _pieChartConfig; // Configuration for the pie chart.
        private BarConfig _barChartConfig; // Configuration for the bar chart.
        private LineConfig _lineChartConfig; // Configuration for the line chart.


        protected override void OnInitialized()
        {
            // Fetches transactions and debts for the current user from the service.
            _transactions = TransactionService.GetAll(_globalState.CurrentUser.UserId);
            _debts = DebtService.GetAll(_globalState.CurrentUser.UserId);
            // Initializes all chart configurations and updates their data.
            InitializeCharts();
            UpdateLineChart();
            InitializeTopTransactionsChart(); // 
            UpdateTopTransactionsChart();
        }


        private decimal GetTotalByType(TransactionType type)
        {
            // the total amount for a specific transaction type (e.g., income, expense).
            return _transactions
                .Where(t => t.TransactionType == type)
                .Sum(t => t.Amount);
        }

        private decimal GetTotalPaidDebts()
        {
            //  the total amount for debts that are marked as paid.
            return _debts
                .Where(d => d.IsPaid)
                .Sum(d => d.Amount);
        }

        private decimal GetTotalUnpaidDebts()
        {
            //  the total amount for debts that are marked as not paid.
            return _debts
                .Where(d => !d.IsPaid)
                .Sum(d => d.Amount);
        }

        private decimal GetTotalDebtsByType(DebtType type)
        {
            return _debts
                .Where(d => d.DebtType == type)
                .Sum(d => d.Amount);
        }

        private decimal GetPaidDebtsByType(DebtType type)
        {
            return _debts
                .Where(d => d.DebtType == type && d.IsPaid)
                .Sum(d => d.Amount);
        }

        private decimal GetUnpaidDebtsByType(DebtType type)
        {
            return _debts
                .Where(d => d.DebtType == type && !d.IsPaid)
                .Sum(d => d.Amount);
        }

        private decimal GetAvailableBalance()
        {
            // Calculates the user's available balance by subtracting total expenses
            // and paid debts from total income. Also considers unpaid loans.
            decimal transactionBalance = GetTotalByType(TransactionType.Income) -
                                       GetTotalByType(TransactionType.Expense);

            decimal paidDebts = _debts
                .Where(d => d.IsPaid)
                .Sum(d => d.Amount);

            decimal unpaidLoans = _debts
              .Where(d => !d.IsPaid && d.DebtType == DebtType.Loan)
               .Sum(d => d.Amount);

            return transactionBalance - paidDebts;
        }

        private void InitializeCharts()
        {
            // Sets up the configurations for all charts (pie, bar, line).
            InitializePieChart();
            InitializeBarChart();
            InitializeLineChart();
           
        }
        // Pie chart setup
        private void InitializePieChart()
        {
            _pieChartConfig = new PieConfig
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Debt Distribution by Type"
                    }
                }
            };

            var debtTypes = Enum.GetValues(typeof(DebtType));
            foreach (DebtType type in debtTypes)
            {
                _pieChartConfig.Data.Labels.Add(type.ToString());
            }

            var pieDataset = new PieDataset<decimal>
            {
                BackgroundColor = new[]
                        {
                ColorUtil.ColorHexString(255, 99, 132),
                ColorUtil.ColorHexString(54, 162, 235),
                ColorUtil.ColorHexString(255, 206, 86),
                ColorUtil.ColorHexString(75, 192, 192)
            },
                BorderWidth = 1
            };

            foreach (DebtType type in debtTypes)
            {
                pieDataset.Add(GetTotalDebtsByType(type));
            }

            _pieChartConfig.Data.Datasets.Add(pieDataset);
        }
        // Bar graph setup
        private void InitializeBarChart()
        {
            _barChartConfig = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Income vs Expense Overview"
                    },
                    Scales = new BarScales
                    {
                        YAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks
                            {
                                BeginAtZero = true
                            }
                        }
                    }
                    }
                }
            };

            foreach (var label in new[] { "Income", "Expense" })
            {
                _barChartConfig.Data.Labels.Add(label);
            }

            var barDataset = new BarDataset<decimal>
            {
                Label = "Amount",
                BackgroundColor = new[]
                        {
                ColorUtil.ColorHexString(75, 192, 192),
                ColorUtil.ColorHexString(255, 99, 132)
            },
                BorderWidth = 1
            };

            barDataset.Add(GetTotalByType(TransactionType.Income));
            barDataset.Add(GetTotalByType(TransactionType.Expense));

            _barChartConfig.Data.Datasets.Add(barDataset);
        }
        // line chart start
        private DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        private DateTime EndDate { get; set; } = DateTime.Now;

        private void ResetDateRange()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            UpdateLineChart();
        }
        private void InitializeLineChart()
        {
            _lineChartConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Transaction History Over Time"
                    },
                    Scales = new Scales
                    {
                        XAxes = new List<CartesianAxis>
                  {
                      new TimeAxis
                      {
                          Distribution = TimeDistribution.Linear,
                          Time = new TimeOptions
                          {
                              Unit = TimeMeasurement.Day,
                              DisplayFormats = new Dictionary<TimeMeasurement, string>
                              {
                                  { TimeMeasurement.Day, "MMM dd" }
                              }
                          }
                      }
                  }
                    }
                }
            };
        }

        private void UpdateLineChart()
        {
            if (EndDate < StartDate)
            {
                EndDate = StartDate.AddDays(1);
            }

            // Clear existing datasets
            _lineChartConfig.Data.Datasets.Clear();

            var filteredTransactions = _transactions
                .Where(t => t.CreatedAt.Date >= StartDate.Date && t.CreatedAt.Date <= EndDate.Date)
                .OrderBy(t => t.CreatedAt)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Income = g.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                    
                })
                .ToList();
            // Get debts from DebtService
            var filteredDebts = _debts
                .Where(d => d.CreatedAt.Date >= StartDate.Date && d.CreatedAt.Date <= EndDate.Date)
                .OrderBy(d => d.CreatedAt)
                .GroupBy(d => d.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    DebtAmount = g.Sum(d => d.Amount)
                })
                .ToList();
            var incomeDataset = new LineDataset<TimePoint>
            {
                Label = "Income",
                BorderColor = ColorUtil.ColorHexString(75, 192, 192),
                Fill = false
            };

            var expenseDataset = new LineDataset<TimePoint>
            {
                Label = "Expense",
                BorderColor = ColorUtil.ColorHexString(255, 99, 132),
                Fill = false
            };

            var debtDataset = new LineDataset<TimePoint>
            {
                Label = "Debt",
                BorderColor = ColorUtil.ColorHexString(255, 206, 86),
                Fill = false
            };

            // Get all unique dates from both transactions and debts
            var allDates = filteredTransactions.Select(t => t.Date)
                .Union(filteredDebts.Select(d => d.Date))
                .OrderBy(date => date)
                .ToList();

            foreach (var date in allDates)
            {
                var transactionData = filteredTransactions.FirstOrDefault(t => t.Date == date);
                var debtData = filteredDebts.FirstOrDefault(d => d.Date == date);

                // Add income data point
                incomeDataset.Add(new TimePoint(
                    date,
                    Convert.ToDouble(transactionData?.Income ?? 0)
                ));

                // Add expense data point
                expenseDataset.Add(new TimePoint(
                    date,
                    Convert.ToDouble(transactionData?.Expense ?? 0)
                ));

                // Add debt data point
                debtDataset.Add(new TimePoint(
                    date,
                    Convert.ToDouble(debtData?.DebtAmount ?? 0)
                ));
            }

            _lineChartConfig.Data.Datasets.Add(incomeDataset);
            _lineChartConfig.Data.Datasets.Add(expenseDataset);
            _lineChartConfig.Data.Datasets.Add(debtDataset);

            StateHasChanged();
        }
         //Highest bar graph start
        private BarConfig _topTransactionsConfig;
        private Chart _topTransactionsChart;
        private string _selectedView = "highest"; // or "lowest"
        private string _selectedType = "income"; // "income", "expense", "debt"

        private void InitializeTopTransactionsChart()
        {
            _topTransactionsConfig = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = $"Top 5 {_selectedView} {_selectedType}"
                    },
                    Scales = new BarScales
                    {
                        YAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks
                            {
                                BeginAtZero = true
                            }
                        }
                    }
                    }
                }
            };
        }

        private void UpdateTopTransactionsChart()
        {
            // Clear existing data
            _topTransactionsConfig.Data.Labels.Clear();
            _topTransactionsConfig.Data.Datasets.Clear();

            var barDataset = new BarDataset<decimal>
            {
                BorderWidth = 1
            };

            // Handle debts separately from transactions
            if (_selectedType.ToLower() == "debt")
            {
                var filteredDebts = _debts
                    .Where(d => d.CreatedAt.Date >= StartDate.Date &&
                               d.CreatedAt.Date <= EndDate.Date)
                    .OrderBy(d => _selectedView == "highest" ? -d.Amount : d.Amount)
                    .Take(5)
                    .ToList();

                barDataset.Label = "Debt Amount";
                barDataset.BackgroundColor = ColorUtil.ColorHexString(255, 206, 86);

                foreach (var debt in filteredDebts)
                {
                    _topTransactionsConfig.Data.Labels.Add($"{debt.DebtName}\n{debt.CreatedAt:MM/dd}");
                    barDataset.Add(debt.Amount);
                }
            }
            else
            {
                // Handle income and expenses
                var filteredTransactions = _transactions
                    .Where(t => t.CreatedAt.Date >= StartDate.Date &&
                               t.CreatedAt.Date <= EndDate.Date);

                TransactionType type = _selectedType.ToLower() == "income" ?
                    TransactionType.Income : TransactionType.Expense;

                var transactions = filteredTransactions
                    .Where(t => t.TransactionType == type)
                    .OrderBy(t => _selectedView == "highest" ? -t.Amount : t.Amount)
                    .Take(5)
                    .ToList();

                barDataset.Label = $"{char.ToUpper(_selectedType[0])}{_selectedType[1..]} Amount";
                barDataset.BackgroundColor = type == TransactionType.Income ?
                    ColorUtil.ColorHexString(75, 192, 192) :
                    ColorUtil.ColorHexString(255, 99, 132);

                foreach (var transaction in transactions)
                {
                    _topTransactionsConfig.Data.Labels.Add($"{transaction.TransactionName}\n{transaction.CreatedAt:MM/dd}");
                    barDataset.Add(transaction.Amount);
                }
            }

            _topTransactionsConfig.Data.Datasets.Add(barDataset);
            StateHasChanged();
        }


    }
}