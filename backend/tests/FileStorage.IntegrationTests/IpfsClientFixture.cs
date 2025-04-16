using Common.Extensions;
using Ipfs.CoreApi;
using Ipfs.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FileStorage.IntegrationTests;

public sealed class IpfsClientFixture
{
    public IOptions<AddFileOptions> AddFileOptions { get; private set; }
    public IpfsClient IpfsClient { get; private set; }
    public string Url { get; private set; }

    public IpfsClientFixture()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddIniFile("appsettings.ini")
            .Build();

        Url = configuration.GetRequiredString("IpfsSettings:Url");

        AddFileOptions = Options.Create(new AddFileOptions()
        {
            Pin = false
        });

        IpfsClient = new IpfsClient(Url);
    }
}
