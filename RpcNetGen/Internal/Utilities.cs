// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

using Antlr4.Runtime;

internal static class Utilities
{
    public static string ToLowerFirstLetter(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        return str[..1].ToLower() + str[1..];
    }

    public static string ToUpperFirstLetter(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        return str[..1].ToUpper() + str[1..];
    }

    public static void Check(this ParserRuleContext parserRuleContext)
    {
        if (parserRuleContext.exception != null)
        {
            throw parserRuleContext.exception;
        }
    }
}
