FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
COPY . /src/
WORKDIR /src/
RUN dotnet build ZKRD.Slack.sln -c Release
RUN dotnet test ZKRD.Slack.sln -c Release

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS release
COPY --from=builder /src/Zkrd.Slack.Runner/bin/Release/net6.0/ /app/
WORKDIR /app/
RUN useradd runner
RUN chmod 755 Zkrd.Slack.Runner
USER runner
CMD ./Zkrd.Slack.Runner
