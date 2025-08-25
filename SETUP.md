# EmojiTelegramBot Setup Guide

## Overview
This bot converts various emoji formats (TGS animated stickers, Unicode emojis, and WebP static stickers) to GIF format using .NET 6.0 and Python integration.

## Prerequisites

### 1. .NET 6.0 SDK
- Install .NET 6.0 SDK from [Microsoft](https://dotnet.microsoft.com/download/dotnet/6.0)

### 2. Python 3.7+
- Install Python 3.7 or higher
- Ensure `python` or `python3` command is available in PATH

### 3. Python Dependencies
Install required Python packages:
```bash
pip install lottie Pillow cairosvg
```

### 4. Telegram Bot Token
- Create a bot via [@BotFather](https://t.me/botfather)
- Get your bot token

## Configuration

### Option 1: User Secrets (Recommended for Development)
```bash
# Navigate to project directory
cd EmojiTelegramBot

# Set user secrets
dotnet user-secrets set "DevConfig:ApiBotToken" "YOUR_BOT_TOKEN_HERE"
dotnet user-secrets set "DevConfig:ParallelCount" "2"
dotnet user-secrets set "DevConfig:PathToGifDirectory" "Gifs"
```

### Option 2: Environment Variables
```bash
export DevConfig__ApiBotToken="YOUR_BOT_TOKEN_HERE"
export DevConfig__ParallelCount="2"
export DevConfig__PathToGifDirectory="Gifs"
```

### Option 3: secrets.json (Not recommended for production)
Create `secrets.json` in project root:
```json
{
  "DevConfig": {
    "ApiBotToken": "YOUR_BOT_TOKEN_HERE",
    "ParallelCount": 2,
    "PathToGifDirectory": "Gifs"
  }
}
```

## Building and Running

### Local Development
```bash
# Build the project
dotnet build

# Run the bot
dotnet run
```

### Docker
```bash
# Build and run with Docker Compose
docker-compose up --build

# Or build manually
docker build -t emojitelegrambot .
docker run -v $(pwd)/Gifs:/Gifs emojitelegrambot
```

## Testing

### Test TGS Conversion
```bash
cd EmojiTelegramBot
python test_tgs_conversion.py
```

### Test Individual Components
```bash
# Test Python script directly
python tgsconvert.py input.tgs output.gif

# Test .NET build
dotnet build
dotnet test
```

## Troubleshooting

### Common Issues

1. **Python dependencies not found**
   ```bash
   pip install lottie Pillow cairosvg
   ```

2. **TGS conversion fails**
   - Check Python script syntax: `python -m py_compile tgsconvert.py`
   - Verify lottie package: `python -c "import lottie; print(lottie.__version__)"`

3. **WebP conversion fails**
   - Ensure System.Drawing.Common is properly installed
   - Check if WebP files are valid

4. **Unicode emoji not displaying**
   - Install emoji fonts (Noto Color Emoji, Segoe UI Emoji)
   - Check font availability in your system

### Logs
The bot uses Serilog for logging. Check console output for detailed information about:
- File conversions
- Python script execution
- Error details
- Performance metrics

## File Structure
```
EmojiTelegramBot/
├── Jobs/                    # Conversion jobs
│   ├── Tgs2Gif.cs         # TGS to GIF conversion
│   ├── Webp2Gif.cs        # WebP to GIF conversion
│   └── UnicodeEmoji2Gif.cs # Unicode emoji to GIF
├── tgsconvert.py           # Python TGS conversion script
├── test_tgs_conversion.py  # Python conversion test
└── Dockerfile              # Docker configuration
```

## Performance Tuning

- **ParallelCount**: Adjust based on your system's CPU cores
- **GIF Directory**: Use SSD storage for better I/O performance
- **Cache**: The bot caches converted GIFs to avoid re-conversion

## Security Notes

- Never commit bot tokens to source control
- Use user secrets or environment variables
- Consider using Azure Key Vault or similar for production
- Validate all input files before processing
