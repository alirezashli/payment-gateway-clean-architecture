# Sample Payment Gateway

نسخه‌ای مینیمال از تمرین درگاه پرداخت، با دو سرویس مستقل و ساختار Clean Architecture مبتنی بر الگوی Core/Infrastructure/Web.

## معماری

هر سرویس سه پروژه دارد و وابستگی‌ها رو به داخل هستند:

`Web -> Infrastructure -> Core`

- **Core:** مدل‌های دامنه، قراردادها و سرویس‌های اصلی
- **Infrastructure:** پیاده‌سازی repository و ارتباطات خارجی
- **Web:** Controllerها و composition root برنامه
- **Payment.Worker:** پردازش مستقل انقضای پرداخت‌ها؛ خارج از پردازش Web اجرا می‌شود
- **Notification.Worker:** مصرف رویدادهای پرداخت از RabbitMQ با manual acknowledgement

- **Payment Service (5001):** ساخت توکن، Verify، نگهداری تراکنش و انقضای خودکار
- **Gateway Service (5002):** شبیه‌سازی پرداخت با احتمال موفقیت ۸۰٪ و ارتباط HTTP با Payment
- Repository و Event Publisher به‌صورت interface در Core تعریف شده‌اند.
- Repository فعلاً In-Memory است؛ بنابراین با restart سرویس Payment داده‌ها پاک می‌شوند.
- رویداد `PaymentProcessedEvent` به exchange بادوام `payments` در RabbitMQ منتشر می‌شود.
- صف بادوام `notifications.payment-processed` توسط `Notification.Worker` با manual ack مصرف می‌شود.
- انقضا با `BackgroundService` هر ۳۰ ثانیه بررسی می‌شود.

Notification Service اختیاریِ صورت سؤال در این نسخه‌ی ساده پیاده‌سازی نشده است.

## اجرا

پیش‌نیاز اجرا، نصب بودن `.NET 8 SDK` و Docker Desktop است.

### اجرای کامل با Docker Compose (پیشنهادی)

برای build و اجرای RabbitMQ و همهٔ سرویس‌ها از ریشهٔ پروژه اجرا کنید:

```powershell
docker compose up -d --build
docker compose ps
```

سرویس‌های زیر اجرا می‌شوند:

- `payment-web` روی `http://localhost:5001`
- `gateway-web` روی `http://localhost:5002`
- `payment-worker`
- `notification-worker`
- RabbitMQ روی `localhost:5672`
- RabbitMQ Management روی `http://localhost:15672`

برای مشاهدهٔ لاگ همهٔ سرویس‌ها:

```powershell
docker compose logs -f
```

برای مشاهدهٔ لاگ یک سرویس مشخص:

```powershell
docker compose logs -f notification-worker
```

برای توقف کامل stack:

```powershell
docker compose down
```

داده‌های RabbitMQ در volume باقی می‌مانند. برای حذف containerها همراه با داده‌های RabbitMQ:

```powershell
docker compose down -v
```

### اجرای محلی پروژه‌ها

#### راه‌اندازی RabbitMQ

از ریشهٔ پروژه اجرا کنید:

```powershell
docker compose up -d rabbitmq
docker compose ps
```

RabbitMQ روی `localhost:5672` در دسترس است. پنل مدیریت نیز در آدرس زیر باز می‌شود:

`http://localhost:15672`

نام کاربری و رمز عبور محیط توسعه هر دو `guest` هستند. برای توقف RabbitMQ:

```powershell
docker compose down
```

Volume با نام `rabbitmq-data` پیام‌ها را میان restartها نگه می‌دارد؛ `docker compose down -v` آن را حذف می‌کند.

#### Build

از ریشهٔ پروژه solution را build کنید:

```powershell
dotnet restore PaymentGateway.sln
dotnet build PaymentGateway.sln
```

سپس چهار پردازش را به‌ترتیب و در چهار ترمینال جدا اجرا کنید.

ترمینال اول — Payment API روی پورت `5001`:

```powershell
dotnet run --project src/Payment.Web
```

ترمینال دوم — Gateway API روی پورت `5002`:

```powershell
dotnet run --project src/Gateway.Web
```

ترمینال سوم — Worker مستقل انقضای پرداخت‌ها:

```powershell
dotnet run --project src/Payment.Worker
```

ترمینال چهارم — Consumer اعلان‌های RabbitMQ:

```powershell
dotnet run --project src/Notification.Worker
```

برای بررسی آماده‌بودن سرویس‌ها می‌توانید آدرس‌های زیر را باز کنید:

- `http://localhost:5001/health`
- `http://localhost:5002/health`

برای توقف هر پردازش در ترمینال مربوطه `Ctrl+C` را بزنید.

## نمونه استفاده

```powershell
$body = @{
  terminalNo = "123"
  amount = 10000
  redirectUrl = "https://example.com/callback"
  reservationNumber = "RES-1"
  phoneNumber = "09123456789"
} | ConvertTo-Json

$tokenResponse = Invoke-RestMethod -Method Post -Uri http://localhost:5001/api/payment/get-token -ContentType application/json -Body $body
Invoke-RestMethod -Uri $tokenResponse.gatewayUrl

$verify = @{ token = $tokenResponse.token; appCode = "my-app" } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri http://localhost:5001/api/payment/verify -ContentType application/json -Body $verify
```

## تصمیم ساده‌سازی

برای اینکه پروژه بدون Docker، SQL Server و RabbitMQ فوراً قابل بررسی باشد، adapterهای In-Memory استفاده شده‌اند. در نسخه production باید EF Core، RabbitMQ (با Outbox/Retry)، authentication برای endpoint داخلی، idempotency، health checks واقعی و تست‌های integration اضافه شوند.
