using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleImageOCR.Services;
using SimpleImageOCR.Models;
using SimpleImageOCR.Views;
using System.Windows.Input;

namespace SimpleImageOCR.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ICameraService _cameraService;
        private readonly OcrService _ocrService;
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private string statusMessage = "Ready to extract text from images";

        [ObservableProperty]
        private int totalSavedTexts;

        public MainPageViewModel(ICameraService cameraService, OcrService ocrService, DatabaseService databaseService)
        {
            _cameraService = cameraService;
            _ocrService = ocrService;
            _databaseService = databaseService;
            
            LoadStatistics();
        }

        [RelayCommand]
        private async Task TakePhotoAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "📷 Opening camera...";

                var imageStream = await _cameraService.CapturePhotoAsync();
                if (imageStream != null)
                {
                    StatusMessage = "📸 Photo captured! Processing...";
                    await ProcessImageAsync(imageStream, "camera");
                }
                else
                {
                    StatusMessage = "📷 Camera cancelled or unavailable";
                    await Task.Delay(2000);
                    StatusMessage = "Ready to extract text from images";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Camera error: {ex.Message}";
                await Task.Delay(3000);
                StatusMessage = "Ready to extract text from images";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task SelectImageAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "🖼️ Opening gallery...";

                var imageStream = await _cameraService.PickPhotoAsync();
                if (imageStream != null)
                {
                    StatusMessage = "🖼️ Image selected! Processing...";
                    await ProcessImageAsync(imageStream, "gallery");
                }
                else
                {
                    StatusMessage = "🖼️ Gallery cancelled or unavailable";
                    await Task.Delay(2000);
                    StatusMessage = "Ready to extract text from images";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Gallery error: {ex.Message}";
                await Task.Delay(3000);
                StatusMessage = "Ready to extract text from images";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task ViewHistoryAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//HistoryPage");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Navigation error: {ex.Message}";
            }
        }

        private async Task ProcessImageAsync(Stream imageStream, string source)
        {
            try
            {
                StatusMessage = "🔍 Extracting text...";
                
                var ocrResult = await _ocrService.ExtractTextFromStreamAsync(imageStream);
                
                if (ocrResult != null && !string.IsNullOrWhiteSpace(ocrResult.ExtractedText))
                {
                    // Save image to app data directory
                    var imagePath = await _ocrService.SaveImageToAppDataAsync(imageStream, $"{source}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                    
                    // Navigate to OCR result page with the extracted text and image path
                    var navigationParameter = new Dictionary<string, object>
                    {
                        ["ExtractedText"] = ocrResult.ExtractedText,
                        ["ImagePath"] = imagePath
                    };
                    
                    await Shell.Current.GoToAsync("//OcrResultPage", navigationParameter);
                }
                else
                {
                    StatusMessage = "❌ No text found in image";
                    await Task.Delay(3000);
                    StatusMessage = "Ready to extract text from images";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ OCR failed: {ex.Message}";
                await Task.Delay(3000);
                StatusMessage = "Ready to extract text from images";
            }
        }

        private async Task LoadStatistics()
        {
            try
            {
                TotalSavedTexts = await _databaseService.GetTotalResultsCountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Statistics Error: {ex.Message}");
            }
        }

        public async Task RefreshAsync()
        {
            await LoadStatistics();
            StatusMessage = "Ready to extract text from images";
        }
    }
}