namespace ExpectedUtilityModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /*
     * Sources:
     * BdM (2002) https://www.amazon.com/Predicting-Politics-Bruce-Bueno-Mesquita/dp/0814250947
     * Sch (2011) http://journals.sagepub.com/doi/abs/10.1177/0951629811418142
     * Eft (2014) http://www.scirp.org/journal/PaperInformation.aspx?PaperID=49058
     * */
    public class Issue
    {
        /* Private fields */

        private readonly double _q;

        private double[] _c;
        private Challenge[,] _challenges;
        private double[,] _ei;
        private double[,] _ej;
        private int _n;
        private double _median;
        private double[] _newPositions;
        private double[,] _p;
        private double _positionRange;
        private double[] _r;
        private double[] _s;
        private double[,] _ubi;
        private double[,] _ubj;
        private double[,] _ufi;
        private double[,] _ufj;
        private double[,] _usi;
        private double[,] _usj;
        private double[,] _usqi;
        private double[,] _usqj;
        private double[,] _uwi;
        private double[,] _uwj;
        private double[] _weights;
        private double[] _x;

        /* Constructors */

        public Issue(IEnumerable<double> positions, IEnumerable<double> capabilities, IEnumerable<double> saliences, double statusQuoProbability)
        {
            _q = statusQuoProbability;

            Initialize(positions, capabilities, saliences);
        }

        public Issue(IEnumerable<double> positions, IEnumerable<double> capabilities, IEnumerable<double> saliences)
            : this(positions, capabilities, saliences, 0.5)
        {
            // Empty
        }

        /* Public properties */

        public double MeanPosition => CalculateMeanPosition();

        public double MedianPosition => _median;

        public double[] Positions => _x.ToArray();

        /* Public instance methods */

        // Sch (2011)---appendix
        public void SimulateRound()
        {
            ResetSecurities();

            MakePass();

            CalculateSecurities();

            MakePass();

            MakeChallenges();

            AcceptChallenges();

            _median = CalculateMedianPosition();
        }

        /* Private static methods */

        // Sch (2011)---equations 25a and 25b
        private static double CalculateExpectedUtility(double s, double us, double uf, double usq, double ub, double uw, double p, double q, double t)
        {
            return s * (p * us + (1 - p) * uf) + (1 - s) * us - q * usq - (1 - q) * (t * ub + (1 - t) * uw);
        }

        // Sch (2011)---equation 32
        private static double CalculateRisk(double actual, double minimum, double maximum)
        {
            return (2 * actual - maximum - minimum) / (maximum - minimum);
        }

        // Sch (2011)---equation 33
        private static double CalculateSecurity(double actual, double minimum, double maximum)
        {
            double risk = CalculateRisk(actual, minimum, maximum);

            return (1 - risk / 3) / (1 + risk / 3);
        }

        private static bool NearZero(double value)
        {
            const double epsilon = 1e-4;

            return Math.Abs(value) < epsilon;
        }

        /* Private instance methods */
       
        private void AcceptChallenges(int i)
        {
            IEnumerable<Challenge> challenges = Enumerable.Range(0, _n).Select(j => _challenges[i, j]).ToList();

            if (challenges.Contains(Challenge.ConfrontationMinus))
            {
                Move(i, Challenge.ConfrontationMinus);
            }
            else if (challenges.Contains(Challenge.CompromiseMinus))
            {
                Move(i, Challenge.CompromiseMinus);
            }
            else if (challenges.Contains(Challenge.CompelMinus))
            {
                Move(i, Challenge.CompelMinus);
            }
            else
            {
                _newPositions[i] = _x[i];
            }
        }

        // Sch (2015)---section 6.2
        private void AcceptChallenges()
        {
            for (int i = 0; i < _n; i++)
            {
                AcceptChallenges(i);
            }

            for (int i = 0; i < _n; i++)
            {
                _x[i] = _newPositions[i];
            }
        }

        private Challenge CalculateChallenge(int i, int j)
        {
            double ei = _ei[i, j];
            double ej = _ej[j, i];

            Challenge result;

            if (i != j)
            {
                if (NearZero(ei) && NearZero(ej))
                {
                    result = Challenge.StatusQuo;
                }
                else if (NearZero(ei))
                {
                    result = ej > 0 ? Challenge.CompromiseMinus : Challenge.StatusQuo;
                }
                else if (NearZero(ej))
                {
                    result = ei > 0 ? Challenge.CompromisePlus : Challenge.StatusQuo;
                }
                else if (ei > 0)
                {
                    if (ej > 0)
                    {
                        result = ei < ej ? Challenge.ConfrontationMinus : Challenge.ConfrontationPlus;
                    }
                    else
                    {
                        result = ei > -ej ? Challenge.CompromisePlus : Challenge.CompelPlus;
                    }
                }
                else
                {
                    if (ej < 0)
                    {
                        result = Challenge.StatusQuo;
                    }
                    else
                    {
                        result = -ei > ej ? Challenge.CompelMinus : Challenge.CompromiseMinus;
                    }
                }
            }
            else
            {
                result = Challenge.StatusQuo;
            }

            return result;
        }

        private void CalculateExpectedUtilities()
        {
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    double t = CalculateImprovementProbability(i, j);

                    // expected utility to i, if i challenges j
                    _ei[i, j] = CalculateExpectedUtility(_s[j], _usi[i, j], _ufi[i, j], _usqi[i, j], _ubi[i, j], _uwi[i, j], _p[i, j], _q, t);

                    // expected utility to j, if i challenges j
                    _ej[j, i] = CalculateExpectedUtility(_s[j], _usj[i, j], _ufj[i, j], _usqj[i, j], _ubj[i, j], _uwj[i, j], _p[j, i], _q, t);
                }
            }
        }

        // Sch (2015)---equation 16
        private double CalculateFailedChallengeUtility(int i, int j, double r)
        {
            return i != j ? 2 - 4 * Math.Pow(0.5 + 0.5 * Math.Abs((_x[i] - _x[j]) / _positionRange), r) : 0;
        }

        // Eft (2014)---section 2.6
        private double CalculateImprovementProbability(int i, int j)
        {
            return Math.Abs(_x[i] - _median) < Math.Abs(_x[i] - _x[j]) ? 1 : 0;
        }

        private double CalculateMeanPosition()
        {
            return Enumerable.Range(0, _n).Sum(i => _x[i] * _weights[i]) / _weights.Sum();
        }

        // The median voter position is the position of the player that has more votes than any of the other players
        // Sch (2014)---section 3.2
        private double CalculateMedianPosition()
        {
            double votesForMedianPosition = double.MinValue;

            double result = 0;

            for (int i = 0; i < _n; i++)
            {
                double votesForPosition = 0;

                for (int j = 0; j < _n; j++)
                {
                    for (int k = 0; k < _n; k++)
                    {
                        votesForPosition += CalculateVotes(i, j, k);
                    }
                }

                if (votesForPosition > votesForMedianPosition)
                {
                    result = _x[i];

                    votesForMedianPosition = votesForPosition;
                }
            }

            return result;
        }

        // Sch (2011)---equation 19
        private double CalculateNoChallengeDetrimentUtility(int i, int j, double r)
        {
            return i != j ? 2 - 4 * Math.Pow(0.5 + 0.25 * Math.Abs((Math.Abs(_x[i] - _median) + Math.Abs(_x[i] - _x[j])) / _positionRange), r) : 0;
        }

        // Sch (2011)---equation 18
        private double CalculateNoChallengeImprovementUtility(int i, int j, double r)
        {
            return i != j ? 2 - 4 * Math.Pow(0.5 - 0.25 * Math.Abs((Math.Abs(_x[i] - _median) + Math.Abs(_x[i] - _x[j])) / _positionRange), r) : 0;
        }

        private double CalculatePositionRange()
        {
            return _x.Max() - _x.Min();
        }

        // Sch (2011)---section 4
        private void CalculateProbabilities()
        {
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    _p[i, j] = CalculateProbability(i, j);
                }
            }
        }

        // Probability of success for player i in competition with j
        // Sch (2011)---section 4
        private double CalculateProbability(int i, int j)
        {
            double result = 0;

            if (!NearZero(_x[i] - _x[j]))
            {
                double numerator = 0;
                double denominator = 0;

                for (int k = 0; k < _n; k++)
                {
                    // BdM (2002)---page 61
                    double votes = CalculateVotes(i, j, k);
                    double ui = 1 - Math.Pow(Math.Abs(_x[k] - _x[i]), _r[i]);
                    double uj = 1 - Math.Pow(Math.Abs(_x[k] - _x[j]), _r[i]);

                    // Sch (2011)---equation 30
                    numerator += ui > uj ? votes : 0;
                    denominator += Math.Abs(votes);
                }

                result = numerator / denominator;
            }

            return result;
        }

        // Sch (2011)---section 5
        private void CalculateSecurities()
        {
            double[] actuals = CreateVector();

            for (int i = 0; i < _n; i++)
            {
                // The greater the sum (actuals[i]), the more utility *i* believes its adversaries expect to derive from challenging *i*. 
                // As this sum (risk) decreases, *i*'s relative security increases (*r* is inversely related to *R*)---so *i* is assumed to have adopted safe policies.
                actuals[i] = Enumerable.Range(0, _n).Sum(j => i != j ? _ei[j, i] : 0);
            }

            double minimum = actuals.Min();
            double maxium = actuals.Max();

            for (int i = 0; i < _n; i++)
            {
                _r[i] = !NearZero(minimum - maxium) ? CalculateSecurity(actuals[i], minimum, maxium) : 0;
            }
        }

        // Sch (2011)---equation 24
        private double CalculateStatusQuoUtility(double r)
        {
            return 2 - 4 * Math.Pow(0.5, r);
        }

        // Sch (2011)---equation 15
        private double CalculateSuccessfulChallengeUtility(int i, int j, double r)
        {
            return i != j ? 2 - 4 * Math.Pow(0.5 - 0.5 * Math.Abs((_x[i] - _x[j]) / _positionRange), r) : 0;
        }

        // Sch (2011)---section 3.1
        private void CalculateUtilities()
        {
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    _usi[i, j] = CalculateSuccessfulChallengeUtility(i, j, _r[i]);
                    _usj[i, j] = CalculateSuccessfulChallengeUtility(i, j, _r[j]);

                    _ufi[i, j] = CalculateFailedChallengeUtility(i, j, _r[i]);
                    _ufj[i, j] = CalculateFailedChallengeUtility(i, j, _r[j]);

                    _usqi[i, j] = i != j ? CalculateStatusQuoUtility(_r[i]) : 0;
                    _usqj[i, j] = i != j ? CalculateStatusQuoUtility(_r[j]) : 0;

                    _ubi[i, j] = CalculateNoChallengeImprovementUtility(i, j, _r[i]);
                    _ubj[i, j] = CalculateNoChallengeImprovementUtility(i, j, _r[j]);

                    _uwi[i, j] = CalculateNoChallengeDetrimentUtility(i, j, _r[i]);
                    _uwj[i, j] = CalculateNoChallengeDetrimentUtility(i, j, _r[j]);
                }
            }
        }

        private double CalculateVotes(int i, int j, int k)
        {
            return 2 * _weights[k] * ((Math.Abs(_x[k] - _x[j]) - Math.Abs(_x[k] - _x[i])) / _positionRange);
        }

        private double[,] CreateMatrix()
        {
            return new double[_n, _n];
        }

        private double[] CreateVector()
        {
            return new double[_n];
        }

        private void Initialize(IEnumerable<double> positions, IEnumerable<double> capabilities, IEnumerable<double> saliences)
        {
            _x = positions.ToArray();
            _c = capabilities.ToArray();
            _s = saliences.ToArray();

            _n = _x.Length;

            _r = CreateVector();

            _usi = CreateMatrix();
            _usj = CreateMatrix();
            _ufi = CreateMatrix();
            _ufj = CreateMatrix();
            _usqi = CreateMatrix();
            _usqj = CreateMatrix();
            _ubi = CreateMatrix();
            _ubj = CreateMatrix();
            _uwi = CreateMatrix();
            _uwj = CreateMatrix();

            _p = CreateMatrix();

            _ei = CreateMatrix();
            _ej = CreateMatrix();

            _challenges = new Challenge[_n, _n];

            _newPositions = CreateVector();

            _positionRange = CalculatePositionRange();

            _weights = Enumerable.Range(0, _n).Select(i => _c[i] * _s[i]).ToArray();

            _median = CalculateMedianPosition();
        }

        // Sch (2015)---section 6.1
        private void MakeChallenges()
        {
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    _challenges[i, j] = CalculateChallenge(i, j);
                }
            }
        }

        // Sch (2011)---appendix (steps 4-8)
        private void MakePass()
        {
            CalculateUtilities();

            CalculateProbabilities();

            CalculateExpectedUtilities();
        }

        // Sch (2011)---section 6
        private void Move(int i, Challenge challenge)
        {
            double minimumPositionDelta = double.MaxValue;

            double newPosition = -1;

            for (int j = 0; j < _n; j++)
            {
                if (_challenges[i, j] == challenge)
                {
                    double positionDelta;
                    double proposedPosition;

                    switch (challenge)
                    {
                        case Challenge.ConfrontationMinus:
                        case Challenge.CompelMinus:
                            positionDelta = Math.Abs(_x[i] - _x[j]);
                            proposedPosition = _x[j];

                            break;

                        case Challenge.CompromiseMinus:
                            proposedPosition = (Math.Abs(_ei[i, j]) * _x[i] + Math.Abs(_ej[j, i]) * _x[j]) / (Math.Abs(_ei[i, j]) + Math.Abs(_ej[j, i]));
                            positionDelta = Math.Abs(_x[i] - proposedPosition);

                            break;

                        default:

                            throw new Exception($"Unexpected challenge: {challenge}");
                    }

                    if (positionDelta < minimumPositionDelta)
                    {
                        minimumPositionDelta = positionDelta;

                        newPosition = proposedPosition;
                    }
                }
            }

            _newPositions[i] = newPosition;
        }

        private void ResetSecurities()
        {
            for (int i = 0; i < _n; i++)
            {
                _r[i] = 1;
            }
        }
    }
}