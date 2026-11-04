
using System.Net.Http.Json;

// Đọc địa chỉ API từ tham số dòng lệnh
var apiBaseUrl = args.Length > 0
    ? args[0]
    : "http://localhost:5000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

// 1. Lấy danh sách bài tập
var assignments =
    await http.GetFromJsonAsync<List<Assignment>>(
        "/api/assignments");

Console.WriteLine("Danh sach bai tap:");

foreach (var assignment in assignments ?? new List<Assignment>())
{
    Console.WriteLine($"- {assignment.Id}: {assignment.Title}");
}

// 2. Gửi bài nộp
var request = new CreateSubmissionRequest(
    AssignmentId: 1,
    StudentCode: "SV001",
    RepositoryUrl: "https://example.com/sv001/bai-tap-1"
);

var response = await http.PostAsJsonAsync(
    "/api/submissions", request);

response.EnsureSuccessStatusCode();

var result =
    await response.Content.ReadFromJsonAsync<SubmissionResult>();

Console.WriteLine("\nDa nop bai thanh cong!");
Console.WriteLine($"Ma GUID: {result?.Id}");
Console.WriteLine($"Trang thai: {result?.Status}");

// 3. Tra cứu lại lượt nộp bằng GUID
if (result != null)
{
    var found = await http.GetFromJsonAsync<SubmissionResult>(
        $"/api/submissions/{result.Id}");

    Console.WriteLine("\nKet qua tra cuu:");
    Console.WriteLine($"GUID: {found?.Id}");
    Console.WriteLine($"Ma sinh vien: {found?.StudentCode}");
    Console.WriteLine($"Ma bai tap: {found?.AssignmentId}");
    Console.WriteLine($"Trang thai: {found?.Status}");
}

// Các record dữ liệu phía Client
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
