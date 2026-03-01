namespace SecondTestTask;

using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;

public class Reflector
{
    public void PrintStructure(Type someClass)
    {
        ArgumentNullException.ThrowIfNull(someClass);
        var fileName = $"{ClearClassName(someClass)}.cs";
        //File.WriteAllText(fileName, GenerateCode(someClass), Encoding.UTF8);
    }

    public void DiffClasses(Type a, Type b)
    {
        if (a == null || b == null)
        {
            throw new ArgumentNullException();
        }

        if (!a.IsClass || !b.IsClass)
        {
            throw new ArgumentException("DiffClasses поддерживает только классы)");
        }

        Console.WriteLine($"---Сравнение {a.FullName} и {b.FullName} --- ");

        //DiffFields(a, b);
        //DiffMethods(a, b);
    }

    private string ClearClassName(Type T) => T.Name.Split('`')[0];


    private string GenerateTypeFile(Type type)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"using System;");
        builder.AppendLine($"using System.Collections.Generic;");

        if (!string.IsNullOrWhiteSpace(type.Namespace))
        {
            builder.AppendLine($"namespace {type.Namespace};");
            builder.AppendLine();
        }

        builder.Append(this.BuildClassCode(type, ""));
        return builder.ToString();
    }

    private string BuildClassCode(Type type, string indent)
    {
        var sb = new StringBuilder();

        var access = GetTypeAccess(type);
        var isInterface = type.IsInterface;
        var mods = new List<string> { access };
        if (type.IsAbstract && type.IsSealed)
        {
            mods.Add("static");
        }
        else if (type.IsAbstract)
        {
            mods.Add("abstract");
        }
        else if (type.IsSealed)
        {
            mods.Add("sealed");
        }

        var name = TypeNameForDeclaration(type);
        var inheritance = GetInheritanceList(type);
        sb.AppendLine($"{indent}{string.Join(" ", mods)} class {name}{inheritance}");
        sb.AppendLine($"{indent}{{");

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.Static | BindingFlags.DeclaredOnly).Where(x => !x.IsSpecialName).OrderBy(x => x.Name).ToArray();
        foreach (var f in fields)
        {
            sb.AppendLine($"{indent}    {FieldDeclaration(f)}");
        }

        if (fields.Length > 0)
        {
            sb.AppendLine();
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
           BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m=> !m.IsSpecialName).OrderBy(m => m.Name).ToArray();

        foreach (var m in methods)
        {
            sb.AppendLine($"{indent}    {MethodDeclaration(m)}");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw null;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
        }

        var nestedClasses = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(nt => nt.IsClass).OrderBy(nt => nt.Name).ToArray();
        foreach (var nt in nestedClasses)
        {
            sb.AppendLine(BuildClassCode(nt, indent + "    "));
            sb.AppendLine();
        }

        sb.AppendLine($"{indent}}}");
        return sb.ToString();
    }

    private string GetTypeAccess(Type t)
    {
        if (!t.IsNested)
        {
            return t.IsPublic ? "public" : "internal";
        }

        if (t.IsNestedPublic)
        {
            return "public";
        }

        if (t.IsNestedFamily)
        {
            return "protected";
        }

        if (t.IsNestedFamORAssem)
        {
            return "protected internal";
        }

        if (t.IsNestedAssembly)
        {
            return "internal";
        }

        return "private";
    }

    private string FieldDeclaration(FieldInfo f)
    {
        var mods = new List<string> { GetMemberAccess(f) };
        if (f.IsStatic) mods.Add("static");

        return $"{string.Join(" ", mods)} {TypeName(f.FieldType)} {f.Name}";
    }

    private string MethodDeclaration(MethodInfo m)
    {
        var mods = new List<string> { GetMemberAccess(m) };
        if (m.IsStatic)
        {
            mods.Add("static");
        }

        var ret = TypeName(m.ReturnType);

        var gen = m.IsGenericMethodDefinition
            ? $"<{string.Join(", ", m.GetGenericArguments().Select(a => a.Name))}>"
            : "";

        var pars = string.Join(", ", m.GetParameters().Select(ParameterDeclaration));

        return $"{string.Join(" ", mods)} {ret} {m.Name}{gen}({pars})";
    }

    private string ParameterDeclaration(ParameterInfo p)
    {
        var prefix = "";
        if (p.ParameterType.IsByRef)
            prefix = p.IsOut ? "out " : "ref ";

        var t = p.ParameterType.IsByRef ? p.ParameterType.GetElementType()! : p.ParameterType;
        return $"{prefix}{TypeName(t)} {p.Name}";
    }

    private string GetMemberAccess(MethodInfo m)
    {
        if (m.IsPublic)
        {
            return "public";
        }

        if (m.IsFamily)
        {
            return "protected";
        }

        if (m.IsFamilyOrAssembly)
        {
            return "protected internal";
        }

        if (m.IsAssembly)
        {
            return "internal";
        }

        return "private";
    }

    private string TypeNameForDeclaration(Type t)
    {
        var name = ClearClassName(t);

        if (t.IsGenericTypeDefinition)
        {
            var args = string.Join(", ", t.GetGenericArguments().Select(a => a.Name));
            return $"{name}<{args}>";
        }

        return name;
    }

    private string TypeName(Type t)
    {
        if (t.IsGenericParameter) return t.Name;

        if (t == typeof(void))
        {
            return "void";
        }

        if (t == typeof(int))
        {
            return "int";
        }

        if (t == typeof(string))
        {
            return "string";
        }

        if (t == typeof(bool)) return "bool";
        if (t == typeof(double)) return "double";
        if (t == typeof(float)) return "float";
        if (t == typeof(long)) return "long";
        if (t == typeof(byte)) return "byte";
        if (t == typeof(char)) return "char";
        if (t == typeof(decimal)) return "decimal";
        if (t == typeof(object)) return "object";

        if (t.IsArray)
            return TypeName(t.GetElementType()!) + "[]";

        if (t.IsGenericType)
        {
            var name = t.Name.Split('`')[0];
            var args = string.Join(", ", t.GetGenericArguments().Select(TypeName));
            return $"{name}<{args}>";
        }

        return t.Name;
    }

    private string GetInheritanceList(Type t)
    {
        var list = new List<string>();

        if (t.BaseType != null && t.BaseType != typeof(object))
            list.Add(TypeName(t.BaseType));

        list.AddRange(t.GetInterfaces().Select(TypeName));

        list = list.Distinct().ToList();
        return list.Count == 0 ? "" : " : " + string.Join(", ", list);
    }
    private string GetMemberAccess(FieldInfo f)
    {
        if (f.IsPublic)
        {
            return "public";
        }

        if (f.IsFamily)
        {
            return "protected";
        }

        if (f.IsFamilyOrAssembly)
        {
            return "protected internal";
        }

        if (f.IsAssembly)
        {
            return "internal";
        }

        return "private";
    }

}

