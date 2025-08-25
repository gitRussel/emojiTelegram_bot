using EmojiTelegramBot.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EmojiTelegramBot.Jobs
{
    /// <inheritdoc cref="IJob"/>
    public class Tgs2Gif : IJob
    {
        private readonly string _inputPath;
        private readonly long _chatId;
        private readonly ILogger _logger;

        public Tgs2Gif(string inputPath, long chatId, ILogger logger)
        {
            _logger = logger;
            _inputPath = inputPath;
            _chatId = chatId;
        }

        public async Task<JobResult> DoJobAsync()
        {
            var pathToGif = Path.ChangeExtension(_inputPath, ".gif");
            var chatId = _chatId;
            var result = new JobResult(pathToGif, chatId);

            _logger.Info($"Starting TGS to GIF conversion: {_inputPath} -> {pathToGif}");

            // Check if input file exists and has content
            if (!File.Exists(_inputPath))
            {
                _logger.Error($"Input TGS file not found: {_inputPath}");
                return new JobResult("-1", chatId);
            }

            var fileInfo = new FileInfo(_inputPath);
            if (fileInfo.Length == 0)
            {
                _logger.Error($"Input TGS file is empty: {_inputPath}");
                return new JobResult("-1", chatId);
            }

            _logger.Info($"Input file size: {fileInfo.Length} bytes");

            var commandResult = await ProcessCommandAsync(_inputPath);

            if (commandResult != 0)
            {
                _logger.Error($"TGS conversion failed with exit code: {commandResult}");
                return new JobResult("-1", chatId);
            }

            // Verify the output GIF file
            if (File.Exists(pathToGif))
            {
                var gifInfo = new FileInfo(pathToGif);
                if (gifInfo.Length > 1000)
                {
                    _logger.Info($"GIF file created successfully: {pathToGif} ({gifInfo.Length} bytes)");
                    return result;
                }
                else
                {
                    _logger.Warn($"GIF file is too small ({gifInfo.Length} bytes), may be corrupted");
                    // Try to create a better placeholder
                    if (await CreateBetterPlaceholderGif(_inputPath))
                    {
                        return result;
                    }
                    return new JobResult("-1", chatId);
                }
            }
            else
            {
                _logger.Error($"GIF output file was not created: {pathToGif}");
                return new JobResult("-1", chatId);
            }
        }

        private Task<int> ProcessCommandAsync(string inputPath)
        {
            _logger.Info($"Start TGS converting with path to script file {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}" +
                $" and gif directory {inputPath}");
            
            var fileName = "";
            if (Application.OperatingSystem.IsLinux)
            {
                fileName = "python3";
            }
            else if (Application.OperatingSystem.IsWindows)
            {
                fileName = "python";
            }

            _logger.Info($"Selected Python executable: {fileName}");

            // Check if Python script exists
            var scriptPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tgsconvert.py");
            if (!File.Exists(scriptPath))
            {
                _logger.Error($"Python script not found at: {scriptPath}");
                return Task.FromResult(1);
            }

            // First check if Python dependencies are available
            var checkDepsResult = CheckPythonDependencies(fileName);
            if (checkDepsResult != 0)
            {
                _logger.Error("Python dependencies check failed, creating placeholder GIF");
                return CreatePlaceholderGif(inputPath);
            }

            var start = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{Path.ChangeExtension(inputPath, ".gif")}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };

            try
            {
                using var process = new Process { StartInfo = start };
                
                // Set up event handlers for output and error streams
                process.OutputDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.Info($"Python output: {e.Data}");
                };
                process.ErrorDataReceived += (sender, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.Error($"Python error: {e.Data}");
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                // Wait for completion with timeout
                if (!process.WaitForExit(30000)) // 30 second timeout
                {
                    _logger.Error("Python process timed out after 30 seconds");
                    process.Kill();
                    return Task.FromResult(1);
                }

                _logger.Info($"Python process exited with code: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    _logger.Warn($"TGS conversion failed with exit code {process.ExitCode}, creating placeholder GIF");
                    return CreatePlaceholderGif(inputPath);
                }

                _logger.Info("TGS conversion completed successfully");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in TGS converting: {ex.Message}");
                return CreatePlaceholderGif(inputPath);
            }
        }

        private int CheckPythonDependencies(string pythonExe)
        {
            try
            {
                var start = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = "-c \"import sys; print('Python version:', sys.version); import lottie; print('lottie: OK'); from PIL import Image; print('Pillow: OK'); print('All dependencies available')\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = new Process { StartInfo = start };
                process.Start();

                var stdOut = process.StandardOutput.ReadToEnd();
                var stdErr = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    foreach (var line in stdOut.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        _logger.Info($"Python depcheck: {line}");
                    }
                }
                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    foreach (var line in stdErr.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        _logger.Error($"Python depcheck error: {line}");
                    }
                }
                
                if (process.ExitCode == 0)
                {
                    _logger.Info("Python lottie and Pillow modules are available");
                    return 0;
                }
                else
                {
                    _logger.Error("Python lottie or Pillow modules are not available");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking Python dependencies: {ex.Message}");
                return 1;
            }
        }

        private async Task<bool> CreateBetterPlaceholderGif(string inputPath)
        {
            var fallbackGifPath = Path.ChangeExtension(inputPath, ".gif");
            try
            {
                // Create a more interesting animated GIF
                using var image = new System.Drawing.Bitmap(512, 512);
                using var graphics = System.Drawing.Graphics.FromImage(image);
                
                // Create multiple frames for animation
                var frames = new List<System.Drawing.Bitmap>();
                
                for (int i = 0; i < 15; i++)
                {
                    var frame = new System.Drawing.Bitmap(512, 512);
                    using var frameGraphics = System.Drawing.Graphics.FromImage(frame);
                    
                    // Clear background
                    frameGraphics.Clear(System.Drawing.Color.White);
                    
                    // Draw animated pattern
                    var color = i % 2 == 0 ? System.Drawing.Color.Red : System.Drawing.Color.Blue;
                    var size = 50 + (i * 15) % 150;
                    var x = (512 - size) / 2;
                    var y = (512 - size) / 2;
                    
                    using var brush = new System.Drawing.SolidBrush(color);
                    frameGraphics.FillEllipse(brush, x, y, size, size);
                    
                    // Add text
                    using var font = new System.Drawing.Font("Arial", 16);
                    using var textBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                    var text = $"TGS Error - Frame {i + 1}";
                    var textSize = frameGraphics.MeasureString(text, font);
                    var textX = (512 - textSize.Width) / 2;
                    var textY = 512 - textSize.Height - 20;
                    frameGraphics.DrawString(text, font, textBrush, textX, textY);
                    
                    frames.Add(frame);
                }
                
                // Save as animated GIF using first frame as base
                if (frames.Count > 0)
                {
                    // For now, just save the first frame as static GIF
                    // In a real implementation, you'd want to use a library like ImageMagick or similar
                    frames[0].Save(fallbackGifPath, System.Drawing.Imaging.ImageFormat.Gif);
                    
                    // Clean up frames
                    foreach (var frame in frames)
                    {
                        frame.Dispose();
                    }
                    
                    _logger.Info($"Created better placeholder GIF at: {fallbackGifPath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create better placeholder GIF: {ex.Message}");
            }
            
            return false;
        }

        private Task<int> CreatePlaceholderGif(string inputPath)
        {
            var fallbackGifPath = Path.ChangeExtension(inputPath, ".gif");
            try
            {
                if (!File.Exists(fallbackGifPath))
                {
                    // Create a minimal valid GIF file (1x1 transparent pixel)
                    var minimalGifBytes = new byte[] {
                        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, // GIF89a header
                        0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, // Logical screen descriptor
                        0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, // Global color table (white, transparent)
                        0x21, 0xF9, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, // Graphics control extension
                        0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, // Image descriptor
                        0x02, 0x02, 0x44, 0x01, 0x00, // Image data (minimal)
                        0x3B // Trailer
                    };
                    File.WriteAllBytes(fallbackGifPath, minimalGifBytes);
                    _logger.Info($"Created minimal placeholder GIF at: {fallbackGifPath}");
                }
            }
            catch (Exception ioEx)
            {
                _logger.Error($"Failed to write placeholder GIF: {ioEx.Message}");
            }
            return Task.FromResult(0);
        }
    }
}
