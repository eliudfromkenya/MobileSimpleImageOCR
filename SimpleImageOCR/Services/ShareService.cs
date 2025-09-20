using SimpleImageOCR.Models;

namespace SimpleImageOCR.Services
{
    public interface IShareService
    {
        Task ShareTextAsync(string text, string title = "Shared Text");
        Task ShareMultipleTextsAsync(List<OcrResult> results, string separator = "\n\n---\n\n");
        Task ShareAsFileAsync(string text, string fileName = "extracted_text.txt");
        Task CopyToClipboardAsync(string text);
        Task ShareViaEmailAsync(string text, string subject = "OCR Extracted Text");
        Task ShareViaSmsAsync(string text);
        string FormatTextForSharing(List<OcrResult> results, bool includeTimestamps = false, bool includeHeaders = true);
    }

    public class ShareService : IShareService
    {
        public async Task ShareTextAsync(string text, string title = "Shared Text")
        {
            try
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = text,
                    Title = title
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Share Error: {ex.Message}");
            }
        }

        public async Task ShareMultipleTextsAsync(List<OcrResult> results, string separator = "\n\n---\n\n")
        {
            try
            {
                if (results == null || !results.Any())
                    return;

                var combinedText = string.Join(separator, results.Select(r => r.ExtractedText));
                var title = results.Count == 1 ? results[0].Title : $"Combined Text ({results.Count} items)";

                await ShareTextAsync(combinedText, title);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Share Multiple Error: {ex.Message}");
            }
        }

        public async Task ShareAsFileAsync(string text, string fileName = "extracted_text.txt")
        {
            try
            {
                var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(tempPath, text);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Text File",
                    File = new ShareFile(tempPath)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Share File Error: {ex.Message}");
            }
        }

        public async Task CopyToClipboardAsync(string text)
        {
            try
            {
                await Clipboard.SetTextAsync(text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clipboard Error: {ex.Message}");
            }
        }

        public async Task ShareViaEmailAsync(string text, string subject = "OCR Extracted Text")
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = text
                };

                await Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
            }
        }

        public async Task ShareViaSmsAsync(string text)
        {
            try
            {
                var message = new SmsMessage
                {
                    Body = text
                };
                await Sms.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMS Error: {ex.Message}");
            }
        }

        public string FormatTextForSharing(List<OcrResult> results, bool includeTimestamps = false, bool includeHeaders = true)
        {
            if (results == null || !results.Any())
                return string.Empty;

            var formattedTexts = new List<string>();

            foreach (var result in results)
            {
                var text = result.ExtractedText;
                
                if (includeHeaders && !string.IsNullOrWhiteSpace(result.Title))
                {
                    text = $"Title: {result.Title}\n{text}";
                }

                if (includeTimestamps)
                {
                    text = $"{text}\n\nExtracted on: {result.FormattedDate}";
                }

                formattedTexts.Add(text);
            }

            return string.Join("\n\n" + new string('=', 50) + "\n\n", formattedTexts);
        }
    }
}