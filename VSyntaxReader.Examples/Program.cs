using Calendare.VSyntaxReader.Examples;
using Calendare.VSyntaxReader.Examples.Utils;

var runner = new ExampleRunner();

var readIcs = new ReadIcs();
runner.Execute(nameof(readIcs.BasicRead), readIcs.BasicRead);
runner.Execute(nameof(readIcs.HugeRead), readIcs.HugeRead);
runner.Execute(nameof(readIcs.StructuredRead), readIcs.StructuredRead);


var writeIcs = new WriteIcs();
runner.Execute(nameof(writeIcs.BasicWrite), writeIcs.BasicWrite);
runner.Execute(nameof(writeIcs.BasicAmendWrite), writeIcs.BasicAmendWrite);
runner.Execute(nameof(writeIcs.FromScratchWrite), writeIcs.FromScratchWrite);


var occurrences = new OccurrenceExamples();
runner.Execute(nameof(occurrences.Basic), occurrences.Basic);

