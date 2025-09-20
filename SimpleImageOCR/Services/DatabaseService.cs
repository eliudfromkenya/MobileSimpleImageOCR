using SQLite;
using SimpleImageOCR.Models;

namespace SimpleImageOCR.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public async Task InitializeAsync()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "OcrResults.db3");
            _database = new SQLiteAsyncConnection(databasePath);
            
            await _database.CreateTableAsync<OcrResult>();
        }

        public async Task<List<OcrResult>> GetAllResultsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<OcrResult>()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<OcrResult>> GetFavoriteResultsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<OcrResult>()
                .Where(x => x.IsFavorite)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task ClearAllOcrResultsAsync()
        {
            await InitializeAsync();
            await _database!.DeleteAllAsync<OcrResult>();
        }

        public async Task<OcrResult> GetResultAsync(int id)
        {
            await InitializeAsync();
            return await _database!.Table<OcrResult>()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> DeleteOcrResultAsync(OcrResult result)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(result);
        }

        public async Task<int> DeleteOcrResultAsync(int id)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync<OcrResult>(id);
        }

        public async Task<int> SaveOcrResultAsync(OcrResult result)
        {
            await InitializeAsync();
            
            if (result.Id != 0)
            {
                result.UpdatedAt = DateTime.Now;
                return await _database!.UpdateAsync(result);
            }
            else
            {
                result.CreatedAt = DateTime.Now;
                result.UpdatedAt = DateTime.Now;
                return await _database!.InsertAsync(result);
            }
        }

        public async Task<int> ToggleFavoriteAsync(int id)
        {
            await InitializeAsync();
            var result = await GetResultAsync(id);
            if (result != null)
            {
                result.IsFavorite = !result.IsFavorite;
                result.UpdatedAt = DateTime.Now;
                return await _database!.UpdateAsync(result);
            }
            return 0;
        }

        public async Task<List<OcrResult>> GetResultsByIdsAsync(List<int> ids)
        {
            await InitializeAsync();
            var results = new List<OcrResult>();
            
            foreach (var id in ids)
            {
                var result = await GetResultAsync(id);
                if (result != null)
                    results.Add(result);
            }
            
            return results;
        }

        public async Task<int> GetTotalResultsCountAsync()
        {
            await InitializeAsync();
            return await _database!.Table<OcrResult>().CountAsync();
        }
    }
}