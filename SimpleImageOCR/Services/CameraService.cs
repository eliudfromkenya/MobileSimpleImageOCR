namespace SimpleImageOCR.Services
{
    public interface ICameraService
    {
        Task<Stream?> CapturePhotoAsync();
        Task<Stream?> PickPhotoAsync();
    }

    public class CameraService : ICameraService
    {
        public async Task<Stream?> CapturePhotoAsync()
        {
            try
            {
                // Check if camera capture is supported
                if (!MediaPicker.IsCaptureSupported)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Camera Not Supported", 
                        "Camera capture is not supported on this device.", "OK");
                    return null;
                }

                // Request camera photo with options for better quality
                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a photo for OCR"
                });

                if (photo != null)
                {
                    return await photo.OpenReadAsync();
                }
                return null;
            }
            catch (FeatureNotSupportedException)
            {
                await Application.Current?.MainPage?.DisplayAlert("Feature Not Supported", 
                    "Camera is not supported on this device.", "OK");
                return null;
            }
            catch (PermissionException)
            {
                await Application.Current?.MainPage?.DisplayAlert("Permission Denied", 
                    "Camera permission is required to take photos. Please enable camera permission in settings.", "OK");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Camera Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Camera Error", 
                    $"An error occurred while accessing the camera: {ex.Message}", "OK");
                return null;
            }
        }

        public async Task<Stream?> PickPhotoAsync()
        {
            try
            {
                // Pick photo with options for better quality
                var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo for OCR"
                });

                if (photo != null)
                {
                    return await photo.OpenReadAsync();
                }
                return null;
            }
            catch (FeatureNotSupportedException)
            {
                await Application.Current?.MainPage?.DisplayAlert("Feature Not Supported", 
                    "Photo picker is not supported on this device.", "OK");
                return null;
            }
            catch (PermissionException)
            {
                await Application.Current?.MainPage?.DisplayAlert("Permission Denied", 
                    "Storage permission is required to access photos. Please enable storage permission in settings.", "OK");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Photo Picker Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Photo Picker Error", 
                    $"An error occurred while selecting a photo: {ex.Message}", "OK");
                return null;
            }
        }
    }
}