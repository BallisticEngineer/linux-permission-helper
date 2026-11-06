using System.Text;

namespace EngineerForge.LinuxPermissionHelper.Tests;

/// <summary>
/// Minimal POSIX shell word tokenizer used to prove that a generated command line
/// yields exactly the literal argument the operator typed. It implements the
/// quoting rules of POSIX sh (single quotes, double quotes and backslash escapes)
/// and deliberately performs no expansion at all: no parameter expansion, no
/// command substitution, no arithmetic expansion, no tilde expansion and no
/// pathname (glob) expansion.
/// </summary>
internal static class PosixShellTokenizer
{
    public static List<string> Tokenize(string command)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var started = false;
        var index = 0;

        while (index < command.Length)
        {
            var currentChar = command[index];

            if (currentChar is ' ' or '\t' or '\n' or '\r')
            {
                if (started)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                    started = false;
                }

                index++;
                continue;
            }

            started = true;

            if (currentChar == '\'')
            {
                index++;
                while (index < command.Length && command[index] != '\'')
                {
                    current.Append(command[index]);
                    index++;
                }

                index++;
                continue;
            }

            if (currentChar == '"')
            {
                index++;
                while (index < command.Length && command[index] != '"')
                {
                    if (command[index] == '\\' && index + 1 < command.Length &&
                        command[index + 1] is '"' or '\\' or '$' or '`' or '\n')
                    {
                        index++;
                        if (command[index] == '\n')
                        {
                            index++;
                            continue;
                        }
                    }

                    current.Append(command[index]);
                    index++;
                }

                index++;
                continue;
            }

            if (currentChar == '\\' && index + 1 < command.Length)
            {
                index++;
                current.Append(command[index]);
                index++;
                continue;
            }

            current.Append(currentChar);
            index++;
        }

        if (started)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
