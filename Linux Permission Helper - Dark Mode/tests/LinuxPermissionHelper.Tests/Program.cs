using EngineerForge.LinuxPermissionHelper.Models;
using EngineerForge.LinuxPermissionHelper.Services;

namespace EngineerForge.LinuxPermissionHelper.Tests;

internal static class Program
{
    private static int _passed;
    private static int _failed;

    private static int Main()
    {
        ConversionTests();
        SymbolicTests();
        MeaningTests();
        ProfileAndPresetTests();
        RiskTests();
        CommandTests();
        CommandSafetyTests();
        ParserTests();
        InvalidInputTests();
        ValidationStrictnessTests();
        StartupTests();

        Console.WriteLine();
        Console.WriteLine($"RESULT: {_passed} passed, {_failed} failed, {_passed + _failed} total");
        return _failed == 0 ? 0 : 1;
    }

    private static void ConversionTests()
    {
        foreach (var octal in new[] { "000", "600", "644", "700", "750", "755", "775", "777" })
        {
            Check($"Octal round trip {octal}", octal, PermissionConverter.FromOctal(octal).Octal);
        }

        for (var digit = 0; digit <= 7; digit++)
        {
            Check($"Digit flags {digit}", digit, PermissionConverter.FromDigit(digit).Digit);
        }

        var fromFlags = PermissionConverter.FromFlags(
            new PermissionFlags(true, true, true),
            new PermissionFlags(true, false, true),
            new PermissionFlags(true, false, true));
        Check("Flags produce 755", "755", fromFlags.Octal);
    }

    private static void SymbolicTests()
    {
        var cases = new Dictionary<string, string>
        {
            ["755"] = "rwxr-xr-x",
            ["644"] = "rw-r--r--",
            ["600"] = "rw-------",
            ["777"] = "rwxrwxrwx",
            ["000"] = "---------"
        };
        foreach (var pair in cases)
        {
            Check($"Symbolic {pair.Key}", pair.Value, PermissionConverter.FromOctal(pair.Key).Symbolic);
            Check($"Symbolic reverse {pair.Value}", pair.Key, PermissionConverter.SymbolicToOctal(pair.Value));
        }
    }

    private static void MeaningTests()
    {
        var access = new[] { "No access", "Execute only", "Write only", "Write + execute", "Read only", "Read + execute", "Read + write", "Full control" };
        var meaning = new[]
        {
            "Cannot access the object", "Can run or traverse only", "Can modify, but cannot read",
            "Can modify and run, but cannot read", "Can view, but cannot modify or run",
            "Can view and run, cannot modify", "Can view and modify, cannot run", "Full control"
        };
        for (var digit = 0; digit <= 7; digit++)
        {
            Check($"Access text {digit}", access[digit], PermissionAnalyzer.AccessText(digit));
            Check($"Meaning text {digit}", meaning[digit], PermissionAnalyzer.MeaningText(digit));
        }

        var explanation = PermissionConverter.Explain(PermissionConverter.FromOctal("755"));
        CheckTrue("Explanation contains owner", explanation.Contains("Owner:", StringComparison.Ordinal));
        CheckTrue("Explanation contains execute", explanation.Contains("read + write + execute", StringComparison.Ordinal));
    }

    private static void ProfileAndPresetTests()
    {
        var profiles = new Dictionary<string, string>
        {
            ["600"] = "Private file permissions",
            ["644"] = "Standard file permissions",
            ["700"] = "Private directory permissions",
            ["750"] = "Restricted directory permissions",
            ["755"] = "Standard directory permissions",
            ["775"] = "Shared group permissions",
            ["777"] = "Maximum standard permissions"
        };
        foreach (var pair in profiles)
        {
            Check($"Profile {pair.Key}", pair.Value, PermissionAnalyzer.Analyze(PermissionConverter.FromOctal(pair.Key)).ProfileTitle);
        }

        Check("Custom profile", "Custom permission profile", PermissionAnalyzer.Analyze(PermissionConverter.FromOctal("654")).ProfileTitle);
        Check("Preset count", 6, ApplicationDefaults.Presets.Length);
        foreach (var preset in new[] { "600", "644", "700", "750", "755", "775" })
        {
            CheckTrue($"Preset exposes {preset}", ApplicationDefaults.Presets.Contains(preset, StringComparer.Ordinal));
        }
        CheckTrue("777 is not an exposed preset", !ApplicationDefaults.Presets.Contains("777", StringComparer.Ordinal));
    }

