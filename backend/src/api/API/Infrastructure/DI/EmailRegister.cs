namespace API.Infrastructure.DI;

/// <summary>
/// This class registers email service dependencies within the DI container for the application.
/// It configures services necessary for sending emails, including SMTP client and email configuration.
/// </summary>
public static class EmailRegister
{
    /// <summary>
    /// Registers the email service related dependencies in the DI container.
    /// It loads email configuration settings from the application's configuration and adds the necessary services for email handling.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance to register services with.</param>
    /// <returns>The WebApplicationBuilder instance with the email service dependencies registered.</returns>
    public static WebApplicationBuilder AddEmailService(this WebApplicationBuilder builder)
    {
        // Retrieving email configuration from the application's configuration settings.
        EmailConfig? emailConfig = builder.Configuration
            .GetSection("EmailConfiguration")
            .Get<EmailConfig>();

        // Registering the email configuration as a singleton to ensure it's only created once.
        builder.Services.AddSingleton(emailConfig!);

        // Registering services needed to send emails:
        // - SmtpClient is transient, meaning a new instance is created each time it is requested.
        builder.Services.AddTransient<SmtpClient>();

        // - IEmailService is scoped, meaning it will be created once per request.
        builder.Services.AddScoped<IEmailService, EmailService>();

        // - ISmtpClientWrapper is scoped to wrap SMTP-related functionality.
        builder.Services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();

        // Returning the builder for method chaining.
        return builder;
    }
}