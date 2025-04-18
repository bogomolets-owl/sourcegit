using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

#if __WINDOWS__
using System.Drawing; 
#endif

#if __MACOS__
using AppKit; 
#endif

#if __LINUX__
using GLib; 
#endif

namespace SourceGit.Converters
{
    public class FileIconConverter : IValueConverter
    {
        private readonly Dictionary<string, IImage> _cache = new ();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath)
            {
                var extension = Path.GetExtension(filePath);
                if (!string.IsNullOrEmpty(extension))
                {
                    if (_cache.TryGetValue(extension, out var cachedIcon))
                    {
                        return cachedIcon;
                    }
                    
                    var icon = FileIconHelper.GetFileIcon(filePath);
                    if (icon != null)
                    {
                        _cache[extension] = icon;
                        return icon;
                    }
                }
            }
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class FileIconHelper
    {
        public static IImage GetFileIcon(string filePath)
        {
#if __WINDOWS__
            if (OperatingSystem.IsWindows())
            {
                var icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    using (var bitmap = icon.ToBitmap())
                    {
                        var stream = new MemoryStream();
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Seek(0, SeekOrigin.Begin);
                        return new Bitmap(stream);
                    }
                }
            }
#endif

#if __MACOS__
            if (OperatingSystem.IsMacOS())
            {
                // macOS-specific code to get file icon
                var nsWorkspace = AppKit.NSWorkspace.SharedWorkspace;
                var nsImage = nsWorkspace.IconForFile(filePath);
                var tiffData = nsImage.AsTiff();
                var stream = tiffData.AsStream();
                return new Bitmap(stream);
            }
#endif
            
#if __LINUX__
            if (OperatingSystem.IsLinux())
            {
                // Linux-specific code to get file icon
                var iconTheme = new GLib.IconTheme();
                var iconInfo = iconTheme.LookupIcon(filePath, 48, 0);
                if (iconInfo != null)
                {
                    var iconPath = iconInfo.GetFilename();
                    return new Bitmap(iconPath);
                }
            }
#endif
            return null;
        }
    }
}
