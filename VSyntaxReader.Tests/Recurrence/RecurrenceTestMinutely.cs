namespace VSyntaxReader.Tests.Recurrence;

public static partial class RecurrenceTestLibrary
{
    public static TheoryData<RRuleUsecase> MinutelyTestdata() => new()
    {
// { new RRuleUsecase("20241001T143000", "FREQ=MINUTELY;INTERVAL=3;UNTIL=20241101T000000;BYDAY=MO,TU,WE,TH,FR", "Europe/Zurich", new(-1, []))},
{ new RRuleUsecase("20241001T143000", "FREQ=MINUTELY;INTERVAL=15;UNTIL=20241010T000000;BYDAY=MO,FR;BYSECOND=0,30;BYMINUTE=0,30,45;BYHOUR=8,17", "Europe/Zurich", new(24, [
"2024-10-04T08:00:00 Europe/Zurich (+02)",
"2024-10-04T08:00:30 Europe/Zurich (+02)",
"2024-10-04T08:30:00 Europe/Zurich (+02)",
"2024-10-04T08:30:30 Europe/Zurich (+02)",
"2024-10-04T08:45:00 Europe/Zurich (+02)",
"2024-10-04T08:45:30 Europe/Zurich (+02)",
"2024-10-04T17:00:00 Europe/Zurich (+02)",
"2024-10-04T17:00:30 Europe/Zurich (+02)",
"2024-10-04T17:30:00 Europe/Zurich (+02)",
"2024-10-04T17:30:30 Europe/Zurich (+02)",
"2024-10-04T17:45:00 Europe/Zurich (+02)",
"2024-10-04T17:45:30 Europe/Zurich (+02)",
"2024-10-07T08:00:00 Europe/Zurich (+02)",
"2024-10-07T08:00:30 Europe/Zurich (+02)",
"2024-10-07T08:30:00 Europe/Zurich (+02)",
"2024-10-07T08:30:30 Europe/Zurich (+02)",
"2024-10-07T08:45:00 Europe/Zurich (+02)",
"2024-10-07T08:45:30 Europe/Zurich (+02)",
"2024-10-07T17:00:00 Europe/Zurich (+02)",
"2024-10-07T17:00:30 Europe/Zurich (+02)",
"2024-10-07T17:30:00 Europe/Zurich (+02)",
"2024-10-07T17:30:30 Europe/Zurich (+02)",
"2024-10-07T17:45:00 Europe/Zurich (+02)",
"2024-10-07T17:45:30 Europe/Zurich (+02)",
]))},
{ new RRuleUsecase("20241231T103045Z", "FREQ=MINUTELY;INTERVAL=3;COUNT=10", null, new(10, [
"2024-12-31T10:30:45 UTC (+00)",
"2024-12-31T10:33:45 UTC (+00)",
"2024-12-31T10:36:45 UTC (+00)",
"2024-12-31T10:39:45 UTC (+00)",
"2024-12-31T10:42:45 UTC (+00)",
"2024-12-31T10:45:45 UTC (+00)",
"2024-12-31T10:48:45 UTC (+00)",
"2024-12-31T10:51:45 UTC (+00)",
"2024-12-31T10:54:45 UTC (+00)",
"2024-12-31T10:57:45 UTC (+00)",
]))},
{ new RRuleUsecase("20241231T103045Z", "FREQ=MINUTELY;INTERVAL=10;BYSECOND=22,44;UNTIL=20241231T113044Z", null, new(12, [
"2024-12-31T10:40:22 UTC (+00)",
"2024-12-31T10:40:44 UTC (+00)",
"2024-12-31T10:50:22 UTC (+00)",
"2024-12-31T10:50:44 UTC (+00)",
"2024-12-31T11:00:22 UTC (+00)",
"2024-12-31T11:00:44 UTC (+00)",
"2024-12-31T11:10:22 UTC (+00)",
"2024-12-31T11:10:44 UTC (+00)",
"2024-12-31T11:20:22 UTC (+00)",
"2024-12-31T11:20:44 UTC (+00)",
"2024-12-31T11:30:22 UTC (+00)",
"2024-12-31T11:30:44 UTC (+00)",
]))},


    };

}
