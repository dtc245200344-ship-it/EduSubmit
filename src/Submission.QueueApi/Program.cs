using System.Collections.Concurrent;
using System.Threading.Channels;

var queue = Channel.CreateUnbounded<SubmissionJob>();
var submissions = new ConcurrentDictionary<Guid, SubmissionResult>();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var assignments = new[]
{
new Assignment(1, "Bai tap 1", "Tao API dau tien"),
new Assignment(2, "Bai tap 2", "Goi API tu client"),
new Assignment(3, "Bai tap 3", "Tim hieu giao tiep HTTP")
};

app.MapGet("/", () => Results.Ok(new
{
service = "Submission.QueueApi",
status = "running"
}));

app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapPost("/api/submissions", async (CreateSubmissionRequest request) =>
{
if (request.AssignmentId <= 0 ||
string.IsNullOrWhiteSpace(request.StudentCode) ||
string.IsNullOrWhiteSpace(request.RepositoryUrl))
{
return Results.BadRequest(new
{
message = "Du lieu nop bai khong hop le"
});
}

var job = new SubmissionJob(
Guid.NewGuid(),
request.AssignmentId,
request.StudentCode,
request.RepositoryUrl
);

var submission = new SubmissionResult(
job.Id,
job.AssignmentId,
job.StudentCode,
"processing",
null
);

submissions[job.Id] = submission;

await queue.Writer.WriteAsync(job);

Console.WriteLine($"[API] Nhan {job.StudentCode} -> processing");

return Results.Accepted(
$"/api/submissions/{job.Id}",
submission
);
});

app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
if (!submissions.TryGetValue(id, out var submission))
{
return Results.NotFound(new
{
message = "Khong tim thay luot nop bai"
});
}

return Results.Ok(submission);
});

var worker = Task.Run(async () =>
{
await foreach (var job in queue.Reader.ReadAllAsync())
{
Console.WriteLine($"[Worker] Bat dau cham {job.StudentCode}");

await Task.Delay(3000);

var result = new SubmissionResult(
job.Id,
job.AssignmentId,
job.StudentCode,
"graded",
8.5
);

submissions[job.Id] = result;

Console.WriteLine(
$"[Worker] Cham xong {job.StudentCode} - 8.5 diem"
);
}
});

app.Run();

record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
int AssignmentId,
string StudentCode,
string RepositoryUrl
);

record SubmissionJob(
Guid Id,
int AssignmentId,
string StudentCode,
string RepositoryUrl
);

record SubmissionResult(
Guid Id,
int AssignmentId,
string StudentCode,
string Status,
double? Score
);