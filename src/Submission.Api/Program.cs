
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Danh sách bài tập mẫu
var assignments = new[]
{
    new Assignment(1, "Bai tap 1", "Tao API dau tien"),
    new Assignment(2, "Bai tap 2", "Goi API tu client"),
    new Assignment(3, "Bai tap 3", "Tim hieu giao tiep HTTP")
};

// Lưu các lượt nộp bài trong bộ nhớ
var submissions = new List<SubmissionResult>();

// GET / - kiểm tra API
app.MapGet("/", () => Results.Ok(new
{
    service = "Submission.Api",
    status = "running"
}));

// GET /api/assignments - lấy danh sách bài tập
app.MapGet("/api/assignments", () =>
    Results.Ok(assignments));

// POST /api/submissions - gửi bài nộp
app.MapPost("/api/submissions", (CreateSubmissionRequest request) =>
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

    var submission = new SubmissionResult(
        Guid.NewGuid(),
        request.AssignmentId,
        request.StudentCode,
        "received",
        DateTimeOffset.UtcNow
    );

    // Lưu lượt nộp vào danh sách
    submissions.Add(submission);

    return Results.Created(
        $"/api/submissions/{submission.Id}",
        submission
    );
});

// GET /api/submissions/{id} - tra cứu lượt nộp bằng GUID
app.MapGet("/api/submissions/{id:guid}", (Guid id) =>
{
    var submission = submissions.FirstOrDefault(s => s.Id == id);

    if (submission == null)
    {
        return Results.NotFound(new
        {
            message = "Khong tim thay luot nop bai"
        });
    }

    return Results.Ok(submission);
});

app.Run();

// Các record dữ liệu
record Assignment(int Id, string Title, string Description);

record CreateSubmissionRequest(
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl
);

record SubmissionResult(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string Status,
    DateTimeOffset ReceivedAt
);
