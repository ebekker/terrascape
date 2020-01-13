using System;

namespace HC.TFPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class TFArgumentAttribute: TFAttributeAttribute
    {
        public TFArgumentAttribute(string name) : base(name)
        { }

        public new bool Optional
        {
            get => base.Optional;
            set => base.Optional = value;
        }

        public new bool Required
        {
            get => base.Required;
            set => base.Required = value;
        }

        public new bool ForceNew
        {
            get => base.ForceNew;
            set => base.ForceNew = value;
        }

        public new string[] ConflictsWith
        {
            get => base.ConflictsWith;
            set => base.ConflictsWith = value;
        }
    }
}