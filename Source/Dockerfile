#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Presentation/CH.CleanArchitecture.Presentation.WebApp/CH.CleanArchitecture.Presentation.WebApp.csproj", "Presentation/CH.CleanArchitecture.Presentation.WebApp/"]
COPY ["Presentation/CH.CleanArchitecture.Presentation.Framework/CH.CleanArchitecture.Presentation.Framework.csproj", "Presentation/CH.CleanArchitecture.Presentation.Framework/"]
COPY ["Presentation/CH.CleanArchitecture.Presentation.EmailTemplates/CH.CleanArchitecture.Presentation.EmailTemplates.csproj", "Presentation/CH.CleanArchitecture.Presentation.EmailTemplates/"]
COPY ["Core/CH.CleanArchitecture.Core.Application/CH.CleanArchitecture.Core.Application.csproj", "Core/CH.CleanArchitecture.Core.Application/"]
COPY ["Core/CH.CleanArchitecture.Core.Domain/CH.CleanArchitecture.Core.Domain.csproj", "Core/CH.CleanArchitecture.Core.Domain/"]
COPY ["Infrastructure/CH.CleanArchitecture.Infrastructure/CH.CleanArchitecture.Infrastructure.csproj", "Infrastructure/CH.CleanArchitecture.Infrastructure/"]
COPY ["Infrastructure/CH.CleanArchitecture.Infrastructure.ServiceBus/CH.CleanArchitecture.Infrastructure.ServiceBus.csproj", "Infrastructure/CH.CleanArchitecture.Infrastructure.ServiceBus/"]
COPY ["Infrastructure/CH.CleanArchitecture.Infrastructure.Shared/CH.CleanArchitecture.Infrastructure.Shared.csproj", "Infrastructure/CH.CleanArchitecture.Infrastructure.Shared/"]
COPY ["Infrastructure/CH.CleanArchitecture.Infrastructure.Auditing/CH.CleanArchitecture.Infrastructure.Auditing.csproj", "Infrastructure/CH.CleanArchitecture.Infrastructure.Auditing/"]
COPY ["CH.CleanArchitecture.Resources/CH.CleanArchitecture.Resources.csproj", "CH.CleanArchitecture.Resources/"]
COPY ["CH.CleanArchitecture.Common/CH.CleanArchitecture.Common.csproj", "CH.CleanArchitecture.Common/"]
RUN dotnet restore "Presentation/CH.CleanArchitecture.Presentation.WebApp/CH.CleanArchitecture.Presentation.WebApp.csproj"
COPY . .
WORKDIR "/src/Presentation/CH.CleanArchitecture.Presentation.WebApp"
RUN dotnet build "CH.CleanArchitecture.Presentation.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CH.CleanArchitecture.Presentation.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CH.CleanArchitecture.Presentation.WebApp.dll"]
