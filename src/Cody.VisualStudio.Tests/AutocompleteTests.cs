using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        [VsFact(Version = VsVersion.VS2022)]
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
                .TextEntry("if (str")
                .Sleep(5000)
                .KeyPress(VirtualKeyCode.TAB)
                .Sleep(200);

            var newText = doc.CreateEditPoint(doc.StartPoint).GetText(doc.EndPoint);

            Assert.Contains(CodyPackage.TestingSupportService.LastDisplayedAutocompleteSuggestion, newText);

            Dte.ActiveDocument.Close(vsSaveChanges.vsSaveChangesNo);
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
