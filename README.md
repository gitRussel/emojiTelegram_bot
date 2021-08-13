# emojiTelegram_bot
Сервис, который конвертирует анимированные и статические стикеры, смайлы в gif изображения.
***
.NET Core 3.0, Docker, собственная очередь обработки.
## Регистрация бота в Telegram ##
 Переходим на [официальную страницу телеграмма](https://web.telegram.org/) регистрируемся или входим в свой аккаунт. Ищем **BotFather** далее создаём нового бота:
 1. `/start`, появляется список доступных команд;
 2. `/newbot`, **BotFather** запросит имя нового бота;
 3. Он выдаст токен, который нам вскоре пригодится.
## Конфигурация проекта ##
В корне проекта находится файл **secrets.json** (подробнее в [документации](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows)), его структура представлена ниже: 

![secrets_config](https://user-images.githubusercontent.com/28735314/129339147-8ef4e08b-b4fb-4819-8a64-757c9ba83285.jpg)

+ Где ParallelCount — количество потоков в очереди обработки;
+ PathToGifDirectory — директория куда будут сохранятся, кэшироваться gif-анимации;
+ ProxyHostName — ip-адрес прокси-сервера ([список подходящих](https://spys.one/socks/)) для обхода блокировки в России;
+ ProxyPort — порт этого прокси-сервера;
+ ApiBotToken — токен, который нам выдал BotFather.
