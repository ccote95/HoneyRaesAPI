using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;


List<Customer> customers = new List<Customer> {
    new()
    {
        Name = "Benjamin Thompson",
        Id = 1,
        Address ="123 Main Street Anytown, USA"
    },
      new()
    {
        Name = "Olivia Jenkins",
        Id = 2,
        Address ="4321 South Street Extra, Tn"
    },
      new()
    {
        Name = "Ethan Martinez",
        Id = 3,
        Address ="51 E. Main Street Concord, Nh"
    },
 };
List<Employee> employees = new List<Employee> {

    new()
    {
        Name = "Charlotte Patel",
        Id = 1,
        Specialty = "Android Products"
    },
    new()
    {
        Name = "Daniel Garcia",
        Id = 2,
        Specialty = "Mac Books"
    },

 };
List<ServiceTicket> serviceTickets = new List<ServiceTicket> {
    new()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = 0,
        Description = "My computer speakers randomly started working, not too sure why.",
        Emergency = false,
        DateCompleted = new DateTime(2024,03,25)
    },
        new()
    {
        Id = 2,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Broken Samsung s10 screen.",
        Emergency = true,
        DateCompleted = DateTime.MinValue
    },
        new()
    {
        Id = 3,
        CustomerId = 3,
        EmployeeId = 2,
        Description = "Mac book wont turn on",
        Emergency = true,
        DateCompleted = new DateTime(2024,04,01)
    },
        new()
    {
        Id = 4,
        CustomerId = 1,
        EmployeeId = 0,
        Description = "Phone speaker stopped working. sounds awful.",
        Emergency = false,
        DateCompleted = DateTime.MinValue
    },
        new()
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "Mac book fans don't work.",
        Emergency = false,
        DateCompleted = new DateTime(2024,03,10)
    },
};
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/hello", () =>
{
    return "hello";
});
app.MapGet("/ServiceTickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});
//gets tickets by id
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    Employee employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});


// creates a new service ticket
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{

    // Get the customer data to check that the customerid for the service ticket is valid
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    // if the client did not provide a valid customer id, this is a bad request
    if (customer == null)
    {
        return Results.BadRequest();
    }

    // creates a new id (SQL will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });

});
//deletes a ticket by its id
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicketToDelete == null)
    {
        return Results.BadRequest();
    }
    serviceTickets.Remove(serviceTicketToDelete);
    return Results.NoContent();
});
// Updates a ticket
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    // gets the service ticket that matches the id
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    // this updates the ticketTpUpdate with the values in serviceTicket
    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});
// updates the date completed on a service ticket
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
    return Results.NoContent();
});

// get customer by id
app.MapGet("/Customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();

    }
    List<ServiceTicket> ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = ServiceTickets.Select(ticket => new ServiceTicketDTO
        {


            Id = ticket.Id,
            CustomerId = ticket.CustomerId,
            EmployeeId = ticket.EmployeeId,
            Description = ticket.Description,
            Emergency = ticket.Emergency,
            DateCompleted = ticket.DateCompleted
        }).ToList()
    });
});

// get employee by id
app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
