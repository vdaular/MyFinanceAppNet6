﻿using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MainApp.Services;

public class ExpenseService : IExpenseService<ExpenseModel>
{
    [Inject]
    private IExpenseData<ExpenseModel> _expenseData { get; set; } = default!;


    [Inject]
    private ILocationExpenseService<LocationExpenseModel> _locationExpenseService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider _authProvider { get; set; } = default!;

    [Inject]
    private IUserData _userData { get; set; } = default!;

    private UserModel _loggedInUser { get; set; } = new();
    private decimal _expensesByDateRangeSum { get; set; } = 0;

    public ExpenseService(
        IExpenseData<ExpenseModel> expenseData,
        ILocationExpenseService<LocationExpenseModel> locationExpenseService,
        IUserData userData,
        AuthenticationStateProvider authProvider)
    {
        _expenseData = expenseData;
        _locationExpenseService = locationExpenseService;
        _userData = userData;
        _authProvider = authProvider;
    }

    public async Task ArchiveRecord(ExpenseModel model)
    {
        try
        {
            var user = await GetLoggedInUser();

            model.IsArchived = true;
            model.IsActive = false;
            model.UpdatedBy = user.Id;
            model.UpdatedAt = DateTime.Now;

            await _expenseData.ArchiveRecord(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task CreateRecord(ExpenseModel model)
    {
        try
        {
            var user = await GetLoggedInUser();

            model.UpdatedBy = user.Id;

            await _expenseData.CreateRecord(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public Task<ulong> GetLastInsertedId()
    {
        throw new NotImplementedException();
    }

    public async Task<ExpenseModel> GetRecordById(string modelId)
    {
        try
        {
            var user = await GetLoggedInUser();
            ExpenseModel result = await _expenseData.GetRecordById(user.Id, modelId);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public Task<List<ExpenseModel>> GetRecords()
    {
        throw new NotImplementedException();
    }

    public Task<List<ExpenseModel>> GetRecordsActive()
    {
        throw new NotImplementedException();
    }

    public async Task<List<ExpenseListDTO>> GetRecordsByDateRange(DateTimeRange dateTimeRange)
    {
        try
        {
            var user = await GetLoggedInUser();
            List<ExpenseListDTO> results = await _expenseData.GetRecordsByDateRange(user.Id, dateTimeRange);
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task<decimal> GetRecordsByDateRangeSum()
    {
        try
        {
            return await Task.FromResult(_expensesByDateRangeSum);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task<List<ExpenseByCategoryGroupDTO>> GetRecordsByGroupAndDateRange(DateTimeRange dateTimeRange)
    {
        try
        {
            var records = await GetRecordsByDateRange(dateTimeRange);
            var resultsByGroup = records.GroupBy(tc => tc.ExpenseCategoryDescription);
            var results = resultsByGroup.Select(tcGroup => new ExpenseByCategoryGroupDTO()
            {
                Description = tcGroup.Key,
                Color = tcGroup.Select(c => c.ExpenseCategoryColor).FirstOrDefault(),
                Total = tcGroup.Sum(a => a.Amount),
                Expenses = tcGroup.ToList()
            }).ToList();

            _expensesByDateRangeSum = records.Sum(t => t.Amount);

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task<List<ExpenseCalendarDTO>> GetRecordsCalendarView(DateTimeRange dateTimeRange)
    {
        try
        {
            var records = await GetRecordsByDateRange(dateTimeRange);
            var resultsByGroupDay = records.GroupBy(d => d.EDate);

            List<ExpenseCalendarDTO> results = new();

            foreach (var record in records)
            {
                ExpenseCalendarDTO expenseCalendarDTO = new();

                var result = results.Find(e => e.ExpenseCategoryDescription == record.ExpenseCategoryDescription &&
                                               e.EDate.Date == record.EDate.Date);

                if (result is not null)
                {
                    result.Amount += record.Amount;
                }
                else
                {
                    expenseCalendarDTO.EDate = record.EDate;
                    expenseCalendarDTO.ExpenseCategoryColor = record.ExpenseCategoryColor;
                    expenseCalendarDTO.ExpenseCategoryDescription = record.ExpenseCategoryDescription;
                    expenseCalendarDTO.Amount = record.Amount;
                    results.Add(expenseCalendarDTO);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public Task<List<ExpenseModel>> GetSearchResults(string search)
    {
        throw new NotImplementedException();
    }

    public Task UpdateRecord(ExpenseModel model)
    {
        throw new NotImplementedException();
    }

    public Task UpdateRecordStatus(ExpenseModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ExpenseLast3MonthsGraphDTO>> GetRecordsLast3Months()
    {
        try
        {
            var user = await GetLoggedInUser();
            List<ExpenseLast3MonthsGraphDTO> results = await _expenseData.GetRecordsLast3Months(user.Id);
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task<List<ExpenseLast5YearsGraphDTO>> GetRecordsLast5Years()
    {
        try
        {
            var user = await GetLoggedInUser();
            List<ExpenseLast5YearsGraphDTO> results = await _expenseData.GetRecordsLast5Years(user.Id);
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task<List<ExpenseAmountHistoryDTO>> GetAmountHistory()
    {
        try
        {
            var user = await GetLoggedInUser();
            List<ExpenseAmountHistoryDTO> results = await _expenseData.GetAmountHistoryDTO(user.Id);
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    public async Task<GoogleMapStaticModel> GetLocationExpense(DateTimeRange dateTimeRange, MapMarkerColor mapMarkerColor, MapSize mapSizeWidth, MapSize mapSizeHeight)
    {
        try
        {
            List<LocationExpenseDTO> results = await _locationExpenseService.GetRecordsByDateRange(dateTimeRange);

            string location = await BuildLocation(results);

            GoogleMapStaticModel model = new();
            model.Location = location;
            model.Marker = $"color:{mapMarkerColor.ToString().ToLower()}";
            model.Scale = "2";
            model.Width = (int)mapSizeWidth;
            model.Height = (int)mapSizeHeight;

            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            throw;
        }
    }

    private async Task<UserModel> GetLoggedInUser()
    {
        return _loggedInUser = await _authProvider.GetUserFromAuth(_userData);
    }

    private static Task<string> BuildLocation(List<LocationExpenseDTO> locations)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var location in locations)
        {
            sb.Append($"{location.Latitude},");
            sb.Append($"{location.Longitude}|");
        }

        return Task.FromResult(sb.ToString());
    }
}
