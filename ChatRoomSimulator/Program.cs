using Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;

Console.WriteLine("Starting chat room simulator...");

var host = Host.CreateDefaultBuilder()
                .UseOrleansClient((ctx, client) =>
                {
                    client.UseLocalhostClustering();

                    // client.UseStaticClustering(new IPEndPoint(IPAddress.Loopback, 30000));

                    // client.UseAzureStorageClustering(options => options.ConfigureTableServiceClient(Environment.GetEnvironmentVariable("POIAzure")));
                })
                .Build();
host.Start();

var clusterClient = host.Services.GetRequiredService<IClusterClient>();

//Create lobby
var lobby = clusterClient.GetGrain<IRoomGrain>(Guid.Empty);

var user1 = clusterClient.GetGrain<IUserGrain>(Guid.NewGuid());
await user1.SetName("Tommy");
await user1.EnterRoom(Guid.Empty);
var user1id = (await user1.GetUserInfo()).Id;

await lobby.SetName(user1id, "The Lobby");

Thread.Sleep(1000);

var user2 = clusterClient.GetGrain<IUserGrain>(Guid.NewGuid());
await user2.SetName("Peter");
await user2.EnterRoom(Guid.Empty);

Thread.Sleep(1000);

await user1.SendMessage(Guid.Empty, "Hello, anybody there?");

Thread.Sleep(1000);

await user2.SendMessage(Guid.Empty, "Yes, I'm here! But I'm about to leave!");

Thread.Sleep(1000);

await user2.ExitRoom(Guid.Empty);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
