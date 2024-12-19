﻿using System.ComponentModel;

namespace Devlead.Console.Integration.Test.Commands.Settings;

public class TestSettings : CommandSettings
{
    [CommandOption("--test-version")]
    [Description("Version to test against")]
    public required string TestVersion { get; set; }
}