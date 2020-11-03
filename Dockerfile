FROM mcr.microsoft.com/dotnet/core/runtime:3.1
COPY EmojiTelegramBot/bin/Release/netcoreapp3.1/publish/ EmojiTelegramBot/

RUN apt-get update -y && apt-get install build-essential python3-dev python3-pip python3-setuptools python3-wheel python3-cffi libcairo2 libpango-1.0-0 libpangocairo-1.0-0 libgdk-pixbuf2.0-0 libffi-dev shared-mime-info -y
RUN pip3 install --upgrade pip
RUN pip3 install tgs 
RUN pip3 install cairosvg
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so
RUN apt-get install -y libgdiplus 
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so
RUN apt-get install -y fonts-noto-color-emoji
RUN fc-cache -fv

VOLUME [ "/Gifs" ]

ENTRYPOINT ["dotnet", "EmojiTelegramBot/EmojiTelegramBot.dll"]