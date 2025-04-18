using System.Diagnostics;
using Cryptoquip.Extensions;
using Cryptoquip.Models;
using Cryptoquip.Services;

namespace Cryptoquip;

class Program
{
    static void Main(string[] args)
    {
        const string puzzle = "X KYP Y DSSW SF EYEEG'K BYVU LSEYG YFE YDD SB Y KHEEUF X BSHFE CGKUDB DSSWXFN YL RXC FSL YK CG EYEEG OHL YK YFSLRUZ CYF. CYGOU X'C NZSPXFN HQ Y DXLLDU. CYGOU XL PYK LRU QYXF XF RXK BYVU. OHL XL VYCU LS CU LRYL BSZ YDD LRU QDUYKHZU CSCCY YFE EYEEG BXFE XF HK VRXDEZUF, LRUZU YZU SLRUZ LXCUK XL CHKL OU RUYZLOZUYWXFN LS LRUC.";
     
        Stopwatch watch = Stopwatch.StartNew();

        Solver solver = new();
        solver.Run(LogMessage, new DecoderRingCustom(), new WordList(), puzzle, false);
        watch.Stop();
        
        LogMessage(string.Empty);
        LogMessage($"Total run time: {watch.Elapsed.ReadableTime()}");
    }
    
    private static void LogMessage(string message)
    {
        Console.WriteLine(message);
    }
}