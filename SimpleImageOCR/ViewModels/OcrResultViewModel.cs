using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.OCR;
using SimpleImageOCR.Models;
using SimpleImageOCR.Services;
using System.Collections.ObjectModel;

namespace SimpleImageOCR.ViewModels
{
    public partial class OcrResultViewModel : ObservableObject, IQueryAttributable
    {
        private readonly DatabaseService _databaseService;
        private readonly IShareService _shareService;
        private readonly OcrService _ocrService;

        [ObservableProperty]
        private string extractedText = string.Empty;

        [ObservableProperty]
        private string imagePath = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isSaved = false;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> shareOptions = new();

        public OcrResultViewModel(DatabaseService databaseService, IShareService shareService, OcrService ocrService)
        {
            _databaseService = databaseService;
            _shareService = shareService;
            _ocrService = ocrService;
            
            InitializeShareOptions();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("ExtractedText", out var nameValue))
                ExtractedText = nameValue?.ToString();

            if (query.TryGetValue("ImagePath", out nameValue))
                ImagePath = nameValue?.ToString();
          }

        private void InitializeShareOptions()
        {
            ShareOptions.Clear();
            ShareOptions.Add("📋 Copy to Clipboard");
            ShareOptions.Add("📧 Share via Email");
            ShareOptions.Add("💬 Share via SMS");
            ShareOptions.Add("📤 Share as Text");
            ShareOptions.Add("📁 Save as File");
        }

        public async Task LoadOcrResultAsync(string imagePath, string extractedText)
        {
            ImagePath = imagePath;
            ExtractedText = extractedText;
            
            // Check if this result is already saved
            var existingResults = await _databaseService.GetAllResultsAsync();
            IsSaved = existingResults.Any(r => r.ExtractedText == extractedText && r.ImagePath == imagePath);
            
            StatusMessage = IsSaved ? "✅ Already saved" : "📝 Ready to save";
        }

        [RelayCommand]
        private async Task SaveResultAsync()
        {
            if (string.IsNullOrWhiteSpace(ExtractedText))
            {
                StatusMessage = "❌ No text to save";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "💾 Saving...";

                var ocrResult = new Models.OcrResult
                {
                    ExtractedText = ExtractedText,
                    ImagePath = ImagePath,
                    CreatedAt = DateTime.Now
                };

                await _databaseService.SaveOcrResultAsync(ocrResult);
                IsSaved = true;
                StatusMessage = "✅ Saved successfully!";

                // Clear status message after 2 seconds
                await Task.Delay(2000);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Save failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShareTextAsync(string shareOption)
        {
            if (string.IsNullOrWhiteSpace(ExtractedText))
            {
                StatusMessage = "❌ No text to share";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "📤 Sharing...";

                switch (shareOption)
                {
                    case "📋 Copy to Clipboard":
                        await _shareService.CopyToClipboardAsync(ExtractedText);
                        StatusMessage = "📋 Copied to clipboard!";
                        break;
                    case "📧 Share via Email":
                        await _shareService.ShareViaEmailAsync(ExtractedText, "OCR Extracted Text");
                        StatusMessage = "📧 Email opened";
                        break;
                    case "💬 Share via SMS":
                        await _shareService.ShareViaSmsAsync(ExtractedText);
                        StatusMessage = "💬 SMS opened";
                        break;
                    case "📤 Share as Text":
                        await _shareService.ShareTextAsync(ExtractedText);
                        StatusMessage = "📤 Share dialog opened";
                        break;
                    case "📁 Save as File":
                        await _shareService.ShareAsFileAsync(ExtractedText, "extracted_text.txt");
                        StatusMessage = "📁 File shared";
                        break;
                }

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
        private async Task EditTextAsync()
        {
            // This will be handled by the view to show an editor
            StatusMessage = "✏️ Edit mode enabled";
        }

        [RelayCommand]
        private async Task NewScanAsync()
        {
            // Navigate back to main page
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task ViewHistoryAsync()
        {
            // Navigate to history page
            await Shell.Current.GoToAsync("//HistoryPage");
        }

        public void UpdateExtractedText(string newText)
        {
            ExtractedText = newText;
            IsSaved = false;
            StatusMessage = "📝 Text modified - save to keep changes";
        }
    }
}