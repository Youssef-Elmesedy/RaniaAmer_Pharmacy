# 🥩 أولاد زمزم | Awlad Zamzam

نظام إدارة متجر إلكتروني (E-Commerce) متكامل مبني بـ **ASP.NET Core MVC**، مصمم لمتجر لحوم/بقالة يحتاج لواجهة عرض للعملاء بجانب لوحة تحكم كاملة للإدارة — تشمل إدارة المنتجات، الفئات، العروض، الطلبات، العملاء، والمدفوعات (بما فيها نظام البيع الآجل/الدين).

---

## ✨ نظرة عامة

المشروع مقسّم إلى واجهتين رئيسيتين:

| الواجهة | الوصف |
|---|---|
| 🛍️ **واجهة المتجر (Storefront)** | تصفح المنتجات والفئات والعروض، سلة تسوق، إتمام الطلب (Checkout)، حساب عميل بتسجيل دخول/تسجيل مستقل، متابعة الطلبات وفواتير الآجل |
| 🛠️ **لوحة التحكم (Admin Panel)** | Dashboard إحصائي، إدارة المنتجات والفئات والعروض، إدارة الطلبات والمدفوعات، إدارة العملاء ومديونياتهم، إشعارات النظام |

---

## 🧩 المزايا الرئيسية

### للعملاء
- تصفح المنتجات حسب الفئة، مع بحث وعروض خاصة
- سلة تسوق تفاعلية وصفحة إتمام طلب (Checkout)
- إنشاء حساب عميل مستقل عن حساب الإدارة (نظام مصادقة منفصل)
- استرجاع كلمة المرور عبر **سؤال أمان** مخصص (بدون الحاجة لبريد إلكتروني)
- ملف شخصي: تعديل البيانات، تغيير كلمة المرور، تغيير سؤال الأمان
- متابعة الطلبات وفواتير البيع الآجل الخاصة بالعميل

### للإدارة
- تسجيل دخول منفصل بنظام صلاحيات (ASP.NET Core Identity)
- Dashboard بإحصائيات سريعة عن الطلبات والمبيعات والمديونيات
- إدارة كاملة (CRUD) للمنتجات، الفئات، والعروض مع رفع صور
- إدارة الطلبات وتتبّع حالتها
- نظام بيع بالآجل (Credit Orders) مع تسجيل الدفعات ومتابعة المستحقات
- إدارة قائمة العملاء وعرض سجل مديونياتهم
- إشعارات داخلية للأحداث الهامة

---

## 🏗️ البنية التقنية (Architecture)

المشروع مبني بمعمارية طبقات واضحة (Layered Architecture) لفصل الاهتمامات:

```
Controllers / Areas            → طبقة العرض (MVC Controllers) — عامة + منطقة Admin منفصلة
Services (Interfaces/Impl)     → منطق العمل (Business Logic)
Repository (Interfaces/Impl)   → طبقة الوصول للبيانات (Data Access via EF Core)
Models
 ├── Entities                  → كيانات الدومين (Category, Product, Order, Customer, Offer...)
 ├── ViewModels                → نماذج العرض الخاصة بكل صفحة
 └── Exceptions                → استثناءات مخصصة لمنطق العمل (BusinessException)
Data
 ├── ApplicationDbContext      → سياق قاعدة البيانات (EF Core)
 └── Configurations            → إعدادات الـ Fluent API لكل كيان
Migrations                     → EF Core Migrations
wwwroot                        → الملفات الثابتة (CSS / JS / صور المنتجات المرفوعة)
```

