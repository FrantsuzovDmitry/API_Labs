using Grpc.Net.Client;
using PizzaClient;

using var channel = GrpcChannel.ForAddress("https://localhost:7085");

var client = new Pizza.PizzaClient(channel);

// Create response
//var reply = await client.CreatePizzaAsync(
//				new CreatePizzaRequest
//				{
//					Name = "Veggie Pizza",
//					Price = 269.99f
//				});

// Read response
//var reply = await client.ReadPizzaAsync(
//					new ReadPizzaRequest
//					{
//						Id = 1
//					});

// Read all response
var reply = await client.GetPizzaListAsync(
							new GetAllRequest { });

// Update response
//var reply = await client.UpdatePizzaAsync(
//							new UpdatePizzaRequest
//							{
//								Id = 3,
//								Name = "Veggie Pizza",
//								Price = 189.99f
//							});

// Delete response
//var reply = await client.DeletePizzaAsync(
//					new DeletePizzaRequest
//					{
//						Id = 1
//					});

Console.WriteLine("Server response: " + reply);