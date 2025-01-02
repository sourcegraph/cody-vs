using Cody.VisualStudio.Services;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using Xunit;
using Xunit.Abstractions;

namespace Cody.VisualStudio.Tests
{
    public class AutocompleteTests : TestsBase
    {
        public AutocompleteTests(ITestOutputHelper output) : base(output) { }


        [VsFact(Version = VsVersion.VS2022, ReuseInstance = false)]
        public async Task Autocomplete_Is_Working()
        {
            await GetPackageAsync();
            await WaitForChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"));
            await WaitForAsync(() => CodyPackage.TestingSupportService.InProgressBackgroundTasksCount == 0);

            var doc = Dte.ActiveDocument.Object() as TextDocument;
            var oldText = doc.CreateEditPoint(doc.StartPoint).GetText(doc.EndPoint);
            var lineOfCode = "        if (repeat < 0) throw new ArgumentException(\"repeat must be greater than 0\");";
            var position = FindPositionAfterText(oldText, lineOfCode);
            Assert.NotNull(position);

            doc.Selection.MoveToLineAndOffset(position.Value.Line, position.Value.Column);

            var sim = new InputSimulator();
            sim.Keyboard
                .KeyPress(VirtualKeyCode.RETURN)
                .TextEntry("if (str");

            var suggestion = await GetCodyAutocompleteSuggestion();

            sim.Keyboard.KeyPress(VirtualKeyCode.TAB).Sleep(200);

            var newText = doc.CreateEditPoint(doc.StartPoint).GetText(doc.EndPoint);

            Assert.Contains(suggestion, newText);

            Dte.ActiveDocument.Close(vsSaveChanges.vsSaveChangesNo);
        }

        [VsFact(Version = VsVersion.VS2022)]
        public async Task Explicit_Invocation_Is_Working()
        {
            await GetPackageAsync();
            await WaitForChat();
            await OpenSolution(SolutionsPaths.GetConsoleApp1File("ConsoleApp1.sln"));
            await OpenDocument(SolutionsPaths.GetConsoleApp1File(@"ConsoleApp1\Manager.cs"));
            await WaitForAsync(() => CodyPackage.TestingSupportService.InProgressBackgroundTasksCount == 0);

            var doc = Dte.ActiveDocument.Object() as TextDocument;
            var oldText = doc.CreateEditPoint(doc.StartPoint).GetText(doc.EndPoint);
            var lineOfCode = "        if (repeat < 0) throw new ArgumentException(\"repeat must be greater than 0\");";
            var position = FindPositionAfterText(oldText, lineOfCode);
            Assert.NotNull(position);

            doc.Selection.MoveToLineAndOffset(position.Value.Line + 1, 9);

            var sim = new InputSimulator();
            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LMENU, VirtualKeyCode.OEM_COMMA);

            var suggestion = await GetCodyAutocompleteSuggestion();
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB).Sleep(200);

            var newText = doc.CreateEditPoint(doc.StartPoint).GetText(doc.EndPoint);

            Assert.Contains(suggestion, newText);

            Dte.ActiveDocument.Close(vsSaveChanges.vsSaveChangesNo);
        }

        private async Task<string> GetCodyAutocompleteSuggestion()
        {
            var set = new HashSet<string>();
            var sim = new InputSimulator();
            TestingSupportService.AutocompleteSuggestion suggestion;
            while(true)
            {
                suggestion = await CodyPackage.TestingSupportService.GetAutocompleteSuggestion();

                if (suggestion == null) throw new Exception("No sugesstions");
                if (suggestion.IsCodySuggestion) return suggestion.SuggestionText;
                if (set.Contains(suggestion.SuggestionId)) throw new Exception("No Cody suggestions");

                set.Add(suggestion.SuggestionId);
                sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LMENU, VirtualKeyCode.OEM_COMMA);
            }
        }

        private (int Line, int Column)? FindPositionAfterText(string document, string textToFind)
        {
            var lines = document.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var line = Array.FindIndex(lines, x => x.StartsWith(textToFind));
            if (line == -1) return null;
            
            return (line + 1, textToFind.Length + 1);
        }
    }
}
