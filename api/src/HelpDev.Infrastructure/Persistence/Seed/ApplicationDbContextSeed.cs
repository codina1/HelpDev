using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Infrastructure.Persistence.Seed;

public static class ApplicationDbContextSeed
{
    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid Writer1Id = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid Writer2Id = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid PrimaryAdminId = Guid.Parse("11111111-1111-1111-1111-111111111104");

    public const string PrimaryAdminMobile = "09904442841";

    public static async Task SeedAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        var seedTime = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

        var users = new[]
        {
            new User
            {
                Id = AdminId,
                Mobile = "09120000001",
                FullName = "مدیر HelpDev",
                Role = UserRole.Admin,
                Stack = "Platform",
                CreatedAt = seedTime,
            },
            new User
            {
                Id = Writer1Id,
                Mobile = "09120000002",
                FullName = "سارا احمدی",
                Role = UserRole.Writer,
                Stack = "Frontend / React",
                CreatedAt = seedTime,
            },
            new User
            {
                Id = Writer2Id,
                Mobile = "09120000003",
                FullName = "علی رضایی",
                Role = UserRole.Writer,
                Stack = "Backend / .NET",
                CreatedAt = seedTime,
            },
            new User
            {
                Id = PrimaryAdminId,
                Mobile = PrimaryAdminMobile,
                FullName = "مدیر سیستم",
                Role = UserRole.Admin,
                Stack = "Platform",
                CreatedAt = seedTime,
            },
        };

        var contents = new List<ContentEntity>
        {
            CreateNews(
                "11111111-1111-1111-1111-111111111201",
                "React 19 با قابلیت‌های جدید Compiler منتشر شد",
                "react-19-compiler-release",
                "تیم React نسخه ۱۹ را با بهبودهای عملکردی و React Compiler معرفی کرد. این نسخه رندر را بهینه‌تر می‌کند و برای پروژه‌های بزرگ Next.js گزینه جذابی است.",
                Writer1Id,
                seedTime.AddDays(-5),
                views: 1240,
                saves: 86),
            CreateNews(
                "11111111-1111-1111-1111-111111111202",
                "راهنمای مهاجرت از .NET 8 به .NET 9",
                "dotnet-8-to-9-migration-guide",
                "مایکروسافت چک‌لیست مهاجرت را منتشر کرده است. تغییرات breaking کم است اما به‌روزرسانی پکیج‌ها و SDK را قبل از استقرار تست کنید.",
                Writer2Id,
                seedTime.AddDays(-4),
                views: 980,
                saves: 72),
            CreateNews(
                "11111111-1111-1111-1111-111111111203",
                "۵ ابزار AI که گردش کار کدنویسی را عوض می‌کنند",
                "top-ai-coding-tools-2026",
                "از Copilot تا ابزارهای review خودکار، این فهرست ابزارهایی را پوشش می‌دهد که تیم‌های توسعه در ۲۰۲۶ بیشترین استفاده را از آن‌ها دارند.",
                Writer1Id,
                seedTime.AddDays(-3),
                views: 2105,
                saves: 154),
            CreateNews(
                "11111111-1111-1111-1111-111111111204",
                "TypeScript 5.6: type narrowing سریع‌تر",
                "typescript-5-6-type-narrowing",
                "نسخه جدید TypeScript زمان type-check را کاهش می‌دهد و برای monorepoهای بزرگ بهبود محسوسی در تجربه توسعه ایجاد کرده است.",
                Writer2Id,
                seedTime.AddDays(-2),
                views: 756,
                saves: 41),
            CreateNews(
                "11111111-1111-1111-1111-111111111205",
                "PostgreSQL 17 برای workloadهای خواندنی بهینه‌تر شد",
                "postgresql-17-read-performance",
                "به‌روزرسانی آخر PostgreSQL برای APIهای پرترافیک و گزارش‌گیری HelpDev می‌تواند گزینه مناسبی باشد؛ vacuum و index tuning هم ساده‌تر شده است.",
                AdminId,
                seedTime.AddDays(-1),
                views: 543,
                saves: 29),

            CreateRoadmap(
                "11111111-1111-1111-1111-111111111301",
                "گام ۱: تسلط بر JavaScript مدرن",
                "roadmap-javascript-fundamentals",
                "ES6+، async/await، ماژول‌ها و مفاهیم پایه را با تمرین‌های کوتاه روزانه تثبیت کنید. پیش‌نیاز تمام مسیر فرانت‌اند HelpDev است.",
                Writer1Id,
                seedTime.AddDays(-6),
                views: 3200,
                saves: 410),
            CreateRoadmap(
                "11111111-1111-1111-1111-111111111302",
                "گام ۲: React و Next.js در عمل",
                "roadmap-react-nextjs",
                "با ساخت یک داشبورد RTL شبیه HelpDev، routing، server components و state management را یاد بگیرید.",
                Writer1Id,
                seedTime.AddDays(-5),
                views: 2875,
                saves: 365),
            CreateRoadmap(
                "11111111-1111-1111-1111-111111111303",
                "گام ۳: Backend با ASP.NET Core",
                "roadmap-aspnet-core-backend",
                "Clean Architecture، EF Core، JWT و API design را با پروژه واقعی پیاده کنید تا آماده مشارکت در بک‌اند HelpDev شوید.",
                Writer2Id,
                seedTime.AddDays(-4),
                views: 1940,
                saves: 298),

            CreateTool(
                "11111111-1111-1111-1111-111111111401",
                "بسته افزونه‌های VS Code برای توسعه‌دهنده فارسی",
                "tool-vscode-extensions-fa",
                "فهرستی از افزونه‌های RTL، Persian spell-check، REST Client و GitLens برای شروع سریع‌تر محیط توسعه.",
                Writer1Id,
                seedTime.AddDays(-3),
                views: 1675,
                saves: 220),
            CreateTool(
                "11111111-1111-1111-1111-111111111402",
                "جایگزین‌های رایگان Postman برای تیم‌های کوچک",
                "tool-postman-alternatives",
                "Hoppscotch، Bruno و VS Code REST Client مقایسه شده‌اند؛ برای تست APIهای HelpDev در لوکال مناسب هستند.",
                Writer2Id,
                seedTime.AddDays(-2),
                views: 1120,
                saves: 145),
            CreateTool(
                "11111111-1111-1111-1111-111111111403",
                "چیت‌شیت Git برای کار روزانه",
                "tool-git-cli-cheatsheet",
                "دستورات merge، rebase، stash و recovery سناریوهای رایج را پوشش می‌دهد — مخصوص قبل از PR روی main.",
                AdminId,
                seedTime.AddDays(-1),
                views: 2450,
                saves: 512),
        };

