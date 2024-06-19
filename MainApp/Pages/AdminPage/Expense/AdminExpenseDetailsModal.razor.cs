﻿using MainApp.Components.Modal;
using MainApp.Components.Toast;
using Microsoft.AspNetCore.Components;

namespace MainApp.Pages.AdminPage.Expense;

public partial class AdminExpenseDetailsModal : ComponentBase
{
    [Inject]
    private ToastService _toastService { get; set; } = default!;

    [Inject]
    private IExpenseService<ExpenseModel> _expenseService { get; set; } = default!;

    [Inject]
    private IGoogleService _googleService { get; set; } = default!;

    [CascadingParameter(Name = "AppSettings")]
    protected AppSettings _appSettings { get; set; } = new();

    // Add Modal component reference
    private AdminExpenseModal _setupModal { get; set; } = new();

    private Modal _modal { get; set; } = new();
    private Guid _modalTarget { get; set; }
    private bool _isLoading { get; set; } = true;
    List<ExpenseDetailsDTO> _expensesList {get; set; } = new();
    private DateTimeRange _dateTimeRange { get; set; } = new();
    private GoogleMapStaticModel _googleMapStatic{ get; set; } = new();
    private Response<string> _response { get; set; } = new();
    private string _imageURL { get; set; } = string.Empty;

    public AdminExpenseDetailsModal()
    {
    }

    public async Task OpenModalAsync(DateTime date)
    {
        try
        {
            _modalTarget = Guid.NewGuid();

            _dateTimeRange.Start = date;
            _dateTimeRange.End = date;

            await FetchDataAsync();

            await Task.FromResult(_modal.Open(_modalTarget));
        }
        catch (Exception ex)
        {
            _toastService.ShowToast(ex.Message, Theme.Danger);
        }

        await Task.CompletedTask;
    }

    private async Task CloseModalAsync()
    {
        _expensesList = new();
        _googleMapStatic = new();
        _response = new();
        _isLoading = false;
        _imageURL = string.Empty;

        await Task.FromResult(_modal.Close(_modalTarget));
        await Task.CompletedTask;
    }

    private async Task FetchDataAsync()
    {
        try
        {
            _expensesList = await _expenseService.GetRecordsDateView(_dateTimeRange);

            if (_expensesList.Count > 0)
            {
                _googleMapStatic = await _expenseService.GetLocationExpense(_dateTimeRange, MapMarkerColor.Brown, MapSize.Width800, MapSize.Height250, MapScale.Desktop);
                _response = await _googleService.GetMapStaticImage(_googleMapStatic);

                if (_response.Success is false)
                {
                    _toastService.ShowToast($"{_response.ErrorMessage}", Theme.Danger);
                }
                else{
                    _imageURL = _response.Data;
                }
            }
            else{
                _imageURL = string.Empty;
            }
            _isLoading = false;
        }
        catch (Exception ex)
        {
            _isLoading = false;
            _toastService.ShowToast(ex.Message, Theme.Danger);
        }

        await Task.CompletedTask;
    }
}
