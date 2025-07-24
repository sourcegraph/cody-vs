using Cody.Core.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {

        [TestCase(@"c:\path\to\file", "file:///c%3A/path/to/file")]
        [TestCase(@"c:/path/to/file", "file:///c%3A/path/to/file")]
        [TestCase(@"c:/file.txt", "file:///c%3A/file.txt")]
        [TestCase(@"file.txt", "file:///file.txt")]
        public void Can_Convert_Path_To_Valid_Uri(string path, string expectedPath)
        {
            var result = StringExtensions.ToUri(path);

            Assert.That(result, Is.EqualTo(expectedPath));
        }

        [TestCase("line1\rline2\rline3\r", "line1\nline2\nline3\n")]
        [TestCase("line1\nline2\nline3\n", "line1\nline2\nline3\n")]
        [TestCase("line1\r\nline2\r\nline3\r\n", "line1\nline2\nline3\n")]
        [TestCase("line1\r\nline2\rline3\n", "line1\nline2\nline3\n")]
        public void Can_Replace_Line_Brakers(string text, string expectedText)
        {
            var result = StringExtensions.ConvertLineBreaks(text, "\n");

            Assert.That(result, Is.EqualTo(expectedText));
        }
    }
}
