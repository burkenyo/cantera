// This file is part of Cantera. See License.txt in the top-level directory or
// at https://cantera.org/license.txt for license and copyright information.

using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text.RegularExpressions;
using Cantera.Interop;
using Xunit;

namespace Cantera.Tests;

[Collection("Application")]
public class ApplicationTest
{
    class FooException : Exception { }

    readonly static LogMessageEventArgs s_mockLog =
        new(LogLevel.Warning, "Testing", "This is a test message.");

    [Fact]
    public void CanteraInfo_VersionRetrieved()
    {
        // version string should start with a number
        Assert.Matches("^[1-9]", Application.Version);
    }

    [Fact]
    public void CanteraInfo_GitCommitRetrieved()
    {
        Assert.NotEmpty(Application.GitCommit);
    }

    [Fact]
    public void CanteraInfo_DataDirectoryAdded()
    {
        var dirs = Application.DataDirectories;
        var originalCount = dirs.Count;
        var longestDir = dirs.MaxBy(d => d.FullName.Length);

        Assert.NotNull(longestDir);

        dirs.Add(Path.Join(longestDir!.FullName, "garbazh"));

        Assert.Equal(originalCount + 1, dirs.Count);
    }

    [Fact]
    public void LogWriter_MessageLogged()
    {
        LogMessageEventArgs? args = null;

        void LogMessage(object? sender, LogMessageEventArgs e)
        {
            args = e;
        }

        try
        {
            Application.MessageLogged += LogMessage;

            ProduceRealLogOutput();

            Assert.Equal(LogLevel.Info, args?.LogLevel);
            Assert.False(String.IsNullOrEmpty(args?.Category));
            Assert.False(String.IsNullOrEmpty(args.Message));
        }
        finally
        {
            Application.MessageLogged -= LogMessage;
        }
    }

    [Fact]
    public void LogWriter_ConsoleLogged()
    {
        var stdOut = Console.Out;
        var consoleOut = new StringWriter();

        try
        {
            Console.SetOut(consoleOut);
            Application.AddConsoleLogging();
            ProduceMockLogOutput();

            var output = consoleOut.ToString();

            var logLevel = s_mockLog.LogLevel.ToString().ToUpperInvariant();

            var prefix = $"{logLevel} ({s_mockLog.Category}) ";
            const string iso8601FormatString = "yyyy-MM-ddThh:mm:ss.fffzzz";
            const int lengthOfIso8601FormattedString = 29;

            Assert.Matches('^' + Regex.Escape(prefix), output);

            var nowString = output.Substring(
                prefix.Length, lengthOfIso8601FormattedString);

            Assert.True(DateTimeOffset.TryParseExact(
                nowString, iso8601FormatString, null, default, out _));
        }
        finally
        {
            Application.RemoveConsoleLogging();
            Console.SetOut(stdOut);
        }
    }

    [Fact]
    public void LogWriter_ExceptionRegistered()
    {
        static void LogMessage(object? sender, LogMessageEventArgs e) =>
            throw new FooException();

        try
        {
            Application.MessageLogged += LogMessage;

            var thrown =
                Assert.Throws<CallbackException>(() => ProduceMockLogOutput());

            Assert.NotNull(thrown.InnerException);
            Assert.IsType<FooException>(thrown.InnerException);
        }
        finally
        {
            Application.MessageLogged -= LogMessage;
        }
    }

    /// <summary>
    /// Produces log output by calling into the native Cantera library to invoke
    /// the logging callback.
    /// </summary>
    static void ProduceRealLogOutput()
    {
        InteropUtil.CheckReturn(LibCantera.ct_writeLog(s_mockLog.Message + "\n"));
    }

    /// <summary>
    /// Produces log output without calling into the native Cantera library.
    /// </summary>
    unsafe static void ProduceMockLogOutput()
    {
        var eventField = typeof(Application).GetField("s_invokeMessageLoggedDelegate",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(eventField);

        var del = (LibCantera.LogCallback) eventField.GetValue(null)!;

        Assert.NotNull(del);

        byte* category = null;
        byte* message = null;
        try
        {
            category = Utf8StringMarshaller.ConvertToUnmanaged(s_mockLog.Category);
            message = Utf8StringMarshaller.ConvertToUnmanaged(s_mockLog.Message);
            del(s_mockLog.LogLevel, category, message);
        }
        finally
        {
            Utf8StringMarshaller.Free(category);
            Utf8StringMarshaller.Free(message);
        }

        CallbackException.ThrowIfAny();
    }
}
