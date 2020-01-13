using System.Collections.Generic;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types;

namespace Terraform.Plugin
{
    public class TFSteps
    {
        private List<Step> _steps = new List<Step>();

        public AttributePath ToPath() => new AttributePath { Steps = { _steps } };

        public TFSteps Attribute(string name) => Add(new Step { AttributeName = name });

        public TFSteps Element(long index) => Add(new Step { ElementKeyInt = index });

        public TFSteps Element(string name) => Add(new Step { ElementKeyString = name });

        protected TFSteps Add(Step step)
        {
            _steps.Add(step);
            return this;
        }
    }
}