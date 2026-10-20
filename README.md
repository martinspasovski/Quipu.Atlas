# Quipu.Atlas

Quipu.Atlas is an ASP.NET Core MVC API for synchronously submitting a customer onboarding application. It stores the application, checks the submitted identity document with IDNow, screens the applicant with World-Check, stores both provider outcomes, and returns the resulting application status.

## Design and request flow

The API is organized into four responsibilities:

- **API** binds the HTTP request and returns the application result. The controller delegates submission to `ISubmitApplicationService`.
- **Application** validates the request, coordinates persistence and provider calls, and applies idempotency handling.
- **Domain** decides whether the result is approved, rejected, pending review, or has failed checks.
- **Infrastructure** implements SQL Server persistence and the IDNow and World-Check HTTP clients. Development uses configurable HTTP stubs.

For each new submission, the application service:

1. Validates the request and its `Idempotency-Key`.
2. Saves the application with `CHECKS_IN_PROGRESS` before calling providers. SQL connections are used for short persistence operations; no SQL transaction is held open during provider calls.
3. Calls IDNow and World-Check concurrently.
4. Saves the provider attempts and final decision.
5. Returns the application ID, status, and optional reason code.

The client makes one submission request and does not poll. Reusing an idempotency key with the same request refers to the existing application: a completed application returns its stored result, while one still being processed returns `409 Conflict`. Reusing the key with a different request is a conflict.

### Decision outcomes

| Condition | Application status | HTTP status |
| --- | --- | ---: |
| Both checks pass and World-Check is clear | `APPROVED` | 201 |
| IDNow reports an invalid document | `REJECTED` | 201 |
| IDNow is inconclusive | `REJECTED` | 201 |
| IDNow reports a face mismatch | `REJECTED` | 201 |
| World-Check reports a possible match | `PENDING_REVIEW` | 201 |
| A provider call fails or times out | `CHECKS_FAILED` | 502 |
| Matching request is already being processed | `CHECKS_IN_PROGRESS` | 409 |

An application rejection is a completed business decision, so it is returned as `201 Created` and retained in the database. `CHECKS_FAILED` means a provider check could not be completed; the application record is also retained.

## Run SQL Server locally

Run these commands from the solution root (`Quipu.Atlas`):

```bash
docker compose -f platform/docker-compose.yml up -d sql
docker compose -f platform/docker-compose.yml ps
```

Wait until the `sql` service is healthy. The development connection string in `src/Quipu.Atlas.Api/appsettings.Development.json` targets `localhost:1433` with the local credentials defined in the compose file. The SQL data is stored in a Docker volume.

## Apply EF Core migrations

Migrations are applied manually at present. Automatic migration at application startup will be configured separately.

If `dotnet ef` is not installed, install the EF Core 8 tool:

```bash
dotnet tool install --global dotnet-ef --version 8.0.32
```

From the solution root, apply the migrations:

```bash
dotnet ef database update \
  --project src/Quipu.Atlas.Infrastructure/Quipu.Atlas.Infrastructure.csproj \
  --startup-project src/Quipu.Atlas.Api/Quipu.Atlas.Api.csproj \
  --context AtlasDbContext
```

The connection string is read from the API `appsettings.json` by the design-time `AtlasDbContext` factory. The migration creates the `QuipuAtlas` database and its tables if they do not exist.

## Run the API

The HTTP launch profile uses `Development`, where both provider stubs are enabled by default. From the solution root:

```bash
dotnet run --project src/Quipu.Atlas.Api/Quipu.Atlas.Api.csproj --launch-profile http
```

The API listens at `http://localhost:5211`; Swagger is available at `http://localhost:5211/swagger`.

## Submit an application

Send a unique `Idempotency-Key` header with each new application. This example uses Base64 strings that satisfy the current input validator; replace them with real document and selfie image data when testing image handling.

```http
POST http://localhost:5211/applications
Idempotency-Key: atlas-demo-001
Content-Type: application/json

{
  "firstName": "Alex",
  "lastName": "Example",
  "dateOfBirth": "1991-03-04",
  "country": "MB",
  "nationality": "MK",
  "nationalId": "0403991450014",
  "issuingCountry": "MKD",
  "email": "alex@example.com",
  "phone": "+38970123456",
  "documents": [
    { "type": "ID_CARD", "image": "aGVsbG8=" },
    { "type": "SELFIE", "image": "aGVsbG8=" }
  ],
  "termsAccepted": true
}
```

With the default `IDNow:Stub:Scenario` set to `VALID` and `WorldCheck:Stub:Scenario` set to `CLEAR`, the response is:

```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "applicationId": "<generated-application-id>",
  "status": "APPROVED",
  "reasonCode": null
}
```

## Failed-check examples

### Rejected application

Set `Providers:IdNow:Stub:Scenario` to `INVALID` in `src/Quipu.Atlas.Api/appsettings.Development.json`, restart the API, and submit the same request with a new idempotency key. The request is valid, but IDNow rejects the document:

```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "applicationId": "<generated-application-id>",
  "status": "REJECTED",
  "reasonCode": "IDNOW_DOCUMENT_INVALID"
}
```

Other IDNow stub scenarios include `INCONCLUSIVE` and `FACE_MISMATCH`; both return a rejected application with a corresponding reason code.

### Provider request failure

Set `Providers:IdNow:Stub:Scenario` to `HTTP_ERROR`, restart the API, and submit with a new idempotency key. The stub returns an HTTP error to the provider client; Atlas retains the application and returns:

```http
HTTP/1.1 502 Bad Gateway
Content-Type: application/json

{
  "applicationId": "<generated-application-id>",
  "status": "CHECKS_FAILED",
  "reasonCode": "IDNOW_HTTP_503"
}
```

### Possible World-Check match

Set `Providers:WorldCheck:Stub:Scenario` to `POSSIBLE_PEP_MATCH` or `POSSIBLE_SANCTIONS_MATCH`. The API returns `201 Created` with status `PENDING_REVIEW` and reason code `WORLDCHECK_POSSIBLE_MATCH`.

## Stop local SQL Server

```bash
docker compose -f platform/docker-compose.yml down
```

The named SQL volume is retained, so the database remains available when the container is started again. Do not add `-v` if you want to keep that data.
