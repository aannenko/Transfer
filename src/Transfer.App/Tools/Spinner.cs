using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transfer.App.Tools
{
    internal class Spinner
    {
        private static readonly string[] _defaultSequence = new[]
        {
            ".oO", " .oO", "  .oO", "   .oO", "   .Oo",
            "   Oo.", "  Oo.", " Oo.", "Oo.", "oO."
        };

        private static readonly TimeSpan _minDelay = TimeSpan.FromMilliseconds(25);
        private static readonly TimeSpan _defaultDelay = TimeSpan.FromMilliseconds(100);

        private readonly string[] _sequence;
        private readonly string[] _emptyLineParts;
        private readonly TimeSpan _delay;

        private Spinner(IEnumerable<string> sequence, TimeSpan delay)
        {
            _sequence = sequence == null || !sequence.Any()
                ? _defaultSequence
                : sequence.ToArray();

            _emptyLineParts = new string[_sequence.Length];
            var maxSeqLength = _sequence.Max(s => s.Length);
            for (int i = 0; i < _sequence.Length; i++)
            {
                var lengthDiff = maxSeqLength - _sequence[i].Length;
                _emptyLineParts[i] = lengthDiff > 0
                    ? new string(' ', lengthDiff)
                    : string.Empty;
            }

            _delay = delay > _minDelay
                ? delay
                : _minDelay;
        }

        public static Task<Spinner> GetSpinnerAsync(IEnumerable<string> sequence, TimeSpan delay) =>
            Task.Run(() => new Spinner(sequence, delay));

        public static Task<Spinner> GetSpinnerAsync(params string[] sequence) =>
            GetSpinnerAsync(sequence, _defaultDelay);

        public async Task SpinUntilAsync(Task task)
        {
            for (int i = 0; i < _sequence.Length; i++)
            {
                if (task.IsCompleted)
                    return;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{_sequence[i] + _emptyLineParts[i]}\r");
                await Task.Delay(_delay);

                if (i == _sequence.Length - 1)
                    i = -1;
            }
        }
    }
}