### أبرز القرارات التقنية
- **مصادقة مزدوجة (Dual Authentication):** نظامان منفصلان للكوكيز — واحد للإدارة عبر ASP.NET Core Identity، وآخر مستقل للعملاء (`CustomerAccountController.SchemeName`) — لفصل صلاحيات الطرفين تمامًا.
- **استرجاع كلمة المرور بدون بريد إلكتروني:** يعتمد على سؤال أمان يختاره العميل عند التسجيل، مناسب لقاعدة عملاء لا تعتمد بالضرورة على البريد الإلكتروني.
- **Repository + Service Pattern:** كل كيان له Repository للوصول للبيانات، وService لمنطق العمل، ما يسهّل الاختبار والصيانة.
- **تتبع تغييرات الكتالوج:** `ICatalogChangeTracker` (Singleton) لإدارة تحديثات الكتالوج بكفاءة.
- **حماية شاملة:** CSRF (Anti-forgery Tokens بهيدر مخصص)، حماية من الـ bfcache للصفحات الحساسة، وسياسات كوكيز صارمة (`HttpOnly`, `Secure`, `SameSite=Lax`).

---

## 🛠️ التقنيات المستخدمة (Tech Stack)

| الفئة | التقنية |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core 8 (Code-First + Migrations) |
| قاعدة البيانات | Microsoft SQL Server |
| المصادقة | ASP.NET Core Identity (للإدارة) + Cookie Auth مخصص (للعملاء) |
| الواجهة الأمامية | Bootstrap (RTL) + CSS مخصص + JavaScript / jQuery |
| التنبيهات | SweetAlert2 |
| اللغة | العربية بالكامل (RTL) |

---

## 📁 الكيانات الأساسية (Domain Entities)

- `Category` — فئات المنتجات
- `Product` — المنتجات
- `Offer` / `OfferItem` — العروض الترويجية ومنتجاتها
- `Customer` — بيانات العميل (بيانات الحساب، كلمة المرور، سؤال الأمان)
- `Order` / `OrderItem` / `OrderPayment` — الطلبات وعناصرها ودفعاتها (يدعم البيع الآجل)
- `ApplicationUser` — مستخدم لوحة التحكم (Identity)

---

## 🚀 التشغيل محليًا (Getting Started)

### المتطلبات
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Local / Express / أي نسخة متاحة)
- (اختياري) [dotnet-ef tool](https://learn.microsoft.com/ef/core/cli/dotnet) لتشغيل الـ Migrations يدويًا

### خطوات التشغيل

1. **استنساخ المشروع**
   ```bash
   git clone <repository-url>
   cd Awlad_Zamzam.MVC
   ```

2. **إعداد سلسلة الاتصال بقاعدة البيانات**

   عدّل `appsettings.json` (أو أفضل: استخدم [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) في بيئة التطوير) وضع بيانات السيرفر الخاص بك:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=AwladZamzamDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **تشغيل المشروع**
   ```bash
   dotnet restore
   dotnet run
   ```

   > قاعدة البيانات يتم إنشاؤها وتحديثها تلقائيًا عند التشغيل (`context.Database.MigrateAsync()`)، بالإضافة إلى بيانات أولية (Seed Data) يتم إدراجها تلقائيًا.

4. **(اختياري) إدارة الـ Migrations يدويًا**
   ```bash
   dotnet tool restore
   dotnet ef migrations add MigrationName
   dotnet ef database update
   ```

5. افتح المتصفح على الرابط الذي يظهر في الطرفية (عادة `https://localhost:xxxx`).

---

## 🔐 ملاحظات أمنية

- **لا يتم رفع** `appsettings.Development.json` أو أي ملف يحتوي على بيانات اتصال حقيقية أو أسرار — راجع ملف `.gitignore`.
- مفاتيح Data Protection يتم تخزينها محليًا في `DataProtection-Keys/` (مستبعد من Git) — يجب استخدام تخزين مركزي (مثل Redis أو Azure Key Vault) عند النشر على أكثر من سيرفر.
- كلمات المرور وإجابات أسئلة الأمان مُخزّنة بصيغة Hash فقط (`PasswordHasher`)، ولا يتم تخزين أي قيم صريحة.

---

## 📌 حالة المشروع

المشروع في مرحلة **تطوير نشط (Active Development)** — يتم العمل حاليًا على تحسينات في تجربة المستخدم على الأجهزة المحمولة (Responsive Design) وتوسيع صلاحيات إدارة الحساب الشخصي للعملاء.

---

## 📄 الترخيص

هذا المشروع خاص (Private) — جميع الحقوق محفوظة.
