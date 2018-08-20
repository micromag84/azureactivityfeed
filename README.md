## Async activity feed pipeline in Azure
 
Ready to use activity feed using an async pipeline consisting of:
 
- OpenAPI 2.0 (Swagger) endpoint hosted as a Azure Function
- Azure Storage Queue
- ASP.NET Core 2.1 IHostedService
- Azure Cosmos Document DB
- ASP.NET Core Razor Page to display the feed
- [MediatR](https://github.com/jbogard/MediatR) to dispatch commands (vertical slice)

# Introduction

Required Tools:

* [.NET Core 2.1 SDK](https://www.microsoft.com/net/download/core)
* [Azure Function Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
* [Azure CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
* [Azure Blob Storage Local Emulator](https://azure.microsoft.com/de-de/downloads/)
* [Postman](https://www.getpostman.com/) 

1. Clone the repo
1. Restore dependencies with `dotnet restore`
1. Execute `dotnet dev-certs https --trust` to add a dev cert and trust localhost
1. Start the function and web with `dotnet run`
1. Open the application in the [browser](https://localhost:5001)

Use Postman or any other tool to Post a message with content-type `application/json`

```json
{
	Source: "Postman",
	Title: "We love azure",
	Text: "Hello people",
	Url: "aheadintranet.com",
	MediaUrl: ""
}