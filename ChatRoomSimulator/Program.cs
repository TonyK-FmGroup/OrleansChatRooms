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
user1.SetName("Tommy");
user1.EnterRoom(Guid.Empty);

Thread.Sleep(1000);

var user2 = clusterClient.GetGrain<IUserGrain>(Guid.NewGuid());
user2.SetName("Peter");
user2.EnterRoom(Guid.Empty);

Thread.Sleep(1000);

user1.SendMessage(Guid.Empty, "Hello, anybody there?");

Thread.Sleep(1000);

user2.SendMessage(Guid.Empty, "Yes, I'm here! But I'm about to leave!");

Thread.Sleep(1000);

user2.ExitRoom(Guid.Empty);



