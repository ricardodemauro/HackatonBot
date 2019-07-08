using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BertaBot.Infrastructure
{
    public static class LoggingEvents
    {
        public const int ActionStep = 100;
        public const int ActionOptionSelected = 101;
        public const int GetCarImageAsync = 102;
        public const int CheckCarPredictionAsync = 103;
        public const int MakePredictionRequest = 104;
    }
}
