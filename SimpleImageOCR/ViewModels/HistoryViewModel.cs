using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleImageOCR.Models;
using SimpleImageOCR.Services;
using System.Collections.ObjectModel;

namespace SimpleImageOCR.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly IShareService _shareService;

        [ObservableProperty]
        private ObservableCollection<OcrResult> ocrResults = new();

        [ObservableProperty]
        private ObservableCollection<OcrResult> filteredResults = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool hasResults = false;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private int totalResults = 0;

        [ObservableProperty]
        private bool isSelectionMode = false;

        [ObservableProperty]
        private ObservableCollection<OcrResult> selectedResults = new();

        public HistoryViewModel(DatabaseService databaseService, IShareService shareService)
        {
            _databaseService = databaseService;
            _shareService = shareService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterResults();
        }

        public async Task LoadHistoryAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "📚 Loading history...";

                var results = await _databaseService.GetAllResultsAsync();
                
                OcrResults.Clear();
                foreach (var result in results.OrderByDescending(r => r.CreatedAt))
                {
                    OcrResults.Add(result);
                }

                TotalResults = OcrResults.Count;
                HasResults = TotalResults > 0;
                
                FilterResults();
                
                StatusMessage = HasResults ? $"📄 {TotalResults} results found" : "📭 No saved results yet";
                
                // Clear status message after 2 seconds
                await Task.Delay(2000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Failed to load history: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterResults()
        {
            FilteredResults.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? OcrResults
                : OcrResults.Where(r => r.ExtractedText.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                       r.FormattedDate.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var result in filtered)
            {
                FilteredResults.Add(result);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadHistoryAsync();
        }

        [RelayCommand]
        private async Task ViewResultAsync(OcrResult result)
        {
            if (result == null) return;

            try
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    ["ExtractedText"] = result.ExtractedText,
                    ["ImagePath"] = result.ImagePath ?? string.Empty
                };

                await Shell.Current.GoToAsync("//OcrResultPage", navigationParameter);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Navigation failed: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteResultAsync(OcrResult result)
        {
            if (result == null) return;

            try
            {
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Delete Result",
                    "Are you sure you want to delete this OCR result?",
                    "Delete",
                    "Cancel");

                if (confirmed)
                {
                    IsLoading = true;
                    StatusMessage = "🗑️ Deleting...";

                    await _databaseService.DeleteOcrResultAsync(result.Id);
                    
                    OcrResults.Remove(result);
                    FilteredResults.Remove(result);
                    TotalResults = OcrResults.Count;
                    HasResults = TotalResults > 0;

                    StatusMessage = "✅ Result deleted";
                    
                    // Clear status message after 2 seconds
                    await Task.Delay(2000);
                    StatusMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Delete failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShareResultAsync(OcrResult result)
        {
            if (result == null || string.IsNullOrWhiteSpace(result.ExtractedText)) return;

            try
            {
                IsLoading = true;
                StatusMessage = "📤 Sharing...";

                await _shareService.ShareTextAsync(result.ExtractedText);
                
                StatusMessage = "📤 Share dialog opened";
                
                // Clear status message after 2 seconds
                await Task.Delay(2000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Share failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleSelectionMode()
        {
            IsSelectionMode = !IsSelectionMode;
            if (!IsSelectionMode)
            {
                SelectedResults.Clear();
            }
        }

        [RelayCommand]
        private void ToggleResultSelection(OcrResult result)
        {
            if (result == null) return;

            if (SelectedResults.Contains(result))
            {
                SelectedResults.Remove(result);
            }
            else
            {
                SelectedResults.Add(result);
            }
        }

        [RelayCommand]
        private async Task DeleteSelectedAsync()
        {
            if (SelectedResults.Count == 0) return;

            try
            {
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Delete Results",
                    $"Are you sure you want to delete {SelectedResults.Count} selected results?",
                    "Delete",
                    "Cancel");

                if (confirmed)
                {
                    IsLoading = true;
                    StatusMessage = $"🗑️ Deleting {SelectedResults.Count} results...";

                    var resultsToDelete = SelectedResults.ToList();
                    
                    foreach (var result in resultsToDelete)
                    {
                        await _databaseService.DeleteOcrResultAsync(result.Id);
                        OcrResults.Remove(result);
                        FilteredResults.Remove(result);
                    }

                    SelectedResults.Clear();
                    TotalResults = OcrResults.Count;
                    HasResults = TotalResults > 0;
                    IsSelectionMode = false;

                    StatusMessage = "✅ Selected results deleted";
                    
                    // Clear status message after 2 seconds
                    await Task.Delay(2000);
                    StatusMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Delete failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShareSelectedAsync()
        {
            if (SelectedResults.Count == 0) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"📤 Sharing {SelectedResults.Count} results...";

                var combinedText = string.Join("\n\n---\n\n", SelectedResults.Select(r => r.ExtractedText));
                await _shareService.ShareMultipleTextsAsync(SelectedResults.ToList());

                StatusMessage = "📤 Combined text shared";
                
                // Clear status message after 2 seconds
                await Task.Delay(2000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Share failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ClearAllAsync()
        {
            if (OcrResults.Count == 0) return;

            try
            {
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Clear All History",
                    "Are you sure you want to delete ALL saved OCR results? This action cannot be undone.",
                    "Clear All",
                    "Cancel");

                if (confirmed)
                {
                    IsLoading = true;
                    StatusMessage = "🗑️ Clearing all history...";

                    await _databaseService.ClearAllOcrResultsAsync();
                    
                    OcrResults.Clear();
                    FilteredResults.Clear();
                    SelectedResults.Clear();
                    TotalResults = 0;
                    HasResults = false;
                    IsSelectionMode = false;

                    StatusMessage = "✅ All history cleared";
                    
                    // Clear status message after 2 seconds
                    await Task.Delay(2000);
                    StatusMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Clear failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NewScanAsync()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}