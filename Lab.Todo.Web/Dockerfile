#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Lab.Todo.Web/", "Lab.Todo.Web/"]
COPY ["Lab.Todo.Web.Common/", "Lab.Todo.Web.Common/"]
COPY ["Lab.Todo.BLL.Managers/", "Lab.Todo.BLL.Managers/"]
COPY ["Lab.Todo.DAL/", "Lab.Todo.DAL/"]
RUN dotnet restore "Lab.Todo.Web/Lab.Todo.Web.csproj"  --disable-parallel
WORKDIR "/src/Lab.Todo.Web"
RUN dotnet build "Lab.Todo.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lab.Todo.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lab.Todo.Web.dll"]