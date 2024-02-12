// Copyright by Artur Wolf

namespace RpcNetGen.Internal;

using System.Reflection;
using Antlr4.Runtime.Tree;
using static RpcParser;

internal class Content
{
    private readonly HashSet<string> _knownElements = new();
    private readonly string _namespaceName;
    private readonly string _access;
    private readonly Dictionary<string, Constant> _parsedConstants = new();
    private readonly Dictionary<string, Enumeration> _parsedEnums = new();
    private readonly List<Service> _parsedServices = new();
    private readonly Dictionary<string, Struct> _parsedStructs = new();
    private readonly Dictionary<string, Union> _parsedUnions = new();

    public Content(string name, string namespaceName, string access, RpcSpecificationContext rpcSpecificationContext)
    {
        Name = name;
        ConstantClassName = $"{name}Constants";
        _namespaceName = namespaceName;
        _access = access;

        foreach (DefinitionContext definition in rpcSpecificationContext.definition())
        {
            definition.Check();
            ConstContext @const = definition.@const();
            if (@const is not null)
            {
                Add(new Constant(@const, this));
            }

            TypedefContext typedef = definition.typedef();
            if (typedef is not null)
            {
                Add(new Struct(ConstantClassName, typedef, access));
            }

            EnumContext @enum = definition.@enum();
            if (@enum is not null)
            {
                Add(new Enumeration(@enum, access, this));
            }

            StructContext @struct = definition.@struct();
            if (@struct is not null)
            {
                Add(new Struct(ConstantClassName, @struct, access));
            }

            UnionContext union = definition.union();
            if (union is not null)
            {
                Add(new Union(ConstantClassName, union, access, this));
            }
        }

        foreach (ProgramContext program in rpcSpecificationContext.program())
        {
            _parsedServices.Add(new Service(ConstantClassName, program, access, this));
        }

        foreach (Struct parsedStruct in _parsedStructs.Values)
        {
            parsedStruct.Prepare(this);
        }

        foreach (Union parsedUnion in _parsedUnions.Values)
        {
            parsedUnion.Prepare(this);
        }

        foreach (Service parsedService in _parsedServices)
        {
            parsedService.Prepare(this);
        }
    }

    public string Name { get; }
    public string ConstantClassName { get; }

    public void Add(Constant parsedConstant)
    {
        string name = parsedConstant.Name;
        if (!_knownElements.Add(name))
        {
            throw new ParserException($"'{name}' is already used.");
        }

        _parsedConstants[name] = parsedConstant;
    }

    public void Add(Enumeration parsedEnum)
    {
        string name = parsedEnum.Name;
        if (!_knownElements.Add(name))
        {
            throw new ParserException($"'{name}' is already used.");
        }

        _parsedEnums[name] = parsedEnum;
    }

    public void Add(Struct parsedStruct)
    {
        string name = parsedStruct.Name;
        if (!_knownElements.Add(name))
        {
            throw new ParserException($"'{name}' is already used.");
        }

        _parsedStructs[name] = parsedStruct;
    }

    public void Add(Union parsedUnion)
    {
        string name = parsedUnion.Name;
        if (!_knownElements.Add(name))
        {
            throw new ParserException($"'{name}' is already used.");
        }

        _parsedUnions[name] = parsedUnion;
    }

    public bool IsEnum(string identifier) => _parsedEnums.ContainsKey(identifier);

    public bool IsCustomType(string identifier) =>
        _parsedStructs.ContainsKey(identifier) || _parsedUnions.ContainsKey(identifier);

    public string GetValue(ValueContext valueContext)
    {
        if (valueContext is null)
        {
            return null;
        }

        ConstantContext constantContext = valueContext.constant();
        if (constantContext is not null)
        {
            return GetConstant(constantContext);
        }

        ITerminalNode identifier = valueContext.Identifier();
        if (identifier is not null)
        {
            return identifier.GetText();
        }

        throw new ParserException("Could not parse value.");
    }

    public string GetConstant(ConstantContext @const)
    {
        @const.Check();

        ITerminalNode decimalValue = @const.Decimal();
        if (decimalValue is not null)
        {
            return decimalValue.Symbol.Text;
        }

        ITerminalNode octalValue = @const.Octal();
        if (octalValue is not null)
        {
            return octalValue.Symbol.Text;
        }

        ITerminalNode hexadecimalValue = @const.Hexadecimal();
        if (hexadecimalValue is not null)
        {
            return hexadecimalValue.Symbol.Text;
        }

        throw new ParserException("Could not parse constant.");
    }

    public string AddConstant(string name, string value)
    {
        string fullName = $"{ConstantClassName}.{name}";
        var constant = new Constant(name, value);
        if (_parsedConstants.TryGetValue(name, out Constant existingConstant))
        {
            if (!existingConstant.Equals(constant))
            {
                throw new InvalidOperationException($"Cannot add constant {constant}. Existing constant {existingConstant} already added.");
            }
        }
        else
        {
            _parsedConstants.Add(name, constant);
        }

        return fullName;
    }

    public void Dump(XdrFileWriter writer, int indent)
    {
        writer.WriteLine(indent, "//------------------------------------------------------------------------------");
        writer.WriteLine(indent, "// <auto-generated>");
        writer.WriteLine(indent, $"//     This code was generated by RpcNetGen {GetVersion()}.");
        writer.WriteLine(indent, "//");
        writer.WriteLine(indent, "//     Changes to this file may cause incorrect behavior and will be lost if");
        writer.WriteLine(indent, "//     the code is regenerated.");
        writer.WriteLine(indent, "// </auto-generated>");
        writer.WriteLine(indent, "//------------------------------------------------------------------------------");
        writer.WriteLine();
        writer.WriteLine(indent, $"namespace {_namespaceName}");
        writer.WriteLine(indent, "{");
        writer.WriteLine(indent + 1, "using System;");
        writer.WriteLine(indent + 1, "using System.Collections.Generic;");
        writer.WriteLine(indent + 1, "using System.Net;");
        writer.WriteLine(indent + 1, "using System.Text;");
        writer.WriteLine(indent + 1, "using RpcNet;");
        writer.WriteLine();
        writer.WriteLine(indent + 1, $"{_access} static class {ConstantClassName}");
        writer.WriteLine(indent + 1, "{");
        foreach (Constant constant in _parsedConstants.Values.OrderBy(e => e.Name))
        {
            constant.Dump(writer, indent + 2);
        }

        writer.WriteLine(indent + 1, "}");

        foreach (Enumeration parsedEnum in _parsedEnums.Values.OrderBy(e => e.Name))
        {
            parsedEnum.Dump(writer, indent + 1);
        }

        foreach (Struct parsedStruct in _parsedStructs.Values.OrderBy(e => e.Name))
        {
            parsedStruct.Dump(writer, indent + 1);
        }

        foreach (Union parsedUnion in _parsedUnions.Values.OrderBy(e => e.Name))
        {
            parsedUnion.Dump(writer, indent + 1);
        }

        foreach (Service parsedService in _parsedServices)
        {
            parsedService.Dump(writer, indent + 1);
        }

        writer.WriteLine(indent, "}");
    }

    private static string GetVersion()
    {
        Version version = Assembly.GetCallingAssembly().GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }
}
