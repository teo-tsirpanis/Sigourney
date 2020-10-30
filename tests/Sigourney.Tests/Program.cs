// Copyright (c) 2020 Theodore Tsirpanis
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Diagnostics;
using static System.Diagnostics.TraceLevel;

namespace Sigourney.Tests
{
    static class Program
    {
        // The following line would fail on .NET Framework-based MSBuild.
        // When writing this constant, Cecil will try to resolve the "netstandard"
        // assembly. It will fail on .NET Framework-based MSBuild because the .NET
        // Starnard edition of Cecil (that is always used by Sigourney) will fail
        // to find the assembly because it will use .NET Core-specific methods.
        // Cecil's PR 701 will make it use the .NET Framework-specific resolver
        // on .NET Standard if needed, and PR 702 will omit the resolver for the
        // enum constant writer.
        // Until either of the fixes above is shipped, what Sigourney can do is
        // explicitly pass the assembly references for Cecil to find, which is
        // good nevertheless.
        private const TraceLevel ThisLineWillCauseAn = Error;

        static int assertCount = 0;
        static int errorCount = 0;

        private static void PrintInColor(ConsoleColor color, string message, params object[] fmtArgs)
        {
            var colorPrevious = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message, fmtArgs);
            Console.ForegroundColor = colorPrevious;
        }

        private static void Assert(bool condition, string message)
        {
            assertCount++;
            if (condition) return;
            PrintInColor(ConsoleColor.Red, $"Error: {message}");
            errorCount++;
        }

        static int Main()
        {
            var thisAssembly = typeof(Program).Assembly;
            Assert(thisAssembly.GetType("TestWeaver1Rulez") != null, "The type weaver #1 added was not found.");
            Assert(thisAssembly.GetType("ProcessedByTestWeaver1") != null, "The type Sigourney added to mark weaver #1 was not found.");
            Assert(thisAssembly.GetType("TestWeaver2IzBetter") != null, "The type weaver #2 added was not found.");
            Assert(thisAssembly.GetType("ProcessedByTestWeaver2") != null, "The type Sigourney added to mark weaver #2 was not found.");

            if (errorCount == 0)
            {
                PrintInColor(ConsoleColor.Green, "Success. {0} asserts passed.", assertCount);
                return 0;
            }
            else
            {
                PrintInColor(ConsoleColor.Red, "Failure. {0} out of {1} asserts failed.", errorCount, assertCount);
                return 1;
            }
        }
    }
}
