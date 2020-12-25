

using System;

namespace testConsole {
    public interface IDis
    {

    }
    public interface ICalcWorker:IDis  {

        int plus (int a, int b);
        int muliply (int a, int b);

        void noReturn(int a);
    }

    public class CalcWorker : ICalcWorker
    {
        public int muliply(int a, int b)
        {
            return a*b;
        }

        public int plus(int a, int b)
        {
            return a+b;
        }

        public void noReturn(int a){
            Console.WriteLine("onreturn");
        }
    }
}