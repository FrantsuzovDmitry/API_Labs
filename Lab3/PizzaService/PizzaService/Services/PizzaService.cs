using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PizzaShopService.Data;
using PizzaShopService.Models;
using System;

namespace PizzaService.Services
{
	public class PizzaService : Pizza.PizzaBase
	{
		private readonly PizzaShopContext _dbContext;

		public PizzaService(PizzaShopContext dbContext)
		{
			_dbContext = dbContext;
		}

		public override async Task<CreatePizzaResponse> CreatePizza(CreatePizzaRequest request, ServerCallContext context)
		{
			if (request.Name == string.Empty || request.Price <= 0)
			{
				throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a not null data"));
			}
			var pizza = new Product
			{
				Name = request.Name,
				Price = (decimal)request.Price
			};

			await _dbContext.AddAsync(pizza);
			await _dbContext.SaveChangesAsync();

			return await Task.FromResult(new CreatePizzaResponse
			{
				Id = pizza.Id
			});
		}

		public override async Task<ReadPizzaResponse> ReadPizza(ReadPizzaRequest request, ServerCallContext context)
		{
			if (request.Id <= 0)
				throw new RpcException(new Status(StatusCode.InvalidArgument, "index must be more than 0"));

			var pizza = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == request.Id);

			if (pizza != null)
			{
				return await Task.FromResult(new ReadPizzaResponse
				{
					Id = pizza.Id,
					Name = pizza.Name,
					Price = (float)pizza.Price
				});
			}
			throw new RpcException(new Status(StatusCode.NotFound, $"No objects with id {request.Id}"));
		}

		public override async Task<GetAllResponse> GetPizzaList(GetAllRequest request, ServerCallContext context)
		{
			var response = new GetAllResponse();
			var pizzaList = await _dbContext.Products.ToListAsync();

			foreach (var toDoItem in pizzaList)
			{
				response.PizzaList.Add(new ReadPizzaResponse
				{
					Id = toDoItem.Id,
					Name = toDoItem.Name,
					Price = (float)toDoItem.Price,
				});
			}

			return await Task.FromResult(response);
		}

		public override async Task<UpdatePizzaResponse> UpdatePizza(UpdatePizzaRequest request, ServerCallContext context)
		{
			if (request.Id <= 0 || request.Name == string.Empty || request.Price <= 0)
				throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid data"));

			var pizza = _dbContext.Products.FirstOrDefault(item => item.Id == request.Id);

			if (pizza == null)
				throw new RpcException(new Status(StatusCode.NotFound, $"No objects with id {request.Id}"));
			else
			{
				pizza.Name = request.Name;
				pizza.Price = (decimal)request.Price;
			}

			await _dbContext.SaveChangesAsync();

			return await Task.FromResult(new UpdatePizzaResponse { Id = pizza.Id });
		}

		public override async Task<DeletePizzaResponse> DeletePizza(DeletePizzaRequest request, ServerCallContext context)
		{
			if (request.Id <= 0)
				throw new RpcException(new Status(StatusCode.InvalidArgument, "index must be more than 0"));

			var pizza = _dbContext.Products.FirstOrDefault(item => item.Id == request.Id);

			if (pizza == null)
				throw new RpcException(new Status(StatusCode.NotFound, $"No objects with id {request.Id}"));

			_dbContext.Products.Remove(pizza);
			await _dbContext.SaveChangesAsync();

			return await Task.FromResult(new DeletePizzaResponse
			{
				Id = pizza.Id,
			});
		}
	}
}
