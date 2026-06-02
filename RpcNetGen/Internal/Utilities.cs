// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

using Antlr4.Runtime;

internal static class Utilities
{
    extension(string str)
    {
        public string ToLowerFirstLetter()
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return str[..1].ToLower() + str[1..];
        }

        public string ToUpperFirstLetter()
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return str[..1].ToUpper() + str[1..];
        }
    }

    public static void Check(this ParserRuleContext parserRuleContext)
    {
        if (parserRuleContext.exception is not null)
        {
            throw parserRuleContext.exception;
        }
    }
}
