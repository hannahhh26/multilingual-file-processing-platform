# Frontend

Angular frontend for the Multilingual File Processing Platform.

The interface provides job management and the complete file-processing workflow, including source upload, preprocessing, translation upload, validation feedback and delivery download.

## Implementation

The frontend is intentionally lightweight, with most UI behaviour contained in the `JobList` component.

Angular signals are used for local state such as the selected job, selected files, errors and translation validation results.

API communication is kept separate from the component in `JobService`, which uses Angular's `HttpClient` to communicate with the ASP.NET Core backend.

## Environment Configuration

The API URL is provided through Angular environment files.

Development uses the locally hosted API:

```text
http://localhost:8080/api/jobs