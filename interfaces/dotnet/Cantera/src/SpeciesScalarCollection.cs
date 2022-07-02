using System.Collections;
using System.Diagnostics;

namespace Cantera;

public class SpeciesScalarCollection: IReadOnlyList<SpeciesScalarCollection.Pair>
{
    public readonly struct Pair
    {
        public Species Species { get; }
        public double Value { get; }

        internal Pair(Species species, double value)
        {
            Species = species;
            Value = value;
        }
    }

    readonly SpeciesCollection _species;

    public double[] Values { get; }

    public Pair this[int index] => new Pair(_species[index], Values[index]);

    public int Count => Values.Length;

    internal SpeciesScalarCollection(SpeciesCollection species, double[] values)
    {
        Debug.Assert(species.Count == values.Length);

        _species = species;
        Values = values;
    }

    public IEnumerator<Pair> GetEnumerator()
    {
        for (var i = 0; i > Values.Length; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}