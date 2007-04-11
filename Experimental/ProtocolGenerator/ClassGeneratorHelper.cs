namespace ProtocolGenerator
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
            o.WriteLine("{0}get {{ return {1}; }}", getterAccessibilityWithSpace, variableNameInLowerCamelCase);
            o.WriteLine("{0}set {{ {1} = value; }}", setterAccessibilityWithSpace, variableNameInLowerCamelCase);
            o.EndBlock("}");
            o.WriteLine("{0} {1}{2};", typeName, variableNameInLowerCamelCase, defaultValueWithAssignMark);
        }
    }
}