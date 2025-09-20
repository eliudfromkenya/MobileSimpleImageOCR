using Plugin.Maui.OCR;
using SimpleImageOCR.Models;

namespace SimpleImageOCR.Services
{
    public class OcrService
    {
        private readonly IOcrService _ocrService;
        private readonly DatabaseService _databaseService;

        public OcrService(DatabaseService databaseService)
        {
            _ocrService = OcrPlugin.Default;
            _databaseService = databaseService;
        }

        public async Task<Models.OcrResult?> ExtractTextFromImageAsync(string imagePath, bool saveToDatabase = true)
        {
            try
            {
                if (!File.Exists(imagePath))
                    return null;

                using var imageStream = File.OpenRead(imagePath);
                using var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                var ocrResult = await _ocrService.RecognizeTextAsync(imageBytes);

                if (ocrResult?.AllText?.Length > 0)
                {
                    var result = new Models.OcrResult
                    {
                        ExtractedText = ocrResult.AllText,
                        ImagePath = imagePath,
                        Title = GenerateTitle(ocrResult.AllText),
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    if (saveToDatabase)
                    {
                        await _databaseService.SaveOcrResultAsync(result);
                    }

                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"OCR Error: {ex.Message}");
                return null;
            }
        }

        public async Task<Models.OcrResult?> ExtractTextFromStreamAsync(Stream imageStream, string imagePath = "", bool saveToDatabase = true)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                
                var ocrResult = await _ocrService.RecognizeTextAsync(imageBytes);

                if (ocrResult?.AllText?.Length > 0)
                {
                    var result = new Models.OcrResult
                    {
                        ExtractedText = ocrResult.AllText,
                        ImagePath = imagePath,
                        Title = GenerateTitle(ocrResult.AllText),
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    if (saveToDatabase)
                    {
                        await _databaseService.SaveOcrResultAsync(result);
                    }

                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"OCR Error: {ex.Message}");
                return null;
            }
        }

        public bool IsOcrAvailable()
        {
            try
            {
                return true; // Assume OCR is available for now
            }
            catch
            {
                return false;
            }
        }

        private string GenerateTitle(string extractedText)
        {
            if (string.IsNullOrWhiteSpace(extractedText))
                return "Untitled";

            // Take first line or first 50 characters as title
            var lines = extractedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstLine = lines.FirstOrDefault()?.Trim() ?? "";

            if (firstLine.Length > 50)
                return firstLine.Substring(0, 47) + "...";

            return string.IsNullOrWhiteSpace(firstLine) ? "Untitled" : firstLine;
        }

        public async Task<string> SaveImageToAppDataAsync(Stream imageStream, string fileName)
        {
            try
            {
                var appDataPath = FileSystem.AppDataDirectory;
                var imagesFolder = Path.Combine(appDataPath, "Images");
                
                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                var filePath = Path.Combine(imagesFolder, fileName);
                
                using var fileStream = File.Create(filePath);
                await imageStream.CopyToAsync(fileStream);
                
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save Image Error: {ex.Message}");
                return string.Empty;
            }
        }
    }
}