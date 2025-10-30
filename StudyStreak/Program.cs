using System.Text.Json;
using System.Text.Json.Serialization;

var app = AppState.Load();
RunMenu(app);
AppState.Save(app);

// -------- App logic --------

static void RunMenu(AppState app)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== StudyStreak ===");
        Console.WriteLine("1) Start session");
        Console.WriteLine("2) Stop session");
        Console.WriteLine("3) Add goal");
        Console.WriteLine("4) Complete goal");
        Console.WriteLine("5) Today summary");
        Console.WriteLine("6) 7-day summary");
        Console.WriteLine("0) Save & exit");
        Console.Write("Select: ");
        var choice = Console.ReadLine()?.Trim();

        switch (choice)
        {
            case "1": StartSession(app); break;
            case "2": StopSession(app); break;
            case "3": AddGoal(app); break;
            case "4": CompleteGoal(app); break;
            case "5": PrintToday(app); break;
            case "6": PrintSevenDays(app); break;
            case "0":
                Console.WriteLine("Saving…");
                return;
            default:
                Console.WriteLine("Pick a valid option.");
                break;
        }
    }
}

static void StartSession(AppState app)
{
    if (app.ActiveSession != null)
    {
        Console.WriteLine("A session is already running.");
        return;
    }
    app.ActiveSession = new StudySession { StartUtc = DateTime.UtcNow };
    Console.WriteLine("Session started.");
    AppState.Save(app);
}

static void StopSession(AppState app)
{
    if (app.ActiveSession == null)
    {
        Console.WriteLine("No active session.");
        return;
    }
    app.ActiveSession.EndUtc = DateTime.UtcNow;
    var finished = app.ActiveSession.ToCompleted();
    app.Sessions.Add(finished);
    app.ActiveSession = null;
    Console.WriteLine($"Session saved: {finished.Minutes} minutes.");
    AppState.Save(app);
}

static void AddGoal(AppState app)
{
    Console.Write("Goal title: ");
    var title = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine("Title required.");
        return;
    }
    app.Goals.Add(new Goal { Title = title, Date = DateOnly.FromDateTime(DateTime.Now), Done = false });
    Console.WriteLine("Goal added.");
    AppState.Save(app);
}

static void CompleteGoal(AppState app)
{
    var today = DateOnly.FromDateTime(DateTime.Now);
    var todays = app.Goals.Where(g => g.Date == today).ToList();
    if (todays.Count == 0)
    {
        Console.WriteLine("No goals for today.");
        return;
    }

    for (int i = 0; i < todays.Count; i++)
        Console.WriteLine($"{i + 1}) {(todays[i].Done ? "[x]" : "[ ]")} {todays[i].Title}");

    Console.Write("Complete which? ");
    if (int.TryParse(Console.ReadLine(), out int pick) && pick >= 1 && pick <= todays.Count)
    {
        todays[pick - 1].Done = true;
        Console.WriteLine("Marked complete.");
        AppState.Save(app);
    }
    else
    {
        Console.WriteLine("Invalid choice.");
    }
}

static void PrintToday(AppState app)
{
    var today = DateOnly.FromDateTime(DateTime.Now);

    int minutes = app.Sessions
        .Where(s => s.Date == today)
        .Sum(s => s.Minutes);

    int goalsDone = app.Goals
        .Where(g => g.Date == today && g.Done)
        .Count();

    int goalsTotal = app.Goals
        .Where(g => g.Date == today)
        .Count();

    Console.WriteLine();
    Console.WriteLine($"Today: {minutes} minutes");
    Console.WriteLine($"Goals: {goalsDone}/{goalsTotal} done");

    if (app.ActiveSession != null)
    {
        var runningFor = DateTime.UtcNow - app.ActiveSession.StartUtc;
        Console.WriteLine($"Active session: {Math.Max(0, (int)runningFor.TotalMinutes)} minutes so far");
    }
}

static void PrintSevenDays(AppState app)
{
    var today = DateOnly.FromDateTime(DateTime.Now);
    var start = today.AddDays(-6);

    Console.WriteLine();
    Console.WriteLine("Last 7 days (minutes):");

    int streak = 0;
    for (int i = 0; i < 7; i++)
    {
        var day = start.AddDays(i);
        int mins = app.Sessions.Where(s => s.Date == day).Sum(s => s.Minutes);
        Console.WriteLine($"{day}: {mins}");
    }

    // Streak = consecutive days ending today with >0 minutes
    var d = today;
    while (d >= start)
    {
        int mins = app.Sessions.Where(s => s.Date == d).Sum(s => s.Minutes);
        if (mins > 0) streak++;
        else break;
        d = d.AddDays(-1);
    }

    Console.WriteLine($"Current streak (ending today): {streak} day(s)");
}

// -------- Models & persistence --------

public class StudySession
{
    public DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }

    [JsonIgnore]
    public int Minutes
    {
        get
        {
            var end = EndUtc ?? DateTime.UtcNow;
            var mins = (int)Math.Round((end - StartUtc).TotalMinutes);
            return Math.Max(0, mins);
        }
    }

    public CompletedSession ToCompleted()
        => new CompletedSession
        {
            Date = DateOnly.FromDateTime(StartUtc.ToLocalTime()),
            Minutes = Minutes
        };
}

public class CompletedSession
{
    public DateOnly Date { get; set; }
    public int Minutes { get; set; }
}

public class Goal
{
    public string Title { get; set; } = "";
    public DateOnly Date { get; set; }
    public bool Done { get; set; }
}

public class AppState
{
    public List<CompletedSession> Sessions { get; set; } = new();
    public List<Goal> Goals { get; set; } = new();
    public StudySession? ActiveSession { get; set; }

    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "data.json");

    public static AppState Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new AppState();
            var json = File.ReadAllText(FilePath);
            var opts = JsonOptions();
            var state = JsonSerializer.Deserialize<AppState>(json, opts) ?? new AppState();
            return state;
        }
        catch
        {
            // If anything's wrong with the file, start fresh (simple & safe)
            return new AppState();
        }
    }

    public static void Save(AppState state)
    {
        try
        {
            var opts = JsonOptions();
            var json = JsonSerializer.Serialize(state, opts);
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(tmp, FilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(Could not save data: {ex.Message})");
        }
    }

    private static JsonSerializerOptions JsonOptions()
        => new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new DateOnlyConverter() }
        };
}

// DateOnly <-> JSON (System.Text.Json)
public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.ParseExact(reader.GetString() ?? "", Format);

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}
