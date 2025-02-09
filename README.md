# Clean Architecture (Blazor Server)

[![Build Status](https://dev.azure.com/chadjiantoniou/CleanArchitectureBlazor/_apis/build/status%2Fthecodewrapper.CH.CleanArchitectureBlazor?branchName=main)](https://dev.azure.com/chadjiantoniou/CleanArchitectureBlazor/_build/latest?definitionId=5&branchName=main)
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://www.paypal.com/donate?hosted_button_id=XSXQYY5KBMXYW)

A personal take on Clean Architecture for Blazor Server. For a detailed explanation, check out my blog post: [Implementing a Clean Architecture in ASP.NET Core 8](https://thecodewrapper.com/dev/tcw-clean-achitecture).

Looking for the ASP.NET Core MVC version? Find it [here](https://github.com/thecodewrapper/CH.CleanArchitecture).

## 🚀 Technologies Used
- **ASP.NET Core 8**
- **Entity Framework Core 8**
- **MassTransit**
- **AutoMapper**
- **Blazor & MudBlazor**
- **GuardClauses**
- **xUnit, Moq, Fluent Assertions, FakeItEasy**
- **Docker Support**

## 🌟 Features
- 🌍 **Localization**: Provides multi-language support to enhance user experience across different regions.
- 📜 **Event Sourcing**: Event sourcing using EF Core and SQL Server as persistent storage, including snapshots and retroactive events.
- 💾 **Data Persistence**: EventStore repository and DataEntity generic repository. Persistence can be swapped between them, fine-grained to individual entities.
- 🔐 **Secure Configurations**: Persistent application configurations with optional encryption for added security.
- 📑 **Data Auditing**: Built-in auditing for entities not using EventStore, ensuring traceability of data operations.
- 👤 **User Management**: Local user management with ASP.NET Core Identity.
- 🏗️ **Domain/Data entity separatation**: Clean separation of data entities and domain objects and mapping between them for persistence/retrieval using AutoMapper.
- 🎨 **Blazor UI**: Uses Blazor Server with MudBlazor components for a modern UI framework.
- ⚡ **CQRS Pattern**: Implements command and query separation using handler abstractions, supporting MassTransit or MediatR with minimal changes.
- 📩 **Service Bus Abstractions**: Supports message-broker solutions like MassTransit or MediatR, with MassTransit’s mediator as the default implementation.
- 🏗 **Domain-Driven Design**: Unforcefully promoting Domain-Driven Design tactical patterns with aggregates, entities and domain event abstractions.
- 🔏 **Lightweight Authorization**: Implements a custom ASP.NET Core AuthorizationHandler for fine-grained access control.
- 🐳 **Docker Ready**: Offers containerization support for both SQL Server and the web application.

### ✨ Additional Goodies
- ✅ **Password Generator**: Implements a robust password generation strategy based on ASP.NET Core Identity configurations.
- ✅ **Reusable Razor Components**: Includes ready-made Blazor components for common functionalities like CRUD buttons, toast notifications, modals, Blazor Select2 integration.
- ✅ **Common Utilities**: Provides a utility library with type extensions, result wrapper objects, paginated result structures, date format converters, and other useful tools.

---

## 📌 Pre-Deployment Setup
### 🔧 AppSettings Configuration
```json
"Admin": {
  "Username": "{set username here}",
  "Password": "{set password here}",
  "Email": "{set email here}"
},
"Storage": {
  "StorageProvider": "azure" 
},
"EmailSender": {
  "UseSendGrid": true
}
```
### 🔑 User Secrets Configuration
```sh
dotnet user-secrets --project CH.CleanArchitecture.Presentation.WebApp set "ConnectionStrings:ApplicationConnection" "{connection_string}"
dotnet user-secrets --project CH.CleanArchitecture.Presentation.WebApp set "ConnectionStrings:IdentityConnection" "{connection_string}"
```
Or manually edit the `secrets.json` file:
```json
"ConnectionStrings": {
  "ApplicationConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=CH.CleanArchitecture;Integrated Security=True;MultipleActiveResultSets=True",
  "IdentityConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=CH.CleanArchitecture;Integrated Security=True;MultipleActiveResultSets=True"
}
```

---

## 📦 Post-Deployment Setup
### 🔹 Create an Admin Account
Use the `CreateAdmin` endpoint in `SetupController`.

### 🔹 Cloud Storage Setup
#### **Azure Blob Storage** (Skip if using AWS S3)
- Create a container: `profile-pictures`
- Generate a **SAS token** with **Blob permissions** and copy the connection string.
- Assign `Storage Blob Data Reader` to the app's managed identity.

#### **AWS S3**
- Create an **S3 bucket** for documents.
- Inside the bucket, create a folder: `profile-pictures`.

### 🔹 Application Configuration (Under Developer → Application Config)
These settings reside under **Developer → Application Config** in the navigation menu. The following are the configuration keys:

- **EmailSmtpSettings**: `smtp.test.com|587|true|false|username|password`
- **SecurityGoogleRecaptchaClientKey**: `{your_google_recaptcha_client_key}`
- **SecurityGoogleRecaptchaSecretKey**: `{your_google_recaptcha_secret_key}`
- **CryptoJWTSymmetricKey**: `{your_jwt_key}`
- **CryptoJWTIssuer**: `{your_issuer}`
- **CryptoJWTAuthority**: `{your_authority}`
- **AzureBlobStorageBaseURI**: `https://{YOUR_BLOB_STORAGE_ACCOUNT}.blob.core.windows.net/`
- **AzureStorageConnectionString**: `{your_azure_storage_connection_string}`
- **AWSS3Region**: `eu-west-1`
- **AWSS3BucketName**: `{your_s3_bucket_name}`
- **AWSS3EndpointFormat**: `https://{bucket-name}.s3.{region}.amazonaws.com/`
- **AWSAccessKeyId**: `{your_aws_access_key}`
- **AWSSecretAccessKey**: `{your_aws_secret_key}`
- **ResourcesProfilePicturesFolder**: `profile-pictures`

### 🔹 Schedule Jobs (Under Developer → Jobs)
Enroll the following jobs:

- ✅ **Audit History Purge Job**
- ✅ **Notifications Purge Job**

---

## 🛠 Developer Guidelines
### ✅ Best Practices
- **Domain Event Classes** → Require an internal empty constructor.
- **Aggregate Root Classes** → Require a private empty constructor when using the event store.
- **CQRS Handlers**
  - Commands inherit `BaseCommand`.
  - Queries inherit `BaseQuery`.
  - Use `Result<T>` or `Result` for return values.
- **Persistence** →
  - Data models implement `IDataEntity<TId>`.
- **Domain-Driven Design** →
  - Aggregate roots implement `IAggregateRoot<T, TId>`.
- **Application Services** → Return `Result<T>` or `Result`.
---
### 📌 EF Core Migrations
To add migrations for different contexts, use the following commands:

🗂 **Application Context**:
  ```sh
  add-migration {MIGRATION_NAME_HERE} -context ApplicationDbContext -o Migrations/Application
  ```

🏛 **Event Store Context**:
  ```sh
  add-migration {MIGRATION_NAME_HERE} -context EventStoreDbContext -o Migrations/EventStore
  ```

🔐 **Identity Context**:
  ```sh
  add-migration {MIGRATION_NAME_HERE} -context IdentityDbContext -o Migrations/Identity
  ```
---
### 📌 Database Updates
The following commands install/update the databases. This process runs automatically every time the project is executed, but you can also run it manually in **Package Manager Console**, ensuring `CH.CleanArchitecture.Infrastructure` is selected as the **Default Project**, and `CH.CleanArchitecture.Presentation.WebApp` is set as the **solution startup project**:

🗂 **Update Application Database**:
  ```sh
  update-database -context ApplicationDbContext
  ```
🏛 **Update Event Store Database**:
  ```sh
  update-database -context EventStoreDbContext
  ```
🔐 **Update Identity Database**:
  ```sh
  update-database -context IdentityDbContext
  ```
---
### 📌 Docker Compose
```sh
docker-compose up
```
---

**Happy Coding! 🚀**