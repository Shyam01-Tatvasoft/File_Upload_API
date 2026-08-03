namespace Backend.Helpers
{
    public static class FileTypeHelper
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public static bool IsImage(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return ImageExtensions.Contains(extension);
        }
    }
}