    private static void RiskTests()
    {
        Check("Low risk", PermissionRisk.Low, Risk("600"));
        Check("Moderate others read", PermissionRisk.Moderate, Risk("604"));
        Check("Moderate others execute", PermissionRisk.Moderate, Risk("601"));
        Check("Moderate group write", PermissionRisk.Moderate, Risk("620"));
        Check("High others write", PermissionRisk.High, Risk("602"));
        Check("High others write execute", PermissionRisk.High, Risk("603"));
        Check("777 high", PermissionRisk.High, Risk("777"));
    }

    private static void CommandTests()
    {
        Check("Command without path", "chmod 755", ChmodCommandBuilder.Build("755", "", false));
        Check("Command plain path", "chmod 755 /tmp/file", ChmodCommandBuilder.Build("755", "/tmp/file", false));
        Check("Command recursive", "chmod -R 755 /tmp/folder", ChmodCommandBuilder.Build("755", "/tmp/folder", true));
        Check("Command spaced path", "chmod 755 '/tmp/My Folder'", ChmodCommandBuilder.Build("755", "/tmp/My Folder", false));
        Check("Command quoted path", "chmod 755 '/tmp/Sean'\\''s file'", ChmodCommandBuilder.Build("755", "/tmp/Sean's file", false));
        Check("Command quote without space", "chmod 755 '/tmp/Sean'\\''s'", ChmodCommandBuilder.Build("755", "/tmp/Sean's", false));
        Check("Invalid mode clears command", string.Empty, ChmodCommandBuilder.Build("888", "/tmp/file", false));
    }

    /// <summary>
    /// Adversarial coverage for the generated chmod command: every literal path must
    /// survive a POSIX shell tokenizer unchanged and must never be able to act as a
    /// separator, substitution, glob or chmod option.
    /// </summary>
    private static void CommandSafetyTests()
    {
        var literalPaths = new[]
        {
            "plain",
            "/tmp/file",
            "/tmp/My Folder",
            "/tmp/Sean's file",
            "/tmp/Sean's",
            "/tmp/a;printf_TEST",
            "/tmp/a&&b",
            "/tmp/a|b",
            "/tmp/a>b",
            "/tmp/a<b",
            "/tmp/$USER",
            "/tmp/${HOME}",
            "/tmp/$(id)",
            "/tmp/`id`",
            "/tmp/a*b",
            "/tmp/a?b",
            "/tmp/a[b]c",
            "/tmp/a\"b",
            "/tmp/a\\b",
            "/tmp/a#b",
            "/tmp/a~b",
            "/tmp/a!b",
            "/tmp/a{b}",
            "/tmp/a(b)",
            "/tmp/a\nb",
            "/tmp/a\tb",
            "/tmp/a\u0007b",
            "/tmp/Sean \"quoted\" name",
            "/tmp/space trailing",
            "  /tmp/leading",
            "--help",
            "-R",
            "-rf",
            "-",
            "--",
            "a",
            "ab",
            "a'b",
            "$HOME",
            "`id`",
            "*",
            "?"
        };

        foreach (var path in literalPaths)
        {
            AssertLiteralArgument("safe path", path);
        }

        // A path of exactly "-" is the conventional stand-in for standard input and is
        // addressed as "./-" so that chmod receives the literal file name.
        Check("Single dash path", "chmod 644 -- ./-", ChmodCommandBuilder.Build("644", "-", false));
        CheckTrue("Single dash resolved", ChmodCommandBuilder.ResolveSingleDash("-") == "./-");
        CheckTrue("Single dash resolution is narrow", ChmodCommandBuilder.ResolveSingleDash("-x") == "-x");

        // Leading dash paths must be protected by an explicit option boundary so the
        // literal path can never be parsed as a chmod option.
        foreach (var path in new[] { "-file", "--help", "-R", "--", "-rf --x" })
        {
            var command = ChmodCommandBuilder.Build("755", path, false);
            var tokens = PosixShellTokenizer.Tokenize(command);
            CheckTrue($"boundary present for {path}", tokens.Contains("--"));
            Check($"boundary keeps literal path {path}", path, tokens.Count > 0 ? tokens[^1] : string.Empty);
        }

        // A path that does not start with a dash does not need the boundary.
        Check("No boundary for ordinary path", "chmod 755 /tmp/file", ChmodCommandBuilder.Build("755", "/tmp/file", false));

        // Recursive form keeps the option in front of the mode and the boundary in
        // front of the literal path.
        Check("Recursive boundary", "chmod -R 755 -- -x", ChmodCommandBuilder.Build("755", "-x", true));
        Check("Recursive plain path", "chmod -R 755 /tmp/folder", ChmodCommandBuilder.Build("755", "/tmp/folder", true));

        // The mode is always the validated three digit octal value and is never quoted.
        Check("Mode is unquoted", "chmod 644 /tmp/file", ChmodCommandBuilder.Build("644", "/tmp/file", false));
        Check("Invalid mode still clears command", string.Empty, ChmodCommandBuilder.Build("8", "/tmp/file", false));
        Check("Empty mode clears command", string.Empty, ChmodCommandBuilder.Build("", "/tmp/file", false));
        Check("Null mode clears command", string.Empty, ChmodCommandBuilder.Build(null, "/tmp/file", false));

        // The path is trimmed before use, so a trailing separator typed by the
        // operator cannot change the generated argument.
        Check("Trailing whitespace trimmed", "chmod 755 /tmp/file", ChmodCommandBuilder.Build("755", "  /tmp/file  ", false));
    }

