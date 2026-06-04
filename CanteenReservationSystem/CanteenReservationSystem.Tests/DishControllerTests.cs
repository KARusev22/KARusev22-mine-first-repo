using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CanteenReservationSystem.Controllers;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class DishControllerTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private ControllerContext CreateControllerContext(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
        var httpContext = new DefaultHttpContext { User = user };
        return new ControllerContext { HttpContext = httpContext };
    }

    private class FakeDishService : IDishService
    {
        public List<Dish> Items { get; } = new();
        public bool CreateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public Task CreateAsync(Dish dish)
        {
            CreateCalled = true;
            dish.Id = Items.Count + 1;
            Items.Add(dish);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            DeleteCalled = true;
            Items.RemoveAll(d => d.Id == id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Dish>> GetAllAsync() => Task.FromResult<IEnumerable<Dish>>(Items);
        public Task<Dish?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(d => d.Id == id));
        public Task UpdateAsync(Dish dish)
        {
            var existing = Items.FirstOrDefault(d => d.Id == dish.Id);
            if (existing != null) Items.Remove(existing);
            Items.Add(dish);
            return Task.CompletedTask;
        }
        public Task<IEnumerable<Dish>> FilterByCategoryAsync(int categoryId)
        {
            return Task.FromResult<IEnumerable<Dish>>(Items.Where(d => d.Category != null && d.Category.Id == categoryId));
        }
    }

    private class FakeIngredientService : IIngredientService
    {
        public Task<IEnumerable<Ingredient>> GetAllAsync() => Task.FromResult<IEnumerable<Ingredient>>(new List<Ingredient>());
        public Task<Ingredient?> GetByIdAsync(int id) => Task.FromResult<Ingredient?>(null);
        public Task CreateAsync(Ingredient ingredient) => Task.CompletedTask;
        public Task UpdateAsync(Ingredient ingredient) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<IEnumerable<Dish>> GetDishesByIngredientAsync(int ingredientId) => Task.FromResult<IEnumerable<Dish>>(new List<Dish>());
        public Task<Ingredient> FindOrCreateByNameAsync(string name)
        {
            return Task.FromResult(new Ingredient { Id = Math.Abs(name.GetHashCode()), IngredientName = name });
        }
    }

    private UserManager<ApplicationUser> CreateUserManager()
    {
        var store = new InMemoryUserStore();
        return new UserManager<ApplicationUser>(store, Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(), Enumerable.Empty<IUserValidator<ApplicationUser>>(), Enumerable.Empty<IPasswordValidator<ApplicationUser>>(), new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null, new NullLogger<UserManager<ApplicationUser>>());
    }

    private class InMemoryUserStore : IUserStore<ApplicationUser>
    {
        public Task<IdentityResult> CreateAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public void Dispose() { }
        public Task<ApplicationUser?> FindByIdAsync(string userId, System.Threading.CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, System.Threading.CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user?.UserName ?? string.Empty);
        public Task<string> GetUserIdAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user?.Id ?? string.Empty);
        public Task<string> GetUserNameAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user?.UserName ?? string.Empty);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SetUserNameAsync(ApplicationUser user, string? userName, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    }

    [Fact]
    public async Task Index_ReturnsViewWithDishes()
    {
        using var context = CreateContext("Index_ReturnsViewWithDishes");
        var dishSvc = new FakeDishService();
        dishSvc.Items.Add(new Dish { Id = 1, DishName = "A" });
        var controller = new DishController(context, CreateUserManager(), new FakeEnv(), dishSvc, new FakeIngredientService())
        {
            ControllerContext = CreateControllerContext("u1")
        };

        var result = await controller.Index();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Dish>>(view.Model);
        Assert.Single(model);
    }

    private class FakeEnv : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "App";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetTempPath());
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetTempPath());
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var context = CreateContext("Create_Get_ReturnsView");
        var controller = new DishController(context, CreateUserManager(), new FakeEnv(), new FakeDishService(), new FakeIngredientService())
        {
            ControllerContext = CreateControllerContext("u2")
        };

        var result = controller.Create();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_Valid_RedirectsToIndex_AndCreatesDish()
    {
        using var context = CreateContext("Create_Post_Valid_RedirectsToIndex_AndCreatesDish");
        var dishSvc = new FakeDishService();
        var controller = new DishController(context, CreateUserManager(), new FakeEnv(), dishSvc, new FakeIngredientService())
        {
            ControllerContext = CreateControllerContext("u3")
        };

        var dish = new Dish { DishName = "X", Category = new Category { CategoryName = "C" }, Price = 1.0m };
        // Ensure Request.Form can be read by the controller
        var http = controller.ControllerContext.HttpContext;
        http.Request.ContentType = "application/x-www-form-urlencoded";
        http.Features.Set<Microsoft.AspNetCore.Http.Features.IFormFeature>(new Microsoft.AspNetCore.Http.Features.FormFeature(new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>())));

        var result = await controller.Create(dish, null, new List<string>(), new List<int>());

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(dishSvc.CreateCalled);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundWhenNull()
    {
        using var context = CreateContext("Details_ReturnsNotFoundWhenNull");
        var dishSvc = new FakeDishService();
        var controller = new DishController(context, CreateUserManager(), new FakeEnv(), dishSvc, new FakeIngredientService())
        {
            ControllerContext = CreateControllerContext("u4")
        };

        var result = await controller.Details(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_CallsDeleteAndRedirects()
    {
        using var context = CreateContext("DeleteConfirmed_CallsDeleteAndRedirects");
        var dishSvc = new FakeDishService();
        dishSvc.Items.Add(new Dish { Id = 2, DishName = "Z" });
        var controller = new DishController(context, CreateUserManager(), new FakeEnv(), dishSvc, new FakeIngredientService())
        {
            ControllerContext = CreateControllerContext("u5")
        };

        var result = await controller.DeleteConfirmed(2);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(dishSvc.DeleteCalled);
    }
}
