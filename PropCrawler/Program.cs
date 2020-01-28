using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static List<string> _existingTypes = new List<string>();
        static void Main(string[] args)
        {   
            var file = new FileInfo("C:\\temp\\PropGraph.txt");

            using (var sw = file.AppendText())
            {
                BuildClass(typeof(HttpResponseMessage), sw); // use whatever type you want to crawl here

                sw.Close();
            }
        }

        static void BuildClass(Type classType, StreamWriter writer)
        {
            if (classType.IsGenericType)
                classType = classType.GetGenericArguments().First();

            if (_existingTypes.Contains(classType.Name))
                return;
            else
                _existingTypes.Add(classType.Name);

            BeginClass(classType.Name, writer);

            var publicProps = classType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in publicProps)
            {
                if(prop.CanWrite && prop.CanRead)
                    WriteProperty(prop.PropertyType, prop.Name, writer);
            }

            EndClass(writer);

            foreach (var prop in publicProps)
            {
                if (!prop.PropertyType.IsSimpleType())
                {
                    writer.WriteLine();
                    BuildClass(prop.PropertyType, writer);
                }

                if(prop.PropertyType.IsEnum)
                {
                    writer.WriteLine();
                    BuildEnum(prop.PropertyType, writer);
                }
            }
        }

        static void BeginClass(string className, StreamWriter writer)
        {
            if (className.EndsWith("Obj"))
                className = className.Replace("Obj", "Dto");

            writer.WriteLine($"public class {className}");
            writer.WriteLine("{");
        }

        static void WriteProperty(Type type, string propName, StreamWriter writer)
        {
            var typeName = type.Name.EndsWith("Obj") ? type.Name.Replace("Obj", "Dto") : type.Name;
            if (type.IsGenericType)
            {
                var genArg = type.GetGenericArguments().First();
                var genName = genArg.Name.EndsWith("Obj") ? genArg.Name.Replace("Obj", "Dto") : genArg.Name;

                writer.WriteLine($"\t public {typeName.Replace("`1", string.Empty)}<{genName}> {propName} {{ get; set; }}");
            }
            else
            {
                writer.WriteLine($"\t public {typeName} {propName} {{ get; set; }}");
            }
        }

        static void EndClass(StreamWriter writer)
        {
            writer.WriteLine("}");
        }

        static void BuildEnum(Type enumType, StreamWriter writer)
        {
            var vals = Enum.GetNames(enumType);
            BeginEnum(enumType.Name, writer);
            foreach(var val in vals)
            {
                writer.WriteLine($"\t {val},");
            }
            EndClass(writer);
        }

        static void BeginEnum(string enumName, StreamWriter writer)
        {
            writer.WriteLine($"public enum {enumName}");
            writer.WriteLine("{");
        }
    }
}