    /// <summary>
    /// Proves that the generated command line tokenizes back to exactly one literal
    /// path argument and that no token can introduce a shell operator.
    /// </summary>
    private static void AssertLiteralArgument(string label, string path)
    {
        var effective = path.Trim();
        if (effective.Length == 0)
        {
            return;
        }

        var expected = effective == "-" ? "./-" : effective;
        var command = ChmodCommandBuilder.Build("755", path, false);
        var tokens = PosixShellTokenizer.Tokenize(command);

        CheckTrue($"{label} [{path}] produces a command", command.Length > 0);
        CheckTrue($"{label} [{path}] starts with chmod", tokens.Count > 0 && tokens[0] == "chmod");
        CheckTrue($"{label} [{path}] keeps the mode", tokens.Contains("755"));
        Check($"{label} [{path}] round trips", expected, tokens.Count > 0 ? tokens[^1] : string.Empty);

        var operatorTokens = tokens.Count > 0 ? tokens[..^1] : tokens;
        foreach (var token in operatorTokens)
        {
            CheckTrue(
                $"{label} [{path}] token [{token}] carries no shell operator",
                !token.Any(character => character is ';' or '&' or '|' or '>' or '<' or '`' or '$' or '*' or '?' or '\n' or '\r'));
        }
    }

    private static void ParserTests()    {
        AssertParsed("Regular file", "-rw-r--r-- 1 root root 1 file", "regular file", "644");
        AssertParsed("Directory", "drwxr-xr-x 2 root root 1 dir", "directory", "755");
        AssertParsed("Symbolic link", "lrwxrwxrwx 1 root root 1 link", "symbolic link", "777");
        AssertParsed("Block device", "brw-r----- 1 root disk 8, 0 disk", "block device", "640");
        AssertParsed("Character device", "crw-rw-rw- 1 root tty 1, 3 null", "character device", "666");
        AssertParsed("Named pipe", "prw-r----- 1 user group 0 pipe", "named pipe", "640");
        AssertParsed("Socket", "srwxr-x--- 1 user group 0 socket", "socket", "750");
    }

    private static void InvalidInputTests()
    {
        CheckTrue("Reject blank octal", !PermissionConverter.TryFromOctal("", out _));
        CheckTrue("Reject short octal", !PermissionConverter.TryFromOctal("75", out _));
        CheckTrue("Reject long octal", !PermissionConverter.TryFromOctal("0755", out _));
        CheckTrue("Reject invalid digit", !PermissionConverter.TryFromOctal("758", out _));
        Check("Blank parser message", "Paste an ls -l line first.", LsPermissionParser.Parse(" ").Message);
        Check("Malformed parser message", "Could not find a standard ls -l permission field.", LsPermissionParser.Parse("not ls output").Message);
        Check("Special setuid message", "Special bits detected. V1 parses standard rwx permissions only.", LsPermissionParser.Parse("-rwsr-xr-x 1 root root 1 app").Message);
        Check("Special sticky message", "Special bits detected. V1 parses standard rwx permissions only.", LsPermissionParser.Parse("drwxrwxrwt 2 root root 1 tmp").Message);
    }

