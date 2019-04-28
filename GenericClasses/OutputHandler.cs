using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Summary description for Class1
/// </summary>
public class OutputHandler : TextWriter
{
    private static TextWriter _current;
    public StringBuilder Output { get; }

    public OutputHandler()
	{
        Output = new StringBuilder();
	}

    public override Encoding Encoding
    {
        get
        {
            return _current.Encoding;
        }
    }

    public override void WriteLine(string value)
    {
        Output.AppendLine(value);
    }
}
