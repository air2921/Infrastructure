using System.Text;

namespace Infrastructure.Builders;

public class HtmlBuilder(int size = 256)
{
    private readonly StringBuilder _builder = new(size);
    private readonly Stack<string> _tagStack = new();
    private int _indentLevel = 0;

    public void AppendTag(string tag, string text)
    {
        AddIndent();
        _builder.Append($"<{tag}>{text}</{tag}>\n");
    }

    public void AppendEmptyTag(string tag)
    {
        AddIndent();
        _builder.Append($"<{tag}></{tag}>\n");
    }

    public void OpenTag(string tag)
    {
        AddIndent();
        _builder.Append($"<{tag}>\n");

        _indentLevel++;
        _tagStack.Push(tag);
    }

    public void CloseTag()
    {
        if (_tagStack.Count > 0)
        {
            _indentLevel--;

            AddIndent();
            var tag = _tagStack.Pop();
            _builder.Append($"</{tag}>\n");
        }
    }

    public string ToHtml()
    {
        while (_tagStack.Count > 0)
            CloseTag();

        return _builder.ToString();
    }

    private void AddIndent()
    {
        for (int i = 0; i < _indentLevel; i++)
            _builder.Append("    ");
    }
}
