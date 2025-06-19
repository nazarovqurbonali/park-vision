namespace WebUI.Extensions.Middlewares;

public static class RegisterMiddleware
{
    public static WebApplication MapMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();

        app.UseHttpsRedirection();
        app.UseRouting();


        app.MapStaticAssets();

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();


        app.Run();
        return app;
    }
}