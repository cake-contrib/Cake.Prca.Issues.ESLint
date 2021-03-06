﻿namespace Cake.Prca.Issues.EsLint.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Shouldly;
    using Testing;
    using Xunit;

    public class EsLintIssuesSettingsTests
    {
        public sealed class TheEsLintIssuesSettingsCtor
        {
            [Fact]
            public void Should_Throw_If_LogFilePath_Is_Null()
            {
                // Given / When
                var result = Record.Exception(() =>
                    EsLintIssuesSettings.FromFilePath(
                        null,
                        new JsonFormat(new FakeLog())));

                // Then
                result.IsArgumentNullException("logFilePath");
            }

            [Fact]
            public void Should_Throw_If_Format_For_LogFilePath_Is_Null()
            {
                // Given / When
                var result = Record.Exception(() =>
                    EsLintIssuesSettings.FromFilePath(
                        @"C:\foo.log",
                        null));

                // Then
                result.IsArgumentNullException("format");
            }

            [Fact]
            public void Should_Throw_If_LogFileContent_Is_Null()
            {
                // Given / When
                var result = Record.Exception(() =>
                    EsLintIssuesSettings.FromContent(
                        null,
                        new JsonFormat(new FakeLog())));

                // Then
                result.IsArgumentNullException("logFileContent");
            }

            [Fact]
            public void Should_Throw_If_LogFileContent_Is_Empty()
            {
                // Given / When
                var result = Record.Exception(() =>
                    EsLintIssuesSettings.FromContent(
                        string.Empty,
                        new JsonFormat(new FakeLog())));

                // Then
                result.IsArgumentOutOfRangeException("logFileContent");
            }

            [Fact]
            public void Should_Throw_If_LogFileContent_Is_WhiteSpace()
            {
                // Given / When
                var result = Record.Exception(() =>
                    EsLintIssuesSettings.FromContent(
                        " ",
                        new JsonFormat(new FakeLog())));

                // Then
                result.IsArgumentOutOfRangeException("logFileContent");
            }

            [Fact]
            public void Should_Throw_If_Format_For_LogFileContent_Is_Null()
            {
                // Given / When
                var result = Record.Exception(() =>
                    EsLintIssuesSettings.FromContent(
                        "foo",
                        null));

                // Then
                result.IsArgumentNullException("format");
            }

            [Fact]
            public void Should_Set_Property_Values_Passed_To_Constructor()
            {
                // Given
                const string logFileContent = "foo";
                var format = new JsonFormat(new FakeLog());

                // When
                var settings = EsLintIssuesSettings.FromContent(logFileContent, format);

                // Then
                settings.LogFileContent.ShouldBe(logFileContent);
                settings.Format.ShouldBe(format);
            }

            [Fact]
            public void Should_Read_File_From_Disk()
            {
                var fileName = Path.GetTempFileName();
                try
                {
                    // Given
                    string expected;
                    using (var ms = new MemoryStream())
                    using (var stream = this.GetType().Assembly.GetManifestResourceStream("Cake.Prca.Issues.EsLint.Tests.Testfiles.jsonFormatWindows.json"))
                    {
                        stream.CopyTo(ms);
                        var data = ms.ToArray();

                        using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                        {
                            file.Write(data, 0, data.Length);
                        }

                        expected = ConvertFromUtf8(data);
                    }

                    // When
                    var settings =
                        EsLintIssuesSettings.FromFilePath(
                            fileName,
                            new JsonFormat(new FakeLog()));

                    // Then
                    settings.LogFileContent.ShouldBe(expected);
                }
                finally
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
            }

            private static string ConvertFromUtf8(byte[] bytes)
            {
                var enc = new UTF8Encoding(true);
                var preamble = enc.GetPreamble();

                if (preamble.Where((p, i) => p != bytes[i]).Any())
                {
                    throw new ArgumentException("Not utf8-BOM");
                }

                return enc.GetString(bytes.Skip(preamble.Length).ToArray());
            }
        }
    }
}
