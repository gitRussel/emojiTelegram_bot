#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

RUN apt-get update -y && apt-get install build-essential python3-dev python3-pip python3-setuptools python3-wheel python3-cffi libcairo2 libpango-1.0-0 libpangocairo-1.0-0 libgdk-pixbuf2.0-0 libffi-dev shared-mime-info -y
RUN pip3 install --upgrade pip
RUN pip3 install tgs 
RUN pip3 install cairosvg
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so
RUN apt-get install -y libgdiplus 
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so
RUN apt-get install -y fonts-noto-color-emoji
RUN fc-cache -fv

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
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
ENTRYPOINT ["dotnet", "EmojiTelegramBot.dll"]