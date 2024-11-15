using AktBob.DocumentGenerator;
using AktBob.DocumentGenerator.Contracts;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;
using AktBob.Shared;
using AktBob.Deskpro;
using AktBob.Deskpro.Contracts;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Services here
builder.Services.AddSerilog((_, config) => config.ReadFrom.Configuration(builder.Configuration));

var mediatrAssemblies = new List<Assembly>();
builder.Services.AddDocumentGeneratorModule(mediatrAssemblies);
builder.Services.AddDeskproModule(builder.Configuration, mediatrAssemblies);

// Mediatr
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssemblies(mediatrAssemblies.ToArray());
    c.NotificationPublisher = new TaskWhenAllPublisher();
});

builder.Services.AddSharedModule();

IHost host = builder.Build();


// Code here
var mediator = host.Services.GetRequiredService<IMediator>();
var customFields = await mediator.Send(new GetDeskproCustomFieldSpecificationsQuery());
var customFields2 = await mediator.Send(new GetDeskproCustomFieldSpecificationsQuery());


//var command = new GenerateDeskproMessageDocumentCommand(
//    "Aute tempor fugiat mollit ipsum anim eu adipisicing elit commodo do quis Lorem.",
//    new List<MessageDetailsDto>
//    {
//        new MessageDetailsDto(
//            2002,
//            7,
//            "<p>Minim ad in incididunt ex elit. Culpa ad nulla excepteur excepteur id do elit. In quis proident sunt ut consectetur magna officia labore ea amet aute est excepteur adipisicing. Non excepteur quis excepteur commodo ullamco elit.</p><p>Eu tempor minim do culpa elit. Esse voluptate deserunt cillum anim qui non elit proident eu dolore consequat eiusmod. Sint minim irure ea qui enim non reprehenderit elit sunt cillum aute incididunt irure pariatur.</p>\r\n",
//            DateTime.Now,
//            "Jakob Nørager Jørgensen",
//            "jakob@jakobjorgensen.com",
//            ["testdokument_10_sider.pdf", "testdokument_20_sider.pdf"])


//    });

//var result = await mediator.Send(command);

//File.WriteAllBytes(@$"C:\Users\aztmbjj\Desktop\document-{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", result.Value);

await host.StopAsync();