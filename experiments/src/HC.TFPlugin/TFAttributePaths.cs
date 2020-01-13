using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tfplugin5;

namespace HC.TFPlugin
{
    public class TFAttributePaths : IEnumerable<TFSteps>
    {
        private List<TFSteps> _paths = new List<TFSteps>();

        public IEnumerable<TFSteps> All => _paths;

        public IEnumerable<AttributePath> ToPaths() => ToPaths(_paths);

        public static IEnumerable<AttributePath> ToPaths(IEnumerable<TFSteps> steps) =>
            steps.Select(s => s.ToPath());

        public TFAttributePaths Add(TFSteps steps)
        {
            _paths.Add(steps);
            return this;
        }

        public IEnumerator<TFSteps> GetEnumerator() => _paths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _paths.GetEnumerator();
    }
}