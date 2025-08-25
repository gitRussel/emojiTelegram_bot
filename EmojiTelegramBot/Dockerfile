#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Install system dependencies
RUN apt-get update -y && apt-get install -y \
    build-essential \
    python3 \
    python3-dev \
    python3-pip \
    python3-setuptools \
    python3-wheel \
    python3-cffi \
    libcairo2 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libgdk-pixbuf2.0-0 \
    libffi-dev \
    shared-mime-info \
    fonts-noto-color-emoji \
    && rm -rf /var/lib/apt/lists/*

# Upgrade pip and install Python packages
RUN python3 -m pip install --upgrade pip setuptools wheel
RUN python3 -m pip install --no-cache-dir lottie Pillow cairosvg

# Verify Python packages installation
RUN python3 -c "import lottie; print('lottie version:', lottie.__version__)" || echo "lottie import failed"
RUN python3 -c "from PIL import Image; print('Pillow version:', Image.__version__)" || echo "Pillow import failed"

# Install .NET dependencies
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so
RUN apt-get update -y && apt-get install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so
RUN fc-cache -fv

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EmojiTelegramBot/EmojiTelegramBot.csproj", "EmojiTelegramBot/"]
RUN dotnet restore "EmojiTelegramBot/EmojiTelegramBot.csproj"
COPY . .
WORKDIR "/src/EmojiTelegramBot"
RUN dotnet build "EmojiTelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmojiTelegramBot.csproj" -c Release -o /app/publish

VOLUME [ "/Gifs" ]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Verify Python environment in final stage
RUN python3 -m pip list | grep -E "(lottie|Pillow)"
RUN python3 -c "import lottie; print('lottie available in final stage')" || echo "lottie not available in final stage"
RUN python3 -c "from PIL import Image; print('Pillow available in final stage')" || echo "Pillow not available in final stage"

# Test TGS conversion script syntax
RUN python3 -m py_compile tgsconvert.py && echo "TGS conversion script syntax OK" || echo "TGS conversion script syntax error"

ENTRYPOINT ["dotnet", "EmojiTelegramBot.dll"]