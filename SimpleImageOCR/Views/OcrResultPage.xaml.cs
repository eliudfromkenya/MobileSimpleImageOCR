using SimpleImageOCR.ViewModels;

namespace SimpleImageOCR.Views;

[QueryProperty(nameof(ExtractedText), "ExtractedText")]
[QueryProperty(nameof(ImagePath), "ImagePath")]
public partial class OcrResultPage : ContentPage
{
    public string ExtractedText { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    public OcrResultPage(OcrResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is OcrResultViewModel viewModel)
        {
            await viewModel.LoadOcrResultAsync(ImagePath, ExtractedText);
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is OcrResultViewModel viewModel && sender is Editor editor)
        {
            viewModel.UpdateExtractedText(editor.Text);
        }
    }
}