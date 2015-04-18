using LiveSplit.Model;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LiveSplit
{
    public class CSharpScript
    {
        protected dynamic CompiledCode { get; set; }

        public dynamic this[string name]
        {
            get
            {
                return CompiledCode[name];
            }
            set
            {
                CompiledCode[name] = value;
            }
        }

        public CSharpScript(String code)
        {
            using (var provider =
                new Microsoft.CSharp.CSharpCodeProvider())
            {
                var builder = new StringBuilder();
                builder
                    .AppendLine("using System;")
                    .AppendLine("using System.Collections.Generic;")
                    .AppendLine("using System.Linq;")
                    .AppendLine("using System.Reflection;")
                    .AppendLine("using System.Text;")
                    .AppendLine("using LiveSplit;")
                    .AppendLine("using LiveSplit.Model;")
                    .AppendLine("using LiveSplit.Web;")
                    .AppendLine("using LiveSplit.Web.Share;")
                    .AppendLine("public class CompiledScript")
                    .AppendLine("{")
                        .AppendLine("public void Respond(string message)")
                        .AppendLine("{")
                            .AppendLine("Twitch.Instance.Chat.SendMessage(message);")
                        .AppendLine("}")
                        .AppendLine("public void Execute(LiveSplitState state, TwitchChat.User user, string arguments)")
                        .AppendLine("{")
                            .Append(code)
                        .AppendLine("}")
                    .AppendLine("}");

                var parameters = new System.CodeDom.Compiler.CompilerParameters()
                {
                    GenerateInMemory = true,
                    CompilerOptions = "/optimize",
                };
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("LiveSplit.Core.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
                parameters.ReferencedAssemblies.Add("System.Drawing.dll");
                parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

                var res = provider.CompileAssemblyFromSource(parameters, builder.ToString());

                if (res.Errors.HasErrors)
                {
                    var errorMessage = "";
                    foreach (var error in res.Errors)
                    {
                        errorMessage += error + "\r\n";
                    }
                    throw new ArgumentException(errorMessage, "code");
                }

                var type = res.CompiledAssembly.GetType("CompiledScript");
                CompiledCode = Activator.CreateInstance(type);
            }
        }

        public void Run(LiveSplitState state, TwitchChat.User user, string arguments)
        {
            CompiledCode.Execute(state, user, arguments);
        }
    }
}
