namespace VSyntaxReader.Tests.Recurrence;

public static partial class RecurrenceTestLibrary
{
    public static TheoryData<RRuleUsecase> SecondlyTestdata() => new()
    {
//{ new RRuleUsecase("20241001T143000", "FREQ=SECONDLY;INTERVAL=173;UNTIL=20241101T000000;BYDAY=SA,SU;BYMINUTE=0,59;BYHOUR=0,23;BYSECOND=0,1,2,3,4,5,6,7,8,9,10,59,58,57,56,55,54,53,52,51,50", "Europe/Zurich", new(-1, []))},
{ new RRuleUsecase("20241001T143000", "FREQ=SECONDLY;INTERVAL=173;UNTIL=20241101T000000;BYDAY=SA,SU;BYMINUTE=0,59;BYHOUR=0,23;BYSECOND=0,1,2,3,4,5,6,7,8,9,10,59,58,57,56,55,54,53,52,51,50", "Europe/Zurich", new(6, [
"2024-10-05T00:00:08 Europe/Zurich (+02)",
"2024-10-06T23:00:02 Europe/Zurich (+02)",
"2024-10-12T23:59:03 Europe/Zurich (+02)",
"2024-10-13T23:00:10 Europe/Zurich (+02)",
"2024-10-27T00:59:52 Europe/Zurich (+02)",
"2024-10-27T23:00:59 Europe/Zurich (+01)",
]))},
{ new RRuleUsecase("20241027T025905", "FREQ=SECONDLY;INTERVAL=10;COUNT=12", "Europe/Zurich", new(12, [
"2024-10-27T02:59:05 Europe/Zurich (+02)",
"2024-10-27T02:59:15 Europe/Zurich (+02)",
"2024-10-27T02:59:25 Europe/Zurich (+02)",
"2024-10-27T02:59:35 Europe/Zurich (+02)",
"2024-10-27T02:59:45 Europe/Zurich (+02)",
"2024-10-27T02:59:55 Europe/Zurich (+02)",
"2024-10-27T02:00:05 Europe/Zurich (+01)",
"2024-10-27T02:00:15 Europe/Zurich (+01)",
"2024-10-27T02:00:25 Europe/Zurich (+01)",
"2024-10-27T02:00:35 Europe/Zurich (+01)",
"2024-10-27T02:00:45 Europe/Zurich (+01)",
"2024-10-27T02:00:55 Europe/Zurich (+01)",
]))},


    };

}
