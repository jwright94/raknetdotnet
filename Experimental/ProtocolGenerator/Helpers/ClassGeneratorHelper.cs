namespace ProtocolGenerator.Helpers
{
    internal class ClassGeneratorHelper
    {
        private static string GetVariableNameInUpperCamelCase(string variableNameInLowerCamelCase)
        {
            string firstChar = variableNameInLowerCamelCase.Substring(0, 1);
            string remains;
            if (1 < variableNameInLowerCamelCase.Length)
            {
                remains = variableNameInLowerCamelCase.Substring(1);
            }
            else
            {
                remains = "";
            }
            return firstChar.ToUpper() + remains;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="typeName"></param>
        /// <param name="variableNameInLowerCamelCase"></param>
        /// <param name="defaultValue">Can be null</param>
        /// <param name="getterAccessibility">Can be null</param>
        /// <param name="setterAccessibility">Can be null</param>
        public static void WriteProperty(ICodeWriter o, string typeName, string variableNameInLowerCamelCase, string defaultValue, string getterAccessibility, string setterAccessibility)
        {
            WritePropertyImpl(o, typeName, variableNameInLowerCamelCase, defaultValue, true, getterAccessibility, true, setterAccessibility);
        }

        public static void WriteGetProperty(ICodeWriter o, string typeName, string variableNameInLowerCamelCase, string defaultValue, string getterAccessibility)
        {
            WritePropertyImpl(o, typeName, variableNameInLowerCamelCase, defaultValue, true, getterAccessibility, false, null);
        }

        public static void WriteSetProperty(ICodeWriter o, string typeName, string variableNameInLowerCamelCase, string defaultValue, string setterAccessibility)
        {
            WritePropertyImpl(o, typeName, variableNameInLowerCamelCase, defaultValue, false, null, true, setterAccessibility);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="typeName"></param>
        /// <param name="variableNameInLowerCamelCase"></param>
        /// <param name="defaultValue">Can be null</param>
        /// <param name="doesWriteGetter"></param>
        /// <param name="getterAccessibility">Can be null</param>
        /// <param name="doesWriteSetter"></param>
        /// <param name="setterAccessibility">Can be null</param>
        private static void WritePropertyImpl(ICodeWriter o, string typeName, string variableNameInLowerCamelCase, string defaultValue, bool doesWriteGetter, string getterAccessibility, bool doesWriteSetter, string setterAccessibility)
        {
            string getterAccessibilityWithSpace = "";
            string setterAccessibilityWithSpace = "";
            string defaultValueWithAssignMark = "";

            if (getterAccessibility != null)
            {
                getterAccessibilityWithSpace = getterAccessibility + " ";
            }
            if (setterAccessibility != null)
            {
                setterAccessibilityWithSpace = setterAccessibility + " ";
            }
            if (defaultValue != null)
            {
                defaultValueWithAssignMark = " = " + defaultValue;
            }

            string variableNameInUpperCamelCase = GetVariableNameInUpperCamelCase(variableNameInLowerCamelCase);

            o.BeginBlock("public {0} {1} {{", typeName, variableNameInUpperCamelCase);
            if (doesWriteGetter)
            {
                o.WriteLine("{0}get {{ return {1}; }}", getterAccessibilityWithSpace, variableNameInLowerCamelCase);
            }
            if (doesWriteSetter)
            {
                o.WriteLine("{0}set {{ {1} = value; }}", setterAccessibilityWithSpace, variableNameInLowerCamelCase);
            }
            o.EndBlock("}");
            o.WriteLine("{0} {1}{2};", typeName, variableNameInLowerCamelCase, defaultValueWithAssignMark);
        }

        public static void WriteSingleton(ICodeWriter o, string className)
        {
            o.WriteLine("private static {0} instance = new {0}();", className);
            o.BeginBlock("public static {0} Instance {{", className);
            o.WriteLine("get { return instance; }");
            o.EndBlock("}");
            o.WriteLine("private {0}() {{ }}", className);
        }

        public static void WriteGetProperty(ICodeWriter o, string typeName, string propertyName, string value)
        {
            o.BeginBlock("public {0} {1} {{", typeName, propertyName);
            o.WriteLine("get {{ return {0}; }}", value);
            o.EndBlock("}");
        }
    }
}