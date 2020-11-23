# emojiTelegram_bot
Сервис, который конвертирует анимированные и статические стикеры, смайлы в gif изображения.
***
.NET Core 3.0, Docker, собственная очередь обработки.
## Регистрация бота в Telegram ##
 Переходим на [официальную страницу телеграмма] (https://web.telegram.org/) регистрируемся или входим в свой аккаунт. Ищем **BotFather** далее создаём нового бота:
 1. `/start`, появляется список доступных команд;
 2. `/newbot`, **BotFather** запросит имя нового бота;
 3. Он выдаст токен, который нам вскоре пригодится.
## Конфигурация проекта ##
В корне проекта находится файл **App.config**, его структура представлена ниже: 
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="ParallelCount" value="10"/>
    <add key="PathToGifDirectory" value="/Gifs"/>
    <add key="ProxyHostName" value="96.113.166.133"/>
    <add key="ProxyPort" value="1080"/>
    <add key="ApiBotToken" value="PUT_YOUR_BOT_TOKEN_HERE"/>
  </appSettings>
</configuration>
```
+ Где ParallelCount — количество потоков в очереди обработки;
+ PathToGifDirectory — директория куда будут сохранятся, кэшироваться gif-анимации;
+ ProxyHostName — ip-адрес прокси-сервера для обхода блокировки в России;
+ ProxyPort — порт этого прокси-сервера;
+ ApiBotToken — токен, который нам выдал BotFather.