        context.Users.AddRange(users);
        context.Contents.AddRange(contents);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded {UserCount} users and {ContentCount} content items.",
            users.Length,
            contents.Count);
    }

    public static async Task EnsurePrimaryAdminAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Mobile == PrimaryAdminMobile, cancellationToken);

        if (user is null)
        {
            context.Users.Add(new User
            {
                Id = PrimaryAdminId,
                Mobile = PrimaryAdminMobile,
                FullName = "مدیر سیستم",
                Role = UserRole.Admin,
                Stack = "Platform",
                CreatedAt = DateTime.UtcNow,
            });

            logger.LogInformation("Created primary admin user for {Mobile}.", PrimaryAdminMobile);
        }
        else if (user.Role != UserRole.Admin)
        {
            user.Role = UserRole.Admin;
            context.Users.Update(user);
            logger.LogInformation("Upgraded {Mobile} to Admin role.", PrimaryAdminMobile);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static ContentEntity CreateNews(
        string id,
        string title,
        string slug,
        string body,
        Guid authorId,
        DateTime createdAt,
        int views,
        int saves) =>
        CreateContent(id, title, slug, body, ContentType.News, authorId, createdAt, views, saves);

    private static ContentEntity CreateRoadmap(
        string id,
        string title,
        string slug,
        string body,
        Guid authorId,
        DateTime createdAt,
        int views,
        int saves) =>
        CreateContent(id, title, slug, body, ContentType.RoadmapStep, authorId, createdAt, views, saves);

    private static ContentEntity CreateTool(
        string id,
        string title,
        string slug,
        string body,
        Guid authorId,
        DateTime createdAt,
        int views,
        int saves) =>
        CreateContent(id, title, slug, body, ContentType.Tool, authorId, createdAt, views, saves);

    private static ContentEntity CreateContent(
        string id,
        string title,
        string slug,
        string body,
        ContentType type,
        Guid authorId,
        DateTime createdAt,
        int views,
        int saves) =>
        ContentEntity.CreatePublishedSeed(
            Guid.Parse(id),
            title,
            Slug.Create(slug),
            body,
            type,
            authorId,
            createdAt,
            views,
            saves);
}
