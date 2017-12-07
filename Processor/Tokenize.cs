using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Processor
{
    public class Tokenize
    {
        // put this in config
        // only for Go, Java, Javascript, C#, F#(some), PHP, C++, C, Python
        private List<string> reservedWords = new List<string>{ "BEGIN", "END", "\\case", "_ _FILE_ _", "_ _LINE __", "__CLASS__", "__DIR__", "__FILE__",
            "__FUNCTION__", "__LINE__", "__METHOD__", "__NAMESPACE__", "__TRAIT__", "__halt_compiler()", "abstract", "alias", "alignas", "alignof", "and",
            "and_eq", "array()", "as", "asm", "asr", "assert", "atomic", "atomic_cancel", "atomic_commit", "atomic_noexcept", "auto", "base", "begin", "bitand",
            "bitor", "bool", "boolean", "break", "break", "byte", "callable", "case", "case", "catch", "chan", "char", "char16_t", "char32_t", "checked", "class",
            "clone", "co_await", "co_return", "co_yield", "compl", "component", "concept", "const", "const", "const_cast", "constexpr", "constraint", "constructor",
            "continue", "continue", "decimal", "declare", "decltype", "def", "default", "default", "defer", "defined?", "del", "delegate", "delete", "die()", "do",
            "double", "dynamic_cast", "eager", "echo", "elif", "else", "else", "elseif", "elsif", "empty()", "end", "enddeclare", "endfor", "endforeach", "endif",
            "endswitch", "endwhile", "ensure", "enum", "eval()", "event", "except", "exec", "exit()", "explicit", "export", "extends", "extern", "external", "fallthrough",
            "false", "final", "finally", "finally", "fixed", "float", "for", "for", "foreach", "friend", "from", "func", "function", "functor", "global", "go", "goto",
            "goto", "goto", "if", "if", "implements", "implicit", "import", "import", "in", "include", "include_once", "inline", "instanceof", "insteadof", "int",
            "interface", "interface", "internal", "is", "isset()", "lambda", "land", "list()", "lock", "long", "lor", "lsl", "lsr", "lxor", "map", "method", "mixin",
            "mod", "module", "mutable", "namespace", "namespace", "native", "new", "next", "nil", "noexcept", "not", "not_eq", "null", "nullptr", "object", "operator",
            "or", "or_eq", "out", "override", "package", "package", "parallel", "params", "pass", "print", "private", "process", "protected", "public", "pure", "raise",
            "range", "readonly", "redo", "ref", "register", "reinterpret_cast", "require", "require_once", "requires", "rescue", "retry", "return", "return", "sbyte",
            "sealed", "select", "self", "short", "sig", "signed", "sizeof", "stackalloc", "static", "static_assert", "static_cast", "strictfp", "string", "struct", "super",
            "switch", "synchronized", "synchronized", "tailcall", "template", "then", "this", "thread_local", "throw", "throws", "trait", "trait", "transient", "true",
            "try", "type", "typedef", "typeid", "typename", "typeof", "uint", "ulong", "unchecked", "undef", "union", "unless", "unsafe", "unset()", "unsigned", "until",
            "use", "ushort", "using", "usingstatic", "var", "virtual", "void", "volatile", "wchar_t", "when", "while", "xor", "xor_eq", "yield", "yield", "let",
            "debugger", "with", "alert", "frames", "outerHeight", "all", "frameRate", "outerWidth", "anchor", "packages", "anchors", "getClass", "pageXOffset",
            "area", "hasOwnProperty", "pageYOffset", "Array", "hidden", "parent", "assign", "history", "parseFloat", "blur", "image", "parseInt", "button", "images",
            "password", "checkbox", "Infinity", "pkcs11", "clearInterval", "isFinite", "plugin", "clearTimeout", "isNaN", "prompt", "clientInformation", "isPrototypeOf",
            "propertyIsEnum", "close", "java", "prototype", "closed", "JavaArray", "radio", "confirm", "JavaClass", "reset", "JavaObject", "screenX", "crypto",
            "JavaPackage", "screenY", "Date", "innerHeight", "scroll", "decodeURI", "innerWidth", "secure", "decodeURIComponent", "layer", "defaultStatus", "layers",
            "document", "length", "setInterval", "element", "link", "setTimeout", "elements", "location", "status", "embed", "Math", "String", "embeds", "mimeTypes",
            "submit", "encodeURI", "name", "taint", "encodeURIComponent", "NaN", "text", "escape", "navigate", "textarea", "eval", "navigator", "top", "Number",
            "toString", "fileUpload", "Object", "undefined", "focus", "offscreenBuffering", "unescape", "form", "open", "untaint", "forms", "opener", "valueOf", "frame",
            "option", "window" };

        public string Clean(string input)
        {
            input = RemoveSubtraction(input);
            input = RemoveComments(input);
            input = RemovePlainText(input);
            input = RemoveReservedWords(input);
            input = RemoveMisc(input);
            return input;
        }

        private string RemoveSubtraction(string input)
        {
            input = Regex.Replace(input, "((\\n-.*?\\n)|\\n\\+)", "", RegexOptions.IgnoreCase);
            return input;
        }

        private string RemoveReservedWords(string input)
        {
            input = Regex.Replace(input, "\\b" + string.Join("\\b|\\b", reservedWords) + "\\b", "", RegexOptions.IgnoreCase);
            return input;
        }

        private string RemovePlainText(string input)
        {
            input = Regex.Replace(input, "((\".*?\")|\'.*?\')", "", RegexOptions.IgnoreCase);
            return input;
        }

        private string RemoveMisc(string input)
        {
            input = Regex.Replace(input, "(=|,|[0-9]|\\(|\\)|;|@|\\.|\\-|\\+|\\*)", "", RegexOptions.IgnoreCase);
            return input;
        }

        private string RemoveComments(string input)
        {
            input = Regex.Replace(input, "(\\\\.*?\\n|\\\\*.*?\\*\\\\)", "", RegexOptions.IgnoreCase); // might not work
            return input;
        }
    }
}
