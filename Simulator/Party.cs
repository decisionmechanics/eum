namespace Simulator
{
    public class Party
    {
        /* Constructors */

        public Party(string name, double position, double capability, double salience)
        {
            Name = name;
            Position = position;
            Capability = capability;
            Salience = salience;
        }

        /* Public properties */

        public double Capability { get; }

        public string Name { get; }

        public double Position { get; }

        public double Salience { get; }
    }
}