namespace API.Infrastructure.DI;

public static class EmailRegister
{
    public static WebApplicationBuilder AddEmailService(this WebApplicationBuilder builder)
    {
        var emailConfig = builder.Configuration
            .GetSection("EmailConfiguration")
            .Get<EmailConfig>();
        builder.Services.AddSingleton(emailConfig!);
        builder.Services.AddTransient<SmtpClient>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();

        return builder;
    }
}