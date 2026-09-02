# Multilingual File Processing Platform API

ASP.NET Core Web API responsible for job management, JSON preprocessing, translation validation and reconstruction for the Multilingual File Processing Platform.

## Processing Workflow

Each processing job follows the same workflow:

1. Create a job.
2. Upload a source JSON file.
3. Preprocess the source file.
4. Download the prepared translation file.
5. Translate the extracted segments.
6. Upload the translated file.
7. Validate and postprocess the translation.
8. Download the reconstructed delivery file.

Preprocessing separates translatable strings from the original JSON structure. Reconstruction data is stored internally so translated values can later be inserted back into their original locations.

## API Endpoints

All job endpoints are available under `/api/jobs`.

| Method | Endpoint                         | Description                                  |
| ------ | -------------------------------- | -------------------------------------------- |
| GET    | `/api/jobs`                      | Return all jobs                              |
| GET    | `/api/jobs/{id}`                 | Return a job by ID                           |
| POST   | `/api/jobs`                      | Create a processing job                      |
| PUT    | `/api/jobs/{id}`                 | Rename an existing job                       |
| DELETE | `/api/jobs/{id}`                 | Delete a job                                 |
| POST   | `/api/jobs/{id}/source`          | Upload the source JSON file                  |
| POST   | `/api/jobs/{id}/preprocess`      | Preprocess the uploaded source               |
| GET    | `/api/jobs/{id}/prepared-source` | Download the prepared translation file       |
| POST   | `/api/jobs/{id}/translation`     | Upload translated content                    |
| POST   | `/api/jobs/{id}/postprocess`     | Validate and reconstruct the translated file |
| GET    | `/api/jobs/{id}/delivery`        | Download the reconstructed delivery file     |

Swagger UI is available when the API is running in the Development environment.

## JSON Preprocessing and Reconstruction

`JsonProcessingService` recursively walks the uploaded JSON document and extracts string values as translation segments.

For example:

```json
{
  "product": {
    "name": "Wireless Headphones",
    "price": 129.99
  }
}
```

The string value is extracted as a translation segment:

```json
{
  "id": "seg-0001",
  "path": "product.name",
  "source": "Wireless Headphones"
}
```

Each extracted string receives a sequential segment ID such as `seg-0001`. The original string is replaced in the internal reconstruction data with a segment marker:

```json
{
  "product": {
    "name": {
      "__segmentId": "seg-0001"
    },
    "price": 129.99
  }
}
```

The segment path records where the value appeared in the source document and supports nested objects and arrays, for example `product.name`, `messages[0]` and `features[1].title`.

Non-string values such as numbers, booleans and null values remain unchanged in the reconstruction data.

After translation validation succeeds, the service creates a lookup between segment IDs and translated values. It recursively walks the reconstruction data and replaces each `__segmentId` marker with its corresponding translated string, producing a document with the same structure as the original source.

## Translation Validation

Before reconstruction, the translated file is compared with the segment IDs expected by the reconstruction data.

Validation detects:

* Missing segment IDs
* Duplicate segment IDs
* Unexpected segment IDs

If validation fails, postprocessing stops and the API returns the validation result rather than creating a delivery file. This prevents content from being silently reconstructed with missing or incorrect segments.

## Job Storage

Job metadata is persisted in PostgreSQL using Entity Framework Core, while files belonging to each processing job are stored separately.

Each job uses its ID to create an isolated file workspace:

```text
Uploads/
└── {jobId}/
    ├── Original/
    ├── PreparedSource/
    ├── ReconstructionData/
    ├── Translation/
    └── Delivery/
```

The directories correspond to the stages of the processing workflow:

* `Original` — uploaded source JSON
* `PreparedSource` — extracted translation package
* `ReconstructionData` — internal structure used during reconstruction
* `Translation` — translated package uploaded for postprocessing
* `Delivery` — reconstructed translated output

## Database Configuration

The API uses PostgreSQL through Entity Framework Core and the Npgsql provider.

The database connection is supplied through the `DefaultConnection` connection string. Production credentials are not stored in the repository and are provided through environment configuration.

EF Core migrations are applied automatically when the application starts.

## Running the API Locally

The complete application can be run using Docker Compose as described in the root README.

To run the API independently, a PostgreSQL database and a valid `DefaultConnection` connection string must be available.

From the API project directory:

```bash
dotnet restore
dotnet run
```

The Development launch profile runs the API at:

```text
http://localhost:5280
https://localhost:7271
```

Swagger UI is opened automatically when using the Development launch profile.

## Tests

The backend test project is located at:

```text
tests/MultilingualFileProcessingPlatform.Tests
```

The current xUnit test suite focuses on the core JSON processing logic, covering string extraction from nested objects and arrays, preservation of non-string values, reconstruction, and validation of missing, duplicate and unexpected segment IDs.

Run the tests from the repository root with:

```bash
dotnet test
```