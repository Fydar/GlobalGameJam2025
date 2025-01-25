using System;
using System.Runtime.CompilerServices;

namespace HuskyUnity.Engineering.TypeSelector
{
    public class UseScriptIconAttribute : Attribute
    {
        public string CallerFilePath { get; }

        public UseScriptIconAttribute([CallerFilePath] string callerFilePath = "")
        {
            CallerFilePath = callerFilePath;
        }
    }
}
