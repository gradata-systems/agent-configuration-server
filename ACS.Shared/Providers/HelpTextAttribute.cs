namespace ACS.Shared.Providers
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class HelpTextAttribute : Attribute
    {
        public static string Name = "HelpText";

        public string HelpText { get; private set; }

        public HelpTextAttribute(string helpText)
        {
            HelpText = helpText;
        }
    }
}