    private static void ValidationStrictnessTests()
    {
        // Whole-string validation: a trailing newline or any other control character
        // must never be accepted as part of an octal mode.
        CheckTrue("Reject trailing newline", !PermissionConverter.IsValidOctal("755\n"));
        CheckTrue("Reject trailing carriage return", !PermissionConverter.IsValidOctal("755\r"));
        CheckTrue("Reject trailing CRLF", !PermissionConverter.IsValidOctal("755\r\n"));
        CheckTrue("Reject trailing tab", !PermissionConverter.IsValidOctal("755\t"));
        CheckTrue("Reject trailing space", !PermissionConverter.IsValidOctal("755 "));
        CheckTrue("Reject leading newline", !PermissionConverter.IsValidOctal("\n755"));
        CheckTrue("Reject embedded newline", !PermissionConverter.IsValidOctal("75\n5"));
        CheckTrue("Reject embedded null", !PermissionConverter.IsValidOctal("75\u00005"));
        CheckTrue("Reject bell character", !PermissionConverter.IsValidOctal("755\u0007"));
        CheckTrue("Reject delete character", !PermissionConverter.IsValidOctal("755\u007f"));
        CheckTrue("Reject try-from-octal trailing newline", !PermissionConverter.TryFromOctal("644\n", out _));

        // The standard modes must keep working exactly as before.
        CheckTrue("Accept 000", PermissionConverter.IsValidOctal("000"));
        CheckTrue("Accept 755", PermissionConverter.IsValidOctal("755"));
        CheckTrue("Accept 777", PermissionConverter.IsValidOctal("777"));
        CheckTrue("Reject 0755", !PermissionConverter.IsValidOctal("0755"));
        CheckTrue("Reject 75", !PermissionConverter.IsValidOctal("75"));
        CheckTrue("Reject 7555", !PermissionConverter.IsValidOctal("7555"));
        CheckTrue("Reject 888", !PermissionConverter.IsValidOctal("888"));

        for (var owner = 0; owner <= 7; owner++)
        {
            for (var group = 0; group <= 7; group++)
            {
                for (var others = 0; others <= 7; others++)
                {
                    var octal = $"{owner}{group}{others}";
                    var mode = PermissionConverter.FromOctal(octal);
                    Check($"Standard mode {octal} round trips", octal, PermissionConverter.SymbolicToOctal(mode.Symbolic));
                }
            }
        }

        // Symbolic validation is whole-string as well.
        CheckTrue("Accept symbolic rwxr-xr-x", PermissionConverter.SymbolicToOctal("rwxr-xr-x") == "755");
        var symbolicRejected = false;
        try
        {
            PermissionConverter.SymbolicToOctal("rwxr-xr-x\n");
        }
        catch (ArgumentException)
        {
            symbolicRejected = true;
        }

        CheckTrue("Reject symbolic trailing newline", symbolicRejected);
    }

    private static void StartupTests()
    {
        Check("Startup octal", "755", ApplicationDefaults.OctalMode);
        var mode = PermissionConverter.FromOctal(ApplicationDefaults.OctalMode);
        CheckTrue("Startup owner full", mode.Owner is { Read: true, Write: true, Execute: true });
        CheckTrue("Startup group read execute", mode.Group is { Read: true, Write: false, Execute: true });
        CheckTrue("Startup others read execute", mode.Others is { Read: true, Write: false, Execute: true });
    }

    private static PermissionRisk Risk(string octal) => PermissionAnalyzer.DetermineRisk(PermissionConverter.FromOctal(octal));

    private static void AssertParsed(string name, string input, string type, string octal)
    {
        var result = LsPermissionParser.Parse(input);
        CheckTrue($"{name} succeeds", result.Success);
        Check($"{name} type", type, result.ObjectType);
        Check($"{name} octal", octal, result.Mode?.Octal);
    }

    private static void Check<T>(string name, T expected, T actual)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            _passed++;
            Console.WriteLine($"PASS {name}");
            return;
        }

        _failed++;
        Console.Error.WriteLine($"FAIL {name}: expected [{expected}], actual [{actual}]");
    }

    private static void CheckTrue(string name, bool condition) => Check(name, true, condition);
}
