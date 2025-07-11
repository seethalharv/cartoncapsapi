using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Enums;
using CartonCaps.ReferralApi.Repositories;
using CartonCaps.ReferralApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
	.CreateLogger();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseInMemoryDatabase("ReferralDb"));

builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<IReferralLinkService, ReferralLinkService>();
builder.Services.AddScoped<INotificationService, NotificationService>();



var app = builder.Build();


//Seeding data here to get the experience of having a populated database

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	var userProfiles = new List<UserReferralProfile>
	{
		new UserReferralProfile {Id = 1, UserId = 100, ReferralCode= "REF100DEF"},
		new UserReferralProfile {Id = 2, UserId = 101, ReferralCode= "REF101ABC"},
	    new UserReferralProfile {Id = 3, UserId = 200, ReferralCode = "REF200XYZ"}
	};

	var users = new List<User>
	{
		new User { Id = 100, FirstName = "Alice", LastName = "Smith", EmailOrPhone = "user1@gmail.com" },
		new User { Id = 101, FirstName = "Bob", LastName = "Johnson", EmailOrPhone = "user2@gmail.com" },
		new User { Id = 200, FirstName = "Charlie", LastName = "Brown", EmailOrPhone = "user3@gmail.com" }
	};

	var referrals = new List<Referrals>
	{
		new Referrals
		{
			Id = 1,
		ReferrerUserId = 100,
		ReferralCode = "REF101ABC",
		EmailOrPhone = "friend1@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-3),
		ReferralStatusId = (int)ReferralStatus.Pending,
        
		},
		new Referrals
		{
			Id = 2,
		ReferrerUserId = 100,
		ReferralCode = "REF101ABC",
		EmailOrPhone = "friend2@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-2),
		ReferralStatusId = (int)ReferralStatus.Successful
		},
		new Referrals
	{
		Id = 3,
		ReferrerUserId = 101,
		ReferralCode = "REF102XYZ",
		EmailOrPhone = "friend3@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-1),
		ReferralStatusId = (int)ReferralStatus.Redeemed
	},
	new Referrals
	{
		Id = 4,
		ReferrerUserId = 101,
		ReferralCode = "REF102XYZ",
		EmailOrPhone = "colleague@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-5),
		ReferralStatusId =  (int)ReferralStatus.NotificationFailed
	}
	};
	
	context.Referrals.AddRange(referrals);
	context.AddRange(users);
	context.UserRefProfiles.AddRange(userProfiles);
	context.SaveChanges();
	
